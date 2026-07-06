# Findings & Improvement Roadmap

Snapshot from the 2026-07 architecture review. First table: issues **fixed** in that pass. Second table: open issues, prioritized — the ActionSystem items are intentionally deferred to the planned collaborative redesign.

## Fixed (2026-07)

| Issue | Where | Fix applied |
|---|---|---|
| Tests referenced deleted non-generic event classes (`GameActionPerformedEvent`, …), breaking Assembly-CSharp compilation entirely | `Assets/Tests/ActionSystemTests.cs` | Rewritten to the generic API (`GameActionPerformed<TestAction>`, …) |
| Stray auto-imported usings (`CodiceApp.EventTracking`, `Unity.Profiling.LowLevel.Unsafe`) broke player builds | `ActionSystem/GameAction.cs` | Removed |
| Unguarded `using UnityEditor;` broke player builds | `Utility/TypeRegistry.cs`, `ActionSystem/GameActionTypeRegistry.cs` | Wrapped in `#if UNITY_EDITOR` |
| `MatchSystem` subscribed to `ChangeTurnAction` directly — actions are never posted as events, so turn changes never applied | `GameplaySystems/MatchSystem.cs` | Subscribes to `GameActionPerformed<ChangeTurnAction>` via the factory |
| `GameplayAspect.Unsubscribe` never unregistered from the event system (leak) | `GameplaySystems/GameplayAspect.cs` | Also calls `NotificationEventSystem.Unsubscribe` |
| `EventSubscription.TypeHash` used `HashSet.FirstOrDefault()` — implementation-defined ordering | `NotificationEventSystem/EventSubscription.cs` | Hash computed directly from `typeof(T).FullName` |
| Zone "capacity" was only a `List.Capacity` allocation hint; deck/bench/prize limits never enforced | `Model/Zone.cs` | Real `MaxSize`; `AddCard` returns `false` when full |
| `PostEventInstantly` was a byte-identical duplicate of `PostEventAndExecute` | `NotificationEventSystem/NotificationEventSystem.cs` | Removed |
| Profiles cached by **array index** instead of `profile.Id` — every `GetProfile(id)` lookup and deck/card→set cross-reference silently failed | `Data/Profiles/ProfilesLoader.cs` | Keyed by `profile.Id` |
| `MatchSystemTests` was an empty Unity template | `Assets/Tests/GamePlaySystemTests/MatchSystemTests.cs` | Real tests: full container wiring, turn changes verified end-to-end |

## Open issues

### High priority

| # | Issue | Suggested remedy |
|---|---|---|
| 1 | **No bootstrap.** Nothing creates the `Container`, registers aspects, calls `Awake()`, pumps `ActionSystem.Update()` / `StateMachine.Update()`, or flushes the event queue. The engine only runs inside tests. | A `GameBootstrapper : MonoBehaviour`: load profiles → build container → `Awake()` systems in order → drive updates → `Destroy()` on teardown. Settle the ownership story as part of the ActionSystem redesign. |
| 2 | **Abilities/attacks/trainer rules are unparsed** (`List<Dictionary<string,string>>` / raw strings on `PokemonProfile`/`TrainerProfile`; TODOs exist in code). No rules engine can be built on this. | Define `Ability`, `Attack` (damage, costs, effect text/effect graph), `TrainerRule` domain classes; parse during profile decode; fail loudly on unparseable data. |
| 3 | **ActionSystem redesign backlog** (deferred to joint session): no cancellation event flow; post-resolution repeat can loop infinitely; single root action with silent drop and no queue; hardcoded FIFO sorter; static `OrderOfPlayCounter` never reset; viewer can't veto its handler; no exception recovery mid-flow. | See [action-system.md](action-system.md) "Known issues" — treat as the redesign's requirements list. |
| 4 | **StateMachine guard bug:** `TransitionTo` blocks only if *both* guards fail (`!CanExit(target) && !CanEnter(current)`); either failing should block. | Change `&&` to `||` (`StateMachine.cs:62`) and add a guard test. Also: descend into initial children when transitioning into a composite. |

