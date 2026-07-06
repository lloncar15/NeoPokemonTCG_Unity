# Notification Event System

A lightweight, allocation-conscious pub/sub used by everything: gameplay systems, the action system, and the state machine.

## How dispatch works

- Every event extends `EventData` (`GimGim.EventSystem`). Its constructor asks `TypeRegistry.GetTypeHashes(GetType())` for a `HashSet<int>` containing a SHA1-derived hash of the event's own type **plus every base type and interface** (stopping at `object`/`UnityEngine.Object`).
- `EventSubscription<T>` stores a single `TypeHash` — the hash of `T` itself (computed directly via `HashUtility.GenerateSha1Hash(typeof(T).FullName)`).
- `NotificationEventSystem` keeps `Dictionary<int, List<IEventSubscription>>`. On dispatch, every subscription bucket whose key appears in the event's `TypeHashes` gets invoked. This is what makes **polymorphic subscriptions** work: subscribing to `IGameActionPerformedEvent` catches every `GameActionPerformed<T>`, because the interface's hash is in each event's hash set.
- `EventSubscription<T>.Invoke` type-checks (`eventData is T`) before calling the handler, so a hash collision degrades to a no-op rather than a crash.

## API surface

| Call | Behavior |
|---|---|
| `NotificationEventSystem.Subscribe(sub)` | Inserts into the bucket sorted by `Priority` (descending, binary search) |
| `Unsubscribe(sub)` | Removes from the bucket |
| `PostEvent(e)` | Enqueues; nothing happens until `Flush()` |
| `PostEventAndExecute(e)` | Dispatches synchronously, immediately |
| `Flush()` | Dispatches the whole queue |

Subscriptions support `usedOnce` (auto-unsubscribed after first invocation) and `priority`.

The action system uses `PostEventAndExecute` exclusively; the state machine uses queued `PostEvent` (so its events only arrive if something calls `Flush()` — see caveats).

## GameplayAspect / IObserve lifecycle

`GameplayAspect` (base of all gameplay systems) tracks its subscriptions in a local list:

- `Subscribe(sub)` — tracks **and** registers with the event system immediately.
- `Unsubscribe(sub)` — untracks and unregisters.
- `SubscribeAll()` / `UnsubscribeAll()` — (re)register / unregister everything tracked, without clearing the list.
- `UnsubscribeAllAndClear()` — unregister and forget; called from `GameplaySystem.Destroy()`.

⚠️ Caveat: `GameplaySystem.Awake()` calls `SubscribeAll()`. A subclass that calls `Subscribe(...)` in its own `Awake()` **and** `base.Awake()` would register those subscriptions twice. Current subclasses avoid this by not calling `base.Awake()` — the lifecycle semantics deserve a cleanup pass (tracked in [findings-and-improvements.md](findings-and-improvements.md)).

## Caveats / known limitations

- **Not thread-safe** — singleton with mutable dictionaries, assumes the main thread.
- **Cross-bucket priority is not global.** Subscriptions are priority-sorted *within* one type bucket; when an event matches several buckets (own type + base + interfaces) the buckets are concatenated, so a priority-10 interface subscriber can run after a priority-0 concrete-type subscriber.
- **Per-dispatch allocation.** `DispatchEvent` builds a LINQ `Where/SelectMany/ToList` over all buckets on every dispatch; fine for now, worth optimizing once event volume grows.
- **Queued events need a driver.** Nothing calls `Flush()` today, so anything using `PostEvent` (e.g. `StateMachine` transitions) is silently inert until a game loop owns flushing.
- **Hash space is 32-bit** (first 4 bytes of SHA1 of `Type.FullName`) — collisions are unlikely but only detected by the `is T` guard turning a delivery into a no-op.
