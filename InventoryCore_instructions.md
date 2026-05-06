Extend `InventoryCore` into a complete inventory + cursor + world-pickup + crafting + smelting + persistence system in a single change. Every existing test under `tests/InventoryCore.Tests/` (`ItemStackTests.cs`, `SlotTests.cs`, `InventoryAddRemoveTests.cs`, `RegistryTests.cs`) MUST continue to pass without ANY modification to those files. Pre-existing public API on `IInventory`, `Inventory`, `ItemRegistry`, `ItemStack`, `Slot`, `RestrictedSlot`, `ItemDefinition`, and `InventoryChangedEventArgs` MUST remain backward-compatible.

You are free to add new files, new namespaces, new methods, new types. You are NOT free to break the existing API surface or its observable behavior. When `dotnet build && dotnet test` finishes, build must report 0 warnings 0 errors (the core library has `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>`) and the test suite must report 0 failures with at least 65 passing tests in total.

# 1. Cursor and click semantics

Add `CursorSlot` (single slot, holds 0 or 1 stack, lives outside any `Inventory`). Add a click coordinator (a class is fine — call it `ClickHandler` or methods on a new `InventoryController`) implementing all of:

- **LeftClick(slotIndex, inventory)** — pick up the slot's whole stack into the cursor if cursor empty; if cursor has a stack and slot is empty, place the cursor stack into the slot; if both have stacks of the same item id and the destination has free space and neither has instance-specific state, merge as much as possible — overflow stays on the cursor; if both have stacks but they cannot merge (different ids OR either has `IsInstanceSpecific==true` properties like `DurabilityProperty`), SWAP them. If the slot rejects the cursor's item via `Accepts`, do nothing.
- **RightClick(slotIndex, inventory)** — if cursor empty, pick up half of the slot's stack rounded up (stack of 7 → cursor 4, slot 3; stack of 1 → cursor 1, slot empty); if cursor has a stack, place exactly one item into the slot, growing an existing matching stack or seeding an empty slot with a count-1 stack of the cursor's item. Do nothing if the slot rejects the item.
- **ShiftClick(slotIndex, source, target)** — instantly transfer the source slot's whole stack to the target inventory's first valid set of destinations. Respect `RestrictedSlot.AllowedCategory` and per-item `MaxStack`. Prefer filling existing partial stacks of the same id BEFORE consuming empty slots. If `target` and `source` are the same inventory, hotbar slots get pushed to general slots and vice-versa (find the first valid slot in the OPPOSITE region). If destination capacity is insufficient, leave the remainder in the source slot.
- **DragSpread(slotIndices, inventory)** — distribute the cursor stack across the listed slots evenly (left-click drag). Slots that don't `Accepts` the cursor's stack are skipped. Each receiving slot grows by `floor(cursor.Count / acceptingSlotCount)` (clamped by `MaxStack`); remainder stays on the cursor.
- **DragSingle(slotIndices, inventory)** — place exactly one item per listed accepting slot from the cursor (right-click drag). Stop early if the cursor empties.

**HARD INVARIANT:** items with any property where `IsInstanceSpecific==true` (today this is `DurabilityProperty`; tomorrow may be more) NEVER merge under any of these operations. They CAN swap, be carried by the cursor, and be moved between slots — but two such stacks NEVER consolidate into one.

# 2. Container transfers

`bool TryTransferAll(IInventory source, IInventory destination)` — move every stack from `source` into `destination` using the same rules as ShiftClick. Returns `true` if everything moved, `false` if any items remain in `source`. On `false`, `source` contains exactly the unmoved items in their original or compacted slots. Fires exactly ONE `Changed` event on the destination (Reason `"TransferAll"`) and at most one on the source (Reason `"TransferAll"`).

# 3. World items, pickup, and drop

Add a `World/` folder with:

