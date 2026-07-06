# State Machine

A hierarchical finite state machine (`GimGim.StateMachine`) intended to drive match flow (setup → turns → end). It is fully built and unit-testable but **not yet wired to any gameplay** — no concrete match/turn states exist.

## Concepts

- **`State`** — abstract base with lifecycle hooks (`OnEnter`, `OnExit`, `OnUpdate`) and transition guards (`CanExit(target)`, `CanEnter(previous)`, both default `true`). Each state knows its `Parent` and receives a `StateMachine` context reference.
- **`CompositeState`** — a state containing children. Its *initial state* is implicitly **the first child added** (`AddChildState`).
- **`StateMachine`** — holds the root composite and the current (leaf) state.
  - `StartStateMachine()` enters the root and recursively descends into initial children until it reaches a leaf.
  - `Update()` forwards to `_currentState.OnUpdate()` — needs an external driver, like everything else in this codebase.
  - `TransitionTo(target)`:
    1. Checks guards; a blocked transition posts `StateTransitionBlocked` (queued — see caveat) and returns `false`.
    2. Finds the **least common ancestor** of current and target.
    3. Exits states from current up to (excluding) the LCA, then enters from below the LCA down to the target.
    4. Posts `StateTransitionEvent`.

## Builder

`PhaseBuilder<TPhase>` / `MachineBuilder<TRoot>` provide a fluent API for assembling the hierarchy (duplicate state types per phase are rejected). A commented-out usage example lives at the bottom of `StateMachineBuilder.cs` with a TODO — the builder has **never been used in real code** and is waiting for the concrete turn-structure implementation.

```csharp
var startPhase = new PhaseBuilder<StartMatchPhase>()
    .WithComposite<SetupInner>()
        .State<StartingPlayerSelectionState>()
    .EndComposite()
    .State<StartMatchSetupState>()
    .Build();

var machine = new MachineBuilder<BaseMatchState>()
    .Phase(startPhase)
    .Build();
```

## Caveats / known issues

- **Guard logic bug:** `TransitionTo` blocks only when *both* guards fail (`!CanExit && !CanEnter`); a single failing guard should block (`||`). See [findings-and-improvements.md](findings-and-improvements.md).
- **Transitioning into a composite does not descend.** `StartStateMachine` recursively enters initial children, but `TransitionTo(someComposite)` stops at the composite itself — the current state can end up being a non-leaf.
- **Self-transition is not special-cased.** `TransitionTo(current)` runs exit/enter along the path (LCA of a state with itself is the state, so it's mostly a no-op, but re-entry semantics are undefined by design).
- **Initial state is implicit** (first child added); an explicit `SetInitialState` would be safer once phases get complex.
- **Events are queued.** Transition events use `PostEvent`, and nothing calls `NotificationEventSystem.Flush()` yet, so today these events are never delivered.
