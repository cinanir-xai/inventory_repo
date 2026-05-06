namespace InventoryCore.Registry;

using InventoryCore.Items;

public static class DefaultItems
{
    public static ItemRegistry CreateDefault()
    {
        var r = new ItemRegistry();

        r.Register(new ItemDefinition("cobblestone", "Cobblestone", ItemCategory.Blocks, 64, 1.0f));
        r.Register(new ItemDefinition("dirt", "Dirt", ItemCategory.Blocks, 64, 0.5f));
        r.Register(new ItemDefinition("oak_log", "Oak Log", ItemCategory.Blocks, 64, 2.0f));
        r.Register(new ItemDefinition("sand", "Sand", ItemCategory.Blocks, 64, 0.8f));

        r.Register(new ItemDefinition(
            "wooden_pickaxe", "Wooden Pickaxe", ItemCategory.Tools, 1, 1.5f,
            defaultProperties: new IItemProperty[] { new DurabilityProperty(60) }));
        r.Register(new ItemDefinition(
            "iron_pickaxe", "Iron Pickaxe", ItemCategory.Tools, 1, 3.0f,
            defaultProperties: new IItemProperty[] { new DurabilityProperty(250) }));
        r.Register(new ItemDefinition(
            "iron_axe", "Iron Axe", ItemCategory.Tools, 1, 3.0f,
            defaultProperties: new IItemProperty[] { new DurabilityProperty(250) }));

        r.Register(new ItemDefinition(
            "iron_sword", "Iron Sword", ItemCategory.Weapons, 1, 2.5f,
            defaultProperties: new IItemProperty[] { new DurabilityProperty(250) }));
        r.Register(new ItemDefinition(
            "wooden_sword", "Wooden Sword", ItemCategory.Weapons, 1, 1.5f,
            defaultProperties: new IItemProperty[] { new DurabilityProperty(60) }));

        r.Register(new ItemDefinition(
            "throwing_knife", "Throwing Knife", ItemCategory.Weapons, 16, 0.2f,
            defaultProperties: new IItemProperty[] { new ThrowableProperty(damage: 8, range: 12) }));

        r.Register(new ItemDefinition(
            "apple", "Apple", ItemCategory.Food, 64, 0.1f,
            defaultProperties: new IItemProperty[] { new ConsumableProperty(hungerRestore: 4, healthRestore: 0) }));
        r.Register(new ItemDefinition(
            "cooked_steak", "Cooked Steak", ItemCategory.Food, 64, 0.3f,
            defaultProperties: new IItemProperty[] { new ConsumableProperty(hungerRestore: 8, healthRestore: 1) }));
        r.Register(new ItemDefinition(
            "bread", "Bread", ItemCategory.Food, 64, 0.2f,
            defaultProperties: new IItemProperty[] { new ConsumableProperty(hungerRestore: 5, healthRestore: 0) }));

        r.Register(new ItemDefinition("rotten_flesh", "Rotten Flesh", ItemCategory.MobDrops, 64, 0.2f));
        r.Register(new ItemDefinition("bone", "Bone", ItemCategory.MobDrops, 64, 0.3f));
        r.Register(new ItemDefinition("string", "String", ItemCategory.MobDrops, 64, 0.05f));

        r.Register(new ItemDefinition("stick", "Stick", ItemCategory.Misc, 64, 0.1f));

        return r;
    }
}
