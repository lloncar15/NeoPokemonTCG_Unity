# Model & Data

## Runtime model (`GimGim.Model`)

```
Match ── 2 × Player ── 7 × Zone<T> ── Card (abstract)
                                       ├─ PokemonCard
                                       ├─ TrainerCard
                                       └─ EnergyCard
```

- **`Match`** — owns the player list (hardcoded to 2), `CurrentPlayerIndex`, and computed `CurrentPlayer`/`OpponentPlayer`. `_currentTurn` is serialized but not yet used by any logic.
- **`Player`** — creates its zones in the constructor and exposes `GetZone<T>(Zone zone)` (unchecked cast — the caller must pair the right `T` with the right zone):

  | Zone | Element type | Max size |
  |---|---|---|
  | Hand | `Card` | unlimited |
  | Deck | `Card` | 60 |
  | DiscardPile | `Card` | unlimited |
  | Prizes | `Card` | 6 |
  | Bench | `PokemonCard` | 5 |
  | ActivePokemon | `PokemonCard` | 1 |
  | Stadium | `TrainerCard` | 1 |

  (`Zone.AttachedCards` exists in the enum but no zone is created for it yet.)
- **`Zone<T>`** — list wrapper with a `MaxSize` (0 = unlimited). `AddCard` returns `false` when the zone is full; `RemoveCard` returns the underlying `List.Remove` result. `GetCards()` returns a defensive copy.
- **`Card`** — the flyweight handle: `profileId` + mutable per-instance state (`PlayOrder`, `OwnerIndex`, `Zone`). `GetProfile()` resolves static data through `ProfilesController`. The three subclasses exist mainly to give zones compile-time element types and to narrow the profile type.

Everything implements `ISerializable` (`Encode`/`Decode`), though save/load is not wired up yet — and note that decoding types without parameterless constructors (e.g. `Zone<T>`) is not currently supported by `JsonDecoder`'s instantiation path.

## Static data: profiles (`GimGim.Data`)

Profiles are immutable card/deck/set definitions loaded from JSON (`Assets/Resources/Profiles/*.json`, sourced from the pokemontcg.io data shape).

- **`Profile`** — base, just an `Id`. ID scheme: sets use round thousands (Base = 1000, Jungle = 2000, …); cards/decks embed their set as `SetId = (Id / 1000) * 1000`.
- **`CardProfile`** — name, set code, supertype, subtypes, rarity, images, HP. Subclasses:
  - **`PokemonProfile`** — energy types, evolvesFrom/To (by name), weaknesses/resistances, retreat cost, **abilities and attacks as raw `List<Dictionary<string,string>>`** (own TODOs: turn these into domain classes — prerequisite for a rules engine).
  - **`TrainerProfile`** — `Rules` as a raw string (same TODO).
  - **`EnergyProfile`** — empty stub (intentional).
- **`SetProfile`** — set metadata plus `HashSet<int>` of member card/deck IDs, populated during loading.
- **`DeckProfile`** — list of `(cardId, count)` tuples + deck energy types. No deck-building/validation logic yet.

### Loading pipeline

```
ProfilesController.Instance.LoadPokemonProfiles()
  └─ ProfilesLoader("setProfiles", "deckProfiles", "cardProfiles")
       1. LoadSetProfiles()   ← must run first
       2. LoadDeckProfiles()  ← registers deck IDs into their SetProfile
       3. LoadCardProfiles()  ← PokemonProfileFactory picks the subclass from
                                 the JSON "supertype"; registers card IDs into sets
```

Results are cached in `ProfilesController` as `Dictionary<Type, Dictionary<int, Profile>>`, keyed by **profile Id** (fixed 2026-07: they were previously keyed by array index, which silently broke every `GetProfile(id)` lookup and set cross-referencing). Access is via static `ProfilesController.GetProfile<T>(id)` / `GetAllProfiles<T>()`; accessing before `LoadPokemonProfiles()` throws.

Caveats:

- Lookups are exact-type: everything loaded by `LoadCardProfiles` is stored under `typeof(CardProfile)`, so use `GetProfile<CardProfile>(id)` (and cast), not `GetProfile<PokemonProfile>(id)`.
- Malformed JSON entries are logged and skipped (no aggregate error reporting); a missing file logs an error and yields an empty set.
- `PokemonProfileFactory.CreateCardProfile` returns `null` for an unknown supertype; `GetProfile` returns `null` for unknown IDs — callers must null-check.

## Serialization (`GimGim.Serialization`)

- **`ISerializable`** — `Encode(IEncoder)` / `Decode(IDecoder)`; objects serialize themselves field-by-field with string keys.
- **`JsonEncoder`/`JsonDecoder`** — wrap the bundled **SimpleJSON** parser. Broad type support: primitives, enums, lists/sets/stacks/queues, dictionaries, tuples/ValueTuples, nested `ISerializable`, and Unity types (Vector2/3/4, Color, Quaternion, Rect).
- `JsonDecoder` caches compiled constructor delegates (expression trees) and resolves type names only from whitelisted assemblies (`GimGim*`, `Assembly-CSharp*`) as a deserialization-security measure. Types without a parameterless constructor can't be instantiated by the decoder (relevant for `Zone<T>`).
