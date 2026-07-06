# ActionSystem Redesign Plan

> Status: **approved, not yet implemented** (planned 2026-07-06). This document is the agreed
> design for the ActionSystem/EventSystem rewrite. It supersedes the "Known issues" list in
> [action-system.md](action-system.md) and roadmap items 3 and 6 in
> [findings-and-improvements.md](findings-and-improvements.md).

## Context

The action system (`Assets/Scripts/ActionSystem/`) is the WIP core of the card-game engine: every
model change flows through a `GameAction` with Prepare/Perform phases, reactions, and
post-resolution events. A full review (2026-07-06) confirmed the documented known issues and found
additional faults:

- **Silent losses everywhere**: root actions submitted while a flow is active are dropped;
  reactions added outside a phase window go to `null` or a stale already-processed list
  (`_reactionsToResolve` is never restored after recursion).
- **Cancellation incoherent**: `PostCanceled` never invoked; canceled actions still post
  `FlowCompleted`/`Completed` as if they succeeded; no defined point where cancellation is legal.
- **Reflection on every event post**: `GameActionEventTypes` caches `MethodInfo` but still calls
  `Activator.CreateInstance` + `Invoke` per event (~6 per action).
- **Exception = permanent deadlock**: a throwing handler leaves `_rootFlow` non-null; `IsActive`
  stays true; all later actions silently dropped.
