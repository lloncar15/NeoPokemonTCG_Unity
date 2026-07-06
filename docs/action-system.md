# Action System

> ⚠️ **Status: work in progress.** This documents the system as it exists today. A collaborative redesign is planned; the "Known issues" section at the bottom is the working list for that session.

Every change to the game model is supposed to flow through a `GameAction` executed by the `ActionSystem` aspect. Systems never mutate the `Match` directly; they submit actions and react to the events those actions emit.

## GameAction lifecycle

`GameAction` (abstract, extends `EventData`) is created with two fixed phases:

```
PerformGameAction(action)
└─ GameActionFlow
   ├─ PostFlowStarted            → GameActionFlowStarted<TAction>
   ├─ Prepare phase
   │   ├─ OnPrepare(container)   → GameActionPrepared<TAction>
   │   └─ resolve reactions collected during the phase (sorted, recursive)
   ├─ Perform phase
   │   ├─ OnPerform(container)   → GameActionPerformed<TAction>
   │   └─ resolve reactions
   ├─ post-resolution events (root action only; may repeat while they
   │   generate reactions and Repeats == true)
   ├─ PostFlowCompleted          → GameActionFlowCompleted<TAction>
   └─ (from ActionSystem.Update, when the root flow ends)
       PostCompleted             → GameActionCompleted<TAction>
```

Key properties: `Priority` (higher resolves first among queued reactions), `OrderOfPlay` (monotonic tiebreaker from the static `ActionSystem.OrderOfPlayCounter`), `IsCanceled` (checked at the start of each phase flow; a canceled action skips its remaining phases).

## Execution model

- `ActionSystem` is **not** a MonoBehaviour. `Update()` must be pumped externally (tests loop `while (actionSystem.IsActive) actionSystem.Update();`). One `MoveNext()` per call advances the whole nested flow by one step.
- Only **one root action** can run at a time — `PerformGameAction` silently returns if `IsActive`. There is no queue of pending root actions.
- **Reactions**: during any phase, subscribers may call `Container.AddReaction(action)`. After the phase handler finishes, collected reactions are sorted (`ActionSystemSorterFiFo`: priority desc, then order-of-play asc) and each runs through the full `GameActionFlow` recursively.
- **Post-resolution events**: registered via `RegisterPostResolutionEvent`; after the root action and all reactions finish, each is posted (as a plain event) and its reactions resolved, looping while `Repeats && reactions.Count > 0`. Intended for cleanup by systems like passive abilities.
- **Viewers**: each `GameActionPhase` has an optional `Viewer` (`Func<IContainer, GameAction, IEnumerator>`). If set, the viewer coroutine runs and triggers the handler by yielding `true`; if it never does, the handler runs after the viewer finishes. This is the intended hook for UI animation pacing.

## The event-type machinery

Posting a strongly-typed event per action type without runtime reflection is handled by a three-piece pipeline:

1. **`GameActionEvents.cs`** defines generic wrappers (`GameActionPrepared<TAction>`, `GameActionPerformed<TAction>`, `GameActionCanceled<TAction>`, `GameActionCompleted<TAction>`, `GameActionFlowStarted<TAction>`, `GameActionFlowCompleted<TAction>`) plus non-generic interfaces (`IGameActionPerformedEvent`, …) so you can subscribe to "any action's Perform".
2. **`GameActionEventTypes`** — built once per concrete action type; uses `MakeGenericType` + a cached delegate that instantiates the event and calls `NotificationEventSystem.PostEventAndExecute`.
3. **`GameActionTypeRegistry`** — static cache; preloads every concrete `GameAction` subclass at startup (`RuntimeInitializeOnLoadMethod`) and on first construction of an action type.

**Subscribing:** use `GameActionEventSubscriptionFactory` — `SubscribeToPerform<ChangeTurnAction>(handler)` for one action type, `SubscribeToAnyPerform(handler)` / `...Filtered(handler, predicate)` for all. Note: subscribe to the **event wrapper**, never to the action type itself — the action is an `EventData` subclass but is never posted directly, so an `EventSubscription<ChangeTurnAction>` will never fire (this exact bug existed in `MatchSystem` and was fixed).

Example (the only concrete action so far):

```csharp
// MatchSystem
public override void Awake() {
    Subscribe(GameActionEventSubscriptionFactory.SubscribeToPerform<ChangeTurnAction>(OnChangeTurnPerformed));
}
void OnChangeTurnPerformed(GameActionPerformed<ChangeTurnAction> eventData) {
    Container.GetMatch().CurrentPlayerIndex = eventData.Action.TargetPlayerIndex;
}
```

## Known issues to address in the redesign

- **No cancellation flow.** `Cancel()` sets a flag and phases are skipped, but `PostCanceled` is never invoked by `ActionSystem`, so nothing observes cancellation.
- **Post-resolution repeat can loop forever.** `PostActionResolutionFlow` has no iteration cap while `Repeats && reactions.Count > 0`.
- **Single root action, no queue.** `PerformGameAction` during an active flow is silently dropped — callers get no feedback.
- **Sorter is hardcoded** to FIFO (`ActionSystemSorterFiFo`); LIFO exists but isn't injectable.
- **`OrderOfPlayCounter` is static and never reset** in gameplay (tests reset it manually); long sessions keep counting up and cross-match determinism suffers.
- **Viewer can't veto.** The handler always runs even if the viewer never yields `true` — "signal when ready" semantics, not "approve/deny".
- **No error handling.** An exception inside a phase handler leaves `_rootFlow` mid-flight with no recovery path.
- **Update pumping contract is implicit.** Nothing owns calling `Update()`; the bootstrap design should settle who drives the system (and whether to move to `async`/awaitable flows).
