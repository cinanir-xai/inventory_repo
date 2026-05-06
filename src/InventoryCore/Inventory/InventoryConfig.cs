namespace InventoryCore.Inventory;

using InventoryCore.Items;

public sealed class InventoryConfig
{
    public int TotalSlots { get; init; } = 40;
    public int HotbarSize { get; init; } = 10;
    public IReadOnlyDictionary<int, ItemCategory> RestrictedSlots { get; init; }
        = new Dictionary<int, ItemCategory>();

    public static InventoryConfig Default() => new InventoryConfig();
}
