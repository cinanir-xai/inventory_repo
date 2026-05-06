namespace InventoryCore.Tests;

using InventoryCore.Items;
using InventoryCore.Registry;
using Xunit;

public class ItemStackTests
{
    private readonly ItemRegistry _r = DefaultItems.CreateDefault();

    [Fact]
    public void Construction_RejectsZeroCount()
    {
        var def = _r.Get("cobblestone");
        Assert.Throws<ArgumentOutOfRangeException>(() => new ItemStack(def, 0));
    }

    [Fact]
    public void Construction_RejectsCountAboveMax()
    {
        var def = _r.Get("cobblestone");
        Assert.Throws<ArgumentOutOfRangeException>(() => new ItemStack(def, 65));
    }

    [Fact]
    public void Construction_DefaultPropertiesCloned()
    {
        var def = _r.Get("iron_pickaxe");
        var a = new ItemStack(def, 1);
        var b = new ItemStack(def, 1);
        Assert.NotSame(a.Properties["durability"], b.Properties["durability"]);
    }

    [Fact]
    public void TryMerge_FullDestinationReturnsZero()
    {
        var def = _r.Get("cobblestone");
        var a = new ItemStack(def, 64);
        var b = new ItemStack(def, 10);
        Assert.Equal(0, a.TryMerge(b));
        Assert.Equal(64, a.Count);
        Assert.Equal(10, b.Count);
    }

    [Fact]
    public void TryMerge_PartialFillsDestinationAndReducesSource()
    {
        var def = _r.Get("cobblestone");
        var a = new ItemStack(def, 60);
        var b = new ItemStack(def, 10);
        Assert.Equal(4, a.TryMerge(b));
        Assert.Equal(64, a.Count);
        Assert.Equal(6, b.Count);
    }

    [Fact]
    public void TryMerge_DifferentDefinitionsReturnsZero()
    {
        var a = new ItemStack(_r.Get("cobblestone"), 10);
        var b = new ItemStack(_r.Get("dirt"), 10);
        Assert.Equal(0, a.TryMerge(b));
    }

    [Fact]
    public void TryMerge_ItemsWithDurabilityCannotMerge()
    {
        var def = _r.Get("iron_pickaxe");
        var a = new ItemStack(def, 1);
        var b = new ItemStack(def, 1);
        Assert.Equal(0, a.TryMerge(b));
        Assert.Equal(1, a.Count);
        Assert.Equal(1, b.Count);
    }

    [Fact]
    public void Split_ReducesSourceAndProducesNewStack()
    {
        var def = _r.Get("apple");
        var a = new ItemStack(def, 10);
        var split = a.Split(3);
        Assert.NotNull(split);
        Assert.Equal(7, a.Count);
        Assert.Equal(3, split!.Count);
    }

    [Fact]
    public void Split_AmountEqualToCountReturnsNull()
    {
        var def = _r.Get("apple");
        var a = new ItemStack(def, 10);
        Assert.Null(a.Split(10));
        Assert.Equal(10, a.Count);
    }
}