### Medium priority

| # | Issue | Suggested remedy |
|---|---|---|
| 5 | **Silent null returns**: `ProfilesController.GetProfile`, `PokemonProfileFactory.CreateCardProfile`, `Container.GetAspect` all return null on a miss. | Throw (or add `TryGet` variants) — a missing profile/aspect is a programming error and should fail fast. |
| 6 | **Subscription lifecycle semantics**: `Subscribe()` registers immediately, `GameplaySystem.Awake()` calls `SubscribeAll()` → double-registration if a subclass calls both; subclasses currently just skip `base.Awake()`. | Pick one model: e.g. `Subscribe()` only tracks, `Awake()` registers everything. Belongs with the bootstrap/lifecycle work. |
| 7 | **Queued events never flushed** — `PostEvent` (used by `StateMachine`) is inert without a `Flush()` driver. | Bootstrap flushes once per frame (e.g. end of `LateUpdate`). |
| 8 | **Dispatch ordering & allocation**: priority only holds within one type bucket, and every dispatch allocates via LINQ. | Merge matching buckets by priority (or keep a single sorted merge); cache the merged list per event type hash-set. |
| 9 | **`GameDataSystem.Match` is a public mutable field** — anything can bypass the action system. | Private setter + read via `GetMatch()`; mutation only from action handlers. |
| 10 | **Save/load unimplemented** (`SaveGame`/`LoadGame` TODO stubs) while the serialization layer sits ready; `JsonDecoder` can't instantiate types without parameterless constructors (e.g. `Zone<T>`). | Wire `Match.Encode/Decode` up; give serializable model types parameterless constructors or a decoder factory hook. |
| 11 | **Exact-type profile lookup**: cards live under `typeof(CardProfile)` only, so `GetProfile<PokemonProfile>(id)` misses. | Either register under both concrete and base type, or document `GetProfile<CardProfile>` + cast as the API (currently documented in [model-and-data.md](model-and-data.md)). |

### Low priority / design debt

| # | Issue | Suggested remedy |
|---|---|---|
| 12 | `Player.GetZone<T>` unchecked cast over `Dictionary<Zone, object>` | Type-safe zone registry (per-zone fields, or `Dictionary<Zone, ZoneBase>` with a checked cast) |
| 13 | `Match._currentTurn` serialized but never used; `Match` class summary comment is unfinished | Implement turn counting in the turn structure work, or drop it |
| 14 | `Zones.AttachedCards` enum value has no zone; attached energy/evolution mechanics undesigned | Design attachment model (likely per-`PokemonCard` attachment list rather than a player zone) |
| 15 | `StateMachineBuilder` unused (own TODO); `CompositeState` initial state is implicit first-child | Use the builder when building the real match FSM; add explicit `SetInitialState` |
| 16 | No assembly definitions (deliberate, deferred) — everything in Assembly-CSharp; editor-only code must be `#if UNITY_EDITOR`-guarded by hand | Reintroduce asmdefs later (`GimGim.Runtime`, `GimGim.Editor`, `GimGim.Tests`); enables Unity Test Framework isolation and faster compiles |
| 17 | Thread-safety: `NotificationEventSystem`, `TypeRegistry`, `ProfilesController` singletons assume single-threaded access | Fine for now; revisit only if async/jobs enter the picture |
| 18 | `Singleton<T>`/`PersistentSingleton<T>` MonoBehaviour singletons are unused by the core engine | Keep for future view-layer managers; core stays plain-C# |

## Suggested sequencing

1. **ActionSystem redesign** (joint session — item 3, folding in items 6 and the cancellation/queue semantics).
2. **Bootstrap** (item 1 + 7) — makes the engine runnable in a scene, unblocks everything visual.
3. **Turn structure** via the state machine (items 4, 13, 15) — first real consumer of the FSM and builder.
4. **Rules content** (item 2) — abilities/attacks domain model, then effect resolution as `GameAction`s.
5. **View layer** — card/board UI consuming action-phase viewers and events.