- `Vec3` — readonly struct with `double X, Y, Z`. Provide `DistanceTo(Vec3)`, `+`, `-`, scalar `*`, `Zero`.
- `ItemEntity` — has `ItemStack Stack`, `Vec3 Position`, `Vec3 Velocity`, `double AgeSeconds`, `double PickupDelaySeconds`, `string? OwnerId`, `bool IsAlive`. Constructor takes stack + position + optional velocity, ownerId, pickupDelay (default 0.5 seconds).
- `World` — owns a list of `ItemEntity`. Methods: `Spawn(ItemEntity)`, `IReadOnlyList<ItemEntity> Entities { get; }`, `Tick(double dt)`, `IEnumerable<ItemEntity> EntitiesNear(Vec3 center, double radius)`. Tick rules:
  - Every entity ages by `dt`.
  - Entities older than 300 seconds despawn (`IsAlive=false`, removed from the list at end of tick).
  - After aging, attempt near-merge: any two alive entities with the same item id whose stacks `CanMergeWith` (no instance state) and whose positions are within 0.5 distance merge — combined stack stays in the OLDER entity (greater AgeSeconds), the younger one is removed; pickup delays merge by taking the MAX (so a freshly dropped one inheriting an old entity doesn't bypass the cooldown).

Add to `Inventory`:

- `int Drop(int slotIndex, int amount, World world, Vec3 from, Vec3 throwVelocity, string? ownerId, double pickupDelay)` — removes up to `amount` from the slot, spawns an `ItemEntity` at `from` with the given velocity / ownerId / pickupDelay (carries cloned per-slot properties), empties the slot if it hits zero. If `amount > slot.Count`, drops what's there. Returns the number actually dropped. Fires ONE `Changed` event with Reason `"Drop"`.
- `int Pickup(World world, Vec3 holderPosition, double radius, string? holderId)` — for each `IsAlive` entity within `radius` of `holderPosition` where `OwnerId == null || OwnerId == holderId` AND `AgeSeconds >= PickupDelaySeconds`, attempt to merge it into the inventory using the same fill-partials-first / restricted-slot / max-stack rules as `TryAdd`. If the entity is fully consumed, mark `IsAlive=false` and remove from world; if partial, reduce its stack accordingly. Returns total items picked up. Fires ONE `Changed` event with Reason `"Pickup"` aggregating affected slots if any items were picked up.

# 4. Crafting

Add a `Crafting/` folder.

- `CraftingGrid` — N×M grid of slots (typical: 2×2 or 3×3, but the implementation must accept arbitrary). `int Width`, `int Height`, indexed access `Slot this[int x, int y]`, an `OutputSlot` (separate slot for the result), and a `(int x, int y, int w, int h) NormalizedBounds()` returning the trimmed bounding box of non-empty slots.
- `IRecipe` interface: `string Id { get; }`, `bool Matches(CraftingGrid grid)`, `ItemStack Result(CraftingGrid grid)`, `void ConsumeIngredients(CraftingGrid grid)`.
- `ShapedRecipe` — pattern is a 2D `string?[,]` of item ids (`null` = required-empty), result item id + count. Matches if the grid's NormalizedBounds region equals the pattern OR equals the horizontal mirror of the pattern (mirror match is OPTIONAL per recipe via constructor flag, default true).
- `ShapelessRecipe` — multiset of required item ids (e.g. `["oak_planks","oak_planks","oak_planks","oak_planks"]`), result. Matches if the multiset of non-empty cells in the grid equals the required multiset exactly.
- `RecipeRegistry` — `Register(IRecipe)`, `IRecipe? FindMatch(CraftingGrid grid)` returning the FIRST registered match (deterministic — registration order is the iteration order). Duplicate ids throw on Register.
- `Crafter.TryCraft(CraftingGrid grid, RecipeRegistry registry)` — finds a match, consumes ingredients (decrement each contributing slot's count by the recipe's expected amount), and places the result in `OutputSlot` (merging if compatible). Returns `true` if a recipe was crafted, `false` if no match. When the result is a tool/weapon with a `DurabilityProperty` default, the produced stack must have a FRESH `DurabilityProperty` cloned from `ItemDefinition.DefaultProperties` (full durability).

Add the following items to `DefaultItems` (so that recipes have ingredients/results):
- `oak_planks` — Blocks, MaxStack 64, weight 1.0.
- `stone` — Blocks, MaxStack 64, weight 1.5.
- `raw_iron` — MobDrops, MaxStack 64, weight 1.0.
- `iron_ingot` — Misc, MaxStack 64, weight 1.0.
- Tag `oak_log` with `"fuel"`.

Provide `DefaultRecipes.RegisterDefaults(RecipeRegistry, ItemRegistry)` registering at least:
1. Shapeless: 1× `oak_log` → 4× `oak_planks`.
2. Shaped: 2× `oak_planks` (ontop of eachother) → 4× `stick`.
3. Shaped: pattern `[[oak_planks, oak_planks, oak_planks], [null, stick, null], [null, stick, null]]` → 1× `wooden_pickaxe` (fresh DurabilityProperty from default).
4. Shaped: pattern `[[oak_planks, oak_planks], [stick, null], [stick, null]]` → 1× `wooden_sword` (fresh DurabilityProperty).
also add the same recepies for stone pickaxe and sword and iron pickaxe and sword


# 5. Smelting

Add a `Smelting/` folder.

- `Furnace` — three internal slots: `Input`, `Fuel`, `Output`. Maintains `double BurnTimeRemaining`, `double CookProgress`, and a config: `double CookDuration` (default 10.0). Exposes `event EventHandler<FurnaceChangedEventArgs>? Changed`.
- `ISmeltingRecipe` — `string InputItemId`, `string OutputItemId`, `int OutputCount` (default 1).
- `SmeltingRecipeRegistry` — register + lookup by input id.
- `Furnace.Tick(double dt, SmeltingRecipeRegistry recipes, ItemRegistry items)`:
  - If `Input` is non-empty AND a recipe exists for `Input.Stack.Definition.Id` AND (`BurnTimeRemaining > 0` OR `Fuel` slot has a stack tagged `"fuel"`) AND (`Output` is empty OR (`Output.Stack.Definition.Id == recipe.OutputItemId` AND `Output.Stack.FreeSpace >= recipe.OutputCount`)):
    - If `BurnTimeRemaining <= 0`: consume one item from `Fuel` (decrement count, clear if zero), set `BurnTimeRemaining` to that item's burn time (`oak_log` → 15 seconds; default for any `"fuel"`-tagged item without an explicit burn time is 10 seconds), fire `Changed` with Reason `"FuelConsumed"` then `"BurnStart"`.
    - Decrement `BurnTimeRemaining` by `dt`. Increment `CookProgress` by `dt`.
    - When `CookProgress >= CookDuration`: produce `recipe.OutputCount` of `OutputItemId` (merge into `Output`), decrement `Input` count by 1 (clear if zero), reset `CookProgress` to 0, fire `Changed` with Reasons `"InputConsumed"` then `"CookComplete"`.
  - If conditions are NOT met (e.g. input ran out, output is full): `CookProgress` decays toward 0 by `dt` (clamped to ≥ 0), `BurnTimeRemaining` still drains by `dt`.

Default smelting recipes (in `DefaultRecipes` or a sibling): `cobblestone` → `stone`, `raw_iron` → `iron_ingot`.

# 6. Persistence

Add a `Persistence/` folder.

- `IInventorySerializer` — `string Serialize(IInventory inv)`, `void Deserialize(string blob, IInventory inv, ItemRegistry registry)`.
- `JsonInventorySerializer` — uses `System.Text.Json` (already in `net8.0`). Output is deterministic (slots in index order, properties in alphabetical key order). Captures slot index, item id, count, and per-slot properties: durability `Current/Max`, throwable `Damage/Range`, consumable `HungerRestore/HealthRestore`. Unknown property keys round-trip as opaque metadata or are dropped — your choice, document the choice. Deserializing into an inventory with a different `Capacity` than the source throws `InvalidOperationException`. Unknown item ids throw `KeyNotFoundException`.

**Round-trip invariant:** for any inventory `x` with the same shape as inventory `y`, `Deserialize(Serialize(x), y, registry)` makes `y`'s slots and per-slot properties match `x` exactly (slot count, items, counts, properties).

# 7. Events

Every user-visible action emits exactly ONE `InventoryChangedEventArgs` event (or `FurnaceChangedEventArgs` for furnaces) with affected indices and a non-empty `Reason` from this set:

`"LeftClick"`, `"RightClickPickup"`, `"RightClickPlace"`, `"ShiftClick"`, `"DragSpread"`, `"DragSingle"`, `"TransferAll"`, `"Drop"`, `"Pickup"`, `"Craft"`, `"FuelConsumed"`, `"BurnStart"`, `"InputConsumed"`, `"CookComplete"`.

A single ShiftClick that fills three destination slots fires ONCE on the destination with all three indices in `AffectedSlotIndices`, not three separate events. Same rule for DragSpread, DragSingle, TransferAll, Pickup, Drop.

# 8. Required tests

Add at least these test files under `tests/InventoryCore.Tests/`. Use xUnit `[Fact]` (and `[Theory]` where helpful). Total test count across the whole suite must be **≥ 65 passing**.

- `CursorClickTests.cs` (≥ 10 tests). Must cover at minimum: left-click partial overflow (60+10 → 64+6); left-click swap on different ids; left-click swap on two `iron_pickaxe`s (durability stacks NEVER merge, swap correctly); right-click pickup-half rounding (7 → 3+4); right-click on stack of one; right-click place-one on empty slot (cursor 5 → slot 1, cursor 4); right-click place-one on matching stack (slot 10 cursor 5 → slot 11 cursor 4); right-click respects RestrictedSlot; drag-spread distributes evenly with remainder on cursor; drag-single places exactly one per accepting slot.
- `TransferTests.cs` (≥ 6 tests). Includes: shift-click prefers partial existing stacks; shift-click respects category restriction; shift-click hotbar↔general flip in same inventory; shift-click leaves remainder when capacity insufficient; durability stacks never consolidate under shift-click or TryTransferAll; TryTransferAll returns false when remainder exists and source contains exactly the remainder.
- `WorldPickupTests.cs` (≥ 8 tests). Includes: spawn + age + despawn at >300s; pickup respects PickupDelay (entity younger than delay is NOT picked up); owner gating (entity with OwnerId="A" cannot be picked up by holderId "B"); owner-null entities pickable by anyone; near-merge of two alive entities within 0.5 distance; far entities (>0.5) do NOT merge; drop empties the slot when amount equals count; drop-then-pickup round trip preserves count and durability value.
- `CraftingTests.cs` (≥ 6 tests). Includes: shaped recipe matches with pattern at top-left, middle, and bottom-right (NormalizedBounds); shaped recipe mirror match; shapeless recipe match (multiset); shapeless recipe non-match (one extra item makes it fail); crafted wooden_pickaxe has FRESH durability (Current == Max); crafting consumes exactly the required ingredient counts.
- `SmeltingTests.cs` (≥ 6 tests). Includes: fuel only consumed when input + recipe present; one full cook tick produces output and decrements input; output is gated by capacity (full output halts cook progress); cook progress decays when fuel runs out; switching input mid-cook resets progress (or document the behavior — pick one and test it); event reasons are emitted in the correct order.
- `SerializerTests.cs` (≥ 4 tests). Includes: round-trip preserves slot positions, counts, and durability values; unknown item id throws `KeyNotFoundException`; capacity mismatch throws `InvalidOperationException`; serialize output is deterministic (same input → byte-identical output across two calls).

# 9. Hard constraints (verifier will grep for these)

- **No `//`, no `/* */`, no `///` comments anywhere in `src/` or `tests/`.** Verified by `grep -rn "//" src/ tests/ --include="*.cs"`. Must return zero lines.
- Pre-existing test files (`ItemStackTests.cs`, `SlotTests.cs`, `InventoryAddRemoveTests.cs`, `RegistryTests.cs`) MUST be byte-identical to the initial commit. Verified by `git diff <initial-commit> -- tests/InventoryCore.Tests/{ItemStackTests,SlotTests,InventoryAddRemoveTests,RegistryTests}.cs` returning empty.
- No new NuGet dependencies in the core library. The `InventoryCore.csproj` `<ItemGroup>` should remain empty of `<PackageReference>`. The test project may continue to use only its existing references.
- `dotnet build` must produce zero warnings and zero errors. The core library's `<TreatWarningsAsErrors>true</TreatWarningsAsErrors>` is non-negotiable.
- Iteration over `Dictionary`, recipe lists, world entities, and slots must be deterministic in observable behavior. If you use `HashSet` anywhere whose enumeration order is observable, replace it.
- All `IsInstanceSpecific==true` properties NEVER merge under any operation, ever.

# 10. Out of scope (do NOT implement)

- Networking, multiplayer, or save files on disk (persistence is in-memory string blobs only).
- A real game loop, threading, or async. Tick is a single synchronous call driven by the test.
- Rendering, UI, or any console output beyond what `InventoryCore.Cli` already does.
- Modifying the existing `InventoryCore.Cli` is optional — you may add new CLI commands, but the existing ones must keep working with the same syntax.

When done, run `dotnet build` and `dotnet test` from the repo root and confirm both succeed. You may but are not required to commit. Leave the repo in a state where a verifier can `git diff <initial-commit>` and see all changes.
