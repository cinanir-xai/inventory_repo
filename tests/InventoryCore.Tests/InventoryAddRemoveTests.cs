namespace InventoryCore.Tests;

using InventoryCore.Inventory;
using InventoryCore.Items;
using InventoryCore.Registry;
using Xunit;

public class InventoryAddRemoveTests
{
    private readonly ItemRegistry _r = DefaultItems.CreateDefault();

    private Inventory NewInventory() => new Inventory(InventoryConfig.Default());

    [Fact]
    public void TryAdd_ToEmptyInventoryPlacesInFirstSlot()
    {
        var inv = NewInventory();
        var added = inv.TryAdd(new ItemStack(_r.Get("dirt"), 10));
        Assert.Equal(10, added);
        Assert.False(inv.GetSlot(0).IsEmpty);
        Assert.Equal(10, inv.GetSlot(0).Stack!.Count);
    }

    [Fact]
    public void TryAdd_FillsExistingPartialStackBeforeEmptySlot()
    {
        var inv = NewInventory();
        inv.TryAdd(new ItemStack(_r.Get("dirt"), 50));
        inv.TryAdd(new ItemStack(_r.Get("dirt"), 20));
        Assert.Equal(64, inv.GetSlot(0).Stack!.Count);
        Assert.Equal(6, inv.GetSlot(1).Stack!.Count);
    }

    [Fact]
    public void TryAdd_RespectsCategoryRestriction()
    {
        var cfg = new InventoryConfig
        {
            TotalSlots = 40,
            HotbarSize = 10,
            RestrictedSlots = new Dictionary<int, ItemCategory> { { 0, ItemCategory.Food } }
        };
        var inv = new Inventory(cfg);
        inv.TryAdd(new ItemStack(_r.Get("dirt"), 5));
        Assert.True(inv.GetSlot(0).IsEmpty);
        Assert.Equal(5, inv.GetSlot(1).Stack!.Count);
    }

    [Fact]
    public void Remove_TakesFromMultipleSlots()
    {
        var inv = NewInventory();
        inv.TryAdd(new ItemStack(_r.Get("dirt"), 64));
        inv.TryAdd(new ItemStack(_r.Get("dirt"), 64));
        inv.TryAdd(new ItemStack(_r.Get("dirt"), 30));
        var removed = inv.Remove("dirt", 100);
        Assert.Equal(100, removed);
        Assert.Equal(58, inv.Count("dirt"));
    }

    [Fact]
    public void Remove_LeavingEmptySlotsClearsThem()
    {
        var inv = NewInventory();
        inv.TryAdd(new ItemStack(_r.Get("apple"), 5));
        inv.Remove("apple", 5);
        Assert.True(inv.GetSlot(0).IsEmpty);
    }

    [Fact]
    public void TryAdd_FiresChangedEventOnce()
    {
        var inv = NewInventory();
        var fires = 0;
        inv.Changed += (_, _) => fires++;
        inv.TryAdd(new ItemStack(_r.Get("dirt"), 10));
        Assert.Equal(1, fires);
    }

    [Fact]
    public void Count_SumsAcrossSlots()
    {
        var inv = NewInventory();
        inv.TryAdd(new ItemStack(_r.Get("bread"), 30));
        inv.TryAdd(new ItemStack(_r.Get("bread"), 30));
        Assert.Equal(60, inv.Count("bread"));
    }

    [Fact]
    public void TryAdd_ToolsDoNotMergeAndOccupySeparateSlots()
    {
        var inv = NewInventory();
        inv.TryAdd(new ItemStack(_r.Get("iron_pickaxe"), 1));
        inv.TryAdd(new ItemStack(_r.Get("iron_pickaxe"), 1));
        Assert.False(inv.GetSlot(0).IsEmpty);
        Assert.False(inv.GetSlot(1).IsEmpty);
        Assert.Equal("iron_pickaxe", inv.GetSlot(0).Stack!.Definition.Id);
        Assert.Equal("iron_pickaxe", inv.GetSlot(1).Stack!.Definition.Id);
    }
}
