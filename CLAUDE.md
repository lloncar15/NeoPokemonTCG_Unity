# NeoPokemonTCG_Unity

Fan-made, non-commercial recreation of the Pokemon TCG engine (sets up to, excluding, eCard) in **Unity 6000.0.43f1**. All code lives under the `GimGim.*` namespace in `Assets/Scripts`.

## Architecture in ten lines

- **Model** (`GimGim.Model`): pure C# game state — `Match` → 2 × `Player` → typed `Zone<T>` collections of `Card` instances. Cards are flyweights: they hold a `profileId` and look static data up in `ProfilesController`.
- **Data** (`GimGim.Data`): `CardProfile`/`PokemonProfile`/`TrainerProfile`/`EnergyProfile`, `SetProfile`, `DeckProfile` — loaded once from JSON in `Resources/Profiles/` by `ProfilesLoader`, cached in the `ProfilesController` singleton, keyed by profile Id.
- **Aspect container** (`GimGim.AspectContainer`): a service locator. A `Container` holds systems ("aspects"); systems reach each other via `Container.GetAspect<T>()` and extension helpers (`game.GetMatch()`, `game.PerformGameAction(...)`).
- **Gameplay systems** (`GimGim.GameplaySystems`): `GameplaySystem` subclasses (e.g. `MatchSystem`, `GameDataSystem`) mutate the model — but only in response to game actions.
- **Action system** (`GimGim.ActionSystem`): ⚠️ **work in progress, being redesigned.** Every model change goes through a `GameAction` (Prepare + Perform phases) executed by `ActionSystem` as a hand-pumped coroutine; systems react via events and can enqueue reaction actions.
- **Event system** (`GimGim.EventSystem`): `NotificationEventSystem` singleton dispatches `EventData` to `EventSubscription<T>` subscribers, matched by SHA1-based type hashes (`TypeRegistry`) so base-class/interface subscriptions work.
- **State machine** (`GimGim.StateMachine`): hierarchical FSM with guarded, LCA-based transitions; intended for match/turn phases, not yet wired to gameplay.
- **Serialization** (`GimGim.Serialization`): `ISerializable` + `JsonEncoder`/`JsonDecoder` over bundled SimpleJSON; intended for save/load, not yet wired up.
- **View / bootstrap: none yet.** Nothing creates the `Container` or drives `ActionSystem.Update()` outside of tests; there is no UI.

## Conventions & gotchas

- No assembly definitions right now — this is deliberate and deferred (commit 32aabb0); everything compiles into Assembly-CSharp.
- Editor-only APIs must be inside `#if UNITY_EDITOR` (including `using UnityEditor;`) or player builds break.
- Systems don't use Unity's `MonoBehaviour` lifecycle — `GameplaySystem.Awake()/Destroy()` are called manually (see tests for the wiring pattern).
- Tests live in `Assets/Tests` (NUnit; edit-mode style, pumping `ActionSystem.Update()` in a loop).
- `docs/` has per-system documentation; start with [docs/architecture-overview.md](docs/architecture-overview.md). Known issues and the improvement roadmap live in [docs/findings-and-improvements.md](docs/findings-and-improvements.md).
