# Architecture Overview

The engine follows an MVC-inspired split, with the important caveat that only the **M** (model + data) and the **C** (systems + actions) exist today — there is no View layer and no bootstrap yet.

```
┌────────────────────────────────────────────────────────────────┐
│  View (NOT IMPLEMENTED — only the default SampleScene exists)  │
└────────────────────────────────────────────────────────────────┘
                 ▲ events (planned: viewers on action phases)
┌────────────────────────────────────────────────────────────────┐
│  Controller layer                                              │
│  ┌──────────────┐  ┌───────────────┐  ┌─────────────────────┐  │
│  │ Gameplay-    │  │ ActionSystem  │  │ StateMachine        │  │
│  │ Systems      │─▶│ (WIP)         │  │ (built, not wired)  │  │
│  │ (MatchSystem,│  │ GameActions,  │  └─────────────────────┘  │
│  │ GameData-    │  │ reactions,    │                           │
│  │ System)      │  │ post-resolve  │                           │
│  └──────┬───────┘  └───────┬───────┘                           │
│         │    NotificationEventSystem (type-hash pub/sub)       │
│         │    AspectContainer (service locator wiring)          │
└─────────┼──────────────────┼───────────────────────────────────┘
          ▼                  ▼
┌────────────────────────────────────────────────────────────────┐
│  Model: Match ─ Player ─ Zone<T> ─ Card (flyweight)            │
│  Data:  CardProfile / DeckProfile / SetProfile  ◀─ JSON        │
│         (ProfilesController cache, ProfilesLoader)             │
│  Serialization: ISerializable + JsonEncoder/JsonDecoder        │
└────────────────────────────────────────────────────────────────┘
```

## The intended data flow

1. **Startup (bootstrap — missing today):** load profiles (`ProfilesController.Instance.LoadPokemonProfiles()`), create a `Container`, `AddAspect` the systems (`ActionSystem`, `GameDataSystem`, `MatchSystem`, …), call `Awake()` on each, and start driving `ActionSystem.Update()` (and eventually `StateMachine.Update()` / `NotificationEventSystem.Flush()`) from a Unity `MonoBehaviour`. Today this wiring exists **only in tests** (see `Assets/Tests/ActionSystemTests.cs` and `MatchSystemTests.cs`).
2. **Model mutation:** nothing mutates the `Match` directly. A system creates a `GameAction` (e.g. `ChangeTurnAction`) and submits it via `Container.PerformGameAction(action)`.
3. **Action resolution:** `ActionSystem` runs the action's Prepare and Perform phases as a coroutine pumped by `Update()`. Each phase posts a typed event (`GameActionPrepared<T>` / `GameActionPerformed<T>`); subscribers apply the actual model change (e.g. `MatchSystem.OnChangeTurnPerformed` sets `Match.CurrentPlayerIndex`) and may enqueue *reaction* actions, which resolve recursively after the phase.
4. **Presentation (future):** each `GameActionPhase` supports an optional `Viewer` coroutine so a UI can animate a phase and control when the handler fires.

## Key patterns

| Pattern | Where | Notes |
|---|---|---|
| Service locator ("aspects") | `AspectContainer/Container.cs` | Systems keyed by type name; `Aspect.Container` back-reference |
| Command + Observer | `ActionSystem/` + `NotificationEventSystem/` | Actions are commands; all effects flow through events |
| Flyweight | `Model/Card.cs` + `Data/Profiles/` | Runtime cards store only `profileId` + mutable state |
| Type-hash pub/sub | `Utility/TypeRegistry.cs` | SHA1 of `Type.FullName` for the type, its bases, and interfaces → subscribing to a base type or interface catches derived events |
| Reflection caching | `ActionSystem/GameActionEventTypes.cs`, `GameActionTypeRegistry.cs`, `Serialization/JsonDecoder.cs` | Generic event types and post-delegates built once per action type (preloaded via `RuntimeInitializeOnLoadMethod`) |
| Hierarchical FSM + Builder | `StateMachine/` | LCA-based transitions with enter/exit guards; fluent builder exists but is unused |

## What does not exist yet

- **View/UI layer** — no scene content, no card visuals, no input.
- **Bootstrap** — no `MonoBehaviour` creates the container or pumps the systems.
- **Turn structure** — `StateMachine` and `StateMachineBuilder` are built but no concrete match/turn states exist; `Match._currentTurn` is tracked but unused.
- **Rules content** — abilities, attacks, and trainer rules are unparsed raw data (`List<Dictionary<string,string>>` / strings) on the profiles; there is no effect/rules engine.
- **Save/load** — `GameDataSystem.SaveGame()/LoadGame()` are TODO stubs; the serialization layer is ready but unwired.
- **Deck construction** — `DeckProfile` lists card IDs + counts, but nothing instantiates a deck of `Card`s for a `Player` yet.

See [findings-and-improvements.md](findings-and-improvements.md) for the prioritized roadmap, and the per-system docs: [action-system.md](action-system.md) (WIP system), [event-system.md](event-system.md), [model-and-data.md](model-and-data.md), [state-machine.md](state-machine.md).
