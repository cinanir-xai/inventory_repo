namespace InventoryCore.Tests;

using InventoryCore.Items;
using InventoryCore.Registry;
using Xunit;

public class RegistryTests
{
    [Fact]
    public void DefaultRegistry_RegistersAllItems()
    {
        var r = DefaultItems.CreateDefault();
        Assert.True(r.Count >= 15);
    }

    [Fact]
    public void Get_UnknownIdThrows()
    {
        var r = DefaultItems.CreateDefault();
        Assert.Throws<KeyNotFoundException>(() => r.Get("nonexistent"));
    }

    [Fact]
    public void Register_DuplicateIdThrows()
    {
        var r = new ItemRegistry();
        var def = new ItemDefinition("x", "X", ItemCategory.Misc, 64, 1f);
        r.Register(def);
        Assert.Throws<InvalidOperationException>(() => r.Register(def));
    }

    [Fact]
    public void DefaultPickaxe_HasDurabilityProperty()
    {
        var r = DefaultItems.CreateDefault();
        var def = r.Get("iron_pickaxe");
        Assert.Contains("durability", def.DefaultProperties.Keys);
    }
}