- **Sub-steps vs reactions conflated**: intentional composition ("deal damage, then discard
  energy") and triggered reactions share one priority-sorted list — no guaranteed ordering for an
  action's own sub-steps.
- **No causality**: reactions have no parent/source link; tests identify the root via
  `OrderOfPlay == 0`.
- **Global statics**: `NotificationEventSystem` singleton (tests reset `_instance` via
  reflection), static `OrderOfPlayCounter` — blocks parallel matches (AI lookahead, replay).
- **Viewer wiring backwards**: mutable public `Viewer` field patched onto in-flight actions from
  `FlowStarted` handlers; nested pumping discards `Current` so viewers can't yield real Unity
  yield instructions.
- Assorted: `GameAction : EventData` but never posted (caused the original MatchSystem bug);
  `IGameAction.Priority/OrderOfPlay` publicly settable; `as PostResolutionEvent` cast can post
  null; SHA1-int hashing adds collision risk over plain `Type` keys for no current benefit;
  `PredefinedAssemblyUtility` preload breaks silently once asmdefs return.

## Decisions

1. **Instant logic + playback queue.** Game logic resolves synchronously and deterministically,
   emitting an ordered event log; a future presentation layer consumes the log at its own pace
   (the Hearthstone/MTG Arena model). Viewers as a logic-gating concept disappear.
2. **Root action queue + FIFO reactions.** `PerformGameAction` enqueues (never drops); the system
   drains the queue. Reactions stay priority-desc → order-added, depth-first. Sorter stays
   injectable (`IActionSystemSorter`).
3. **CRTP events.** `class DrawCardAction : GameAction<DrawCardAction>` — the base class creates
   `GameActionPerformed<TSelf>` etc. directly, compile-time, zero reflection. The reflection
   pipeline is deleted. The subscriber-facing API is unchanged.
4. **Per-container event bus.** `NotificationEventSystem` becomes an instance aspect on the
   `Container`; the `OrderOfPlay` counter becomes per-`ActionSystem`. Static `TypeRegistry`
   type-metadata caches may stay global (pure type metadata).
5. **Model mutation lives in `OnPerform`.** The action is the single authoritative mutation;
   events become pure notifications for reactions/view/logging. The rule:
   *systems submit actions and react to events; only actions mutate the model.*

## Design

### GameAction (rewrite `GameAction.cs`, trim `IGameAction.cs`)

```csharp
public enum GameActionState { Pending, Validating, Preparing, Performing, Completed, Canceled, Faulted }

public abstract class GameAction : IGameAction {           // no longer extends EventData
    public int Priority { get; set; }                      // set at creation, pre-submission
    public object Source { get; set; }                     // card/effect/player that caused it
    public int OrderOfPlay { get; internal set; }          // assigned by ActionSystem at enqueue
    public GameAction Parent { get; internal set; }        // causality; null == root
    public int Depth { get; internal set; }
    public GameActionState State { get; internal set; }
    public bool IsCanceled { get; }
    public string CancelReason { get; }

    public void Cancel(string reason = null);              // legal only before the Perform phase; throws otherwise

    protected internal virtual bool OnValidate(IContainer game) => true;  // legality; false => Canceled
    protected internal virtual void OnPrepare(IContainer game) { }        // "about to happen, modify me" window
    protected internal abstract void OnPerform(IContainer game);          // the authoritative model mutation

    protected void EnqueueChild(GameAction child);         // sub-steps, run in order after Perform
}

public abstract class GameAction<TSelf> : GameAction where TSelf : GameAction<TSelf> {
    // internal factory overrides create GameActionPrepared<TSelf>, GameActionPerformed<TSelf>,
    // GameActionCanceled<TSelf>, GameActionCompleted<TSelf>, GameActionFaulted<TSelf> directly —
    // zero reflection. ActionSystem calls them and both logs and dispatches the events.
}
```

- Fixed phases **Validate → Prepare → Perform** replace `List<GameActionPhase>`; delete
  `GameActionPhase.cs`, `GameActionPhaseType`, and viewers.
- `Cancel(reason)` is legal in Pending/Validating/Preparing; a canceled action posts
  `GameActionCanceled<TSelf>` and does **not** post Performed/Completed.
- Event set collapses to **Prepared / Performed / Canceled / Completed** plus new **Faulted**
  (carries the exception). `FlowStarted`/`FlowCompleted` are removed — Completed fires after the
  action's children and reactions finish, which is what FlowCompleted meant.
- Keep `GameActionEvents.cs` (generic wrappers + `IGameAction*Event` interfaces +
  `PostResolutionEvent`) and `GameActionEventSubscriptionFactory.cs` — same API minus the Flow*
  methods, plus Faulted. Fix the two `</summary` typos.
- **Delete**: `GameActionEventTypes.cs`, `GameActionTypeRegistry.cs` (registry, preload, editor
  reset — all obsolete under CRTP).

### ActionSystem (rewrite `ActionSystem.cs`)

- **Root queue with auto-drain**: `PerformGameAction` enqueues and, if not already resolving,
  drains synchronously before returning. Re-entrant submissions (from handlers) enqueue and are
  drained by the ongoing loop. `Update()` stays as a drain entry point for the future frame
  driver; tests never pump frames.
- **Reaction windows as an explicit stack**: a fresh window is pushed around each Prepared /
  Performed / post-resolution dispatch and popped after (fixes the stale-list bug).
  `AddReaction` outside any window **throws** with a message pointing at `PerformGameAction`.
  Reactions get `Parent`, `Depth`, `OrderOfPlay` assigned when added. Canceled / Completed /
  Faulted events do *not* open windows — responses to those are new root actions (safe now that
  the queue never drops).
- **Sub-steps separate from reactions**: children enqueued via `EnqueueChild` run in submission
  order (unsorted) immediately after the Perform phase, *before* sorted reactions. This is the
  mechanism for "complicated attack = chained actions".
- **Error handling**: any handler exception marks the action `Faulted`, posts
  `GameActionFaulted<TSelf>`, aborts the current root resolution (windows cleared, remaining
  reactions dropped), logs, and continues with the next queued root. The system always stays
  usable.
- **Post-resolution events**: keep registration, add `Unregister`; cap the `Repeats` loop
  (16 iterations, then error log + break). Registration requires the event to derive from
  `EventData` (fixes the silent `as PostResolutionEvent` null-post).
- **Resolution event log**: every event posted during a root resolution is appended, in dispatch
  order, to `IReadOnlyList<EventData> LastResolutionLog` (cleared per root). This is the contract
  the future presentation/playback layer consumes.
- **Depth guard**: max reaction/child depth 64 → Faulted; protects against infinite chains.
- `OrderOfPlay` counter is a per-instance `ManualCounter`; sorter injected via constructor
  (`ActionSystem(IActionSystemSorter)`, default FIFO).

Resolution flow per action:

```
Validate:  State=Validating; OnValidate(); false or Cancel() → post Canceled, stop.
Prepare:   State=Preparing; OnPrepare(); post Prepared (reaction window);
           resolve prepare-reactions (sorted); Cancel() here → post Canceled, stop.
Perform:   State=Performing; OnPerform() — the model mutation; post Performed (reaction window);
           resolve children in submission order; resolve perform-reactions (sorted).
Complete:  State=Completed; post Completed (no window).
```

Player mid-effect choices are **out of scope** for this pass: inputs are gathered before an
action is submitted. A future choice-request pattern (effect completes and requests input, input
arrives as a new root action) fits the queue model and is noted here for the attack/trainer work.

### Event system (`NotificationEventSystem.cs`, `EventSubscription.cs`, `TypeRegistry.cs`)

- `NotificationEventSystem` becomes an instance class implementing `IAspect`, registered per
  container (`container.AddAspect<NotificationEventSystem>()`); the static
  `Subscribe/PostEvent/...` API is removed. Add a `container.GetEventSystem()` extension.
- **Type keys instead of SHA1 ints**: subscriptions stored in
  `Dictionary<Type, List<...>>`; `IEventSubscription.TypeHash` → `Type SubscriptionType`.
  `TypeRegistry` returns a deterministic `IReadOnlyList<Type>` (self, base chain, then interfaces
  sorted by full name); SHA1 hashing and the preload step leave the event path entirely.
  `EventData.TypeHashes` → `EventData.EventTypes`.
- **Dispatch**: iterate the *event's* type list (small) with dictionary lookups — no LINQ over
  all buckets, no per-dispatch scan. Collect matches, then sort once by **priority desc, then
  registration order** (global, stable — fixes cross-bucket priority inversion). Registration
  order comes from a per-bus sequence number stored with each entry. Handlers unsubscribed
  mid-dispatch are skipped (active-set check).
- Drop `EventSubscription.Equals/GetHashCode` overrides (two subscriptions wrapping the same
  handler are distinct registrations; reference equality).
- Keep the `PostEvent` queue + `Flush()` (state machine uses it).

### Consumers

- **`GameplayAspect` / `GameplaySystem` lifecycle** (roadmap item 6): `Subscribe()` only
  *tracks*; registration happens in `SubscribeAll()`. `GameplaySystem.Awake()` becomes
  `RegisterSubscriptions(); SubscribeAll();` — subclasses override the new
  `protected virtual void RegisterSubscriptions()` instead of `Awake()`. Double-registration is
  additionally prevented by the bus's active-set.
- **`ChangeTurnAction`**: `: GameAction<ChangeTurnAction>`; `OnValidate` checks the player index;
  `OnPerform` sets `match.CurrentPlayerIndex`. `MatchSystem.OnChangeTurnPerformed` is deleted —
  MatchSystem just submits the action.
- **`GameDataSystem.Awake`** calls `base.Awake()` after creating the `Match`.
- **`StateMachine`** gets an optional `NotificationEventSystem` constructor parameter and
  null-guards its two `PostEvent` calls (it currently posts into the global singleton, which is
  going away).
- **`IObserve`**: unchanged shape; remove the stray `using NUnit.Framework;`.

### Serialization / content authoring (direction only — no implementation this pass)

Actions become data-thin runtime commands (parameters + phase overrides, no delegate lists),
which is the prerequisite for: save/replay as a log of root actions, and card content authored as
**JSON effect definitions** parsed into an effect AST (primitives like DealDamage / FlipCoin /
DiscardEnergy plus sequence/conditional combinators) that expand into chained `GameAction`s at
play time (roadmap item 2). A Unity editor tool later is just a writer of that JSON.

## Files to change

| File | Change |
|---|---|
| `Assets/Scripts/ActionSystem/GameAction.cs` | Rewrite: CRTP, no EventData base, fixed Validate/Prepare/Perform, state + parent/source, EnqueueChild |
| `Assets/Scripts/ActionSystem/ActionSystem.cs` | Rewrite: root queue + auto-drain, synchronous resolve, reaction-window stack, children vs reactions, fault handling, event log, per-instance counter, injectable sorter |
| `Assets/Scripts/ActionSystem/GameActionPhase.cs` | Delete (viewers/phase objects gone) |
| `Assets/Scripts/ActionSystem/GameActionEventTypes.cs`, `GameActionTypeRegistry.cs` | Delete |
| `Assets/Scripts/ActionSystem/GameActionEvents.cs` | Drop Flow* events; add `GameActionFaulted`; fix typos |
| `Assets/Scripts/ActionSystem/GameActionEventSubscriptionFactory.cs` | Same API minus Flow*, plus Faulted |
| `Assets/Scripts/ActionSystem/IGameAction.cs`, `ActionSystemSorter.cs` | Read-only interface (no setters); sorter injectable |
| `Assets/Scripts/ActionSystem/GameActions/ChangeTurnAction.cs` | `: GameAction<ChangeTurnAction>`; mutation in `OnPerform`; validation in `OnValidate` |
| `Assets/Scripts/NotificationEventSystem/*` | Instance bus (IAspect), Type keys, global stable priority sort, low-alloc dispatch |
| `Assets/Scripts/Utility/TypeRegistry.cs` | Deterministic `IReadOnlyList<Type>`; SHA1 + preload removed from event path |
| `Assets/Scripts/GameplaySystems/GameplayAspect.cs`, `GameplaySystem.cs`, `GameDataSystem.cs`, `MatchSystem.cs` | Container-scoped bus; RegisterSubscriptions lifecycle; MatchSystem handler removed |
| `Assets/Scripts/StateMachine/StateMachine.cs` | Optional bus reference instead of global singleton |
| `Assets/Tests/ActionSystemTests.cs`, `MatchSystemTests.cs` | Rewrite: no frame pumping, no reflection singleton reset; new coverage (below) |
| `docs/action-system.md`, `docs/event-system.md`, `docs/findings-and-improvements.md`, `CLAUDE.md` | Update to the new design; close roadmap items 3, 6 (and the dispatch part of 8) |

## Verification

1. Unity edit-mode tests (Test Runner or `Unity.exe -runTests -testPlatform EditMode`) — all
   rewritten `ActionSystemTests` + `MatchSystemTests` green. Unity 6000.0.43f1 may need to be
   installed first.
2. New tests proving each fixed fault:
   - a second root submitted from a handler queues and resolves after the first (not dropped);
   - `AddReaction` from a Completed handler / outside resolution throws (not silently lost);
   - canceled action posts Canceled with reason and never Performed/Completed;
   - `Cancel()` during Perform throws;
   - a throwing `OnPerform` posts Faulted and the next root still resolves (no deadlock);
   - children resolve in submission order before reactions; reactions sort by priority then order;
   - reaction chains carry `Parent`/`Depth`; depth cap faults instead of hanging;
   - repeating post-resolution events stop at the cap;
   - `LastResolutionLog` contains the expected ordered event sequence;
   - two containers with separate buses don't cross-talk.
3. Grep-level checks: no `Activator.CreateInstance`/`MakeGenericType` under
   `Assets/Scripts/ActionSystem`; no `NotificationEventSystem.Instance`; no reflection in test
   SetUp.
