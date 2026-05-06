namespace InventoryCore.Tests;

using InventoryCore.Items;
using InventoryCore.Registry;
using InventoryCore.Slots;
using Xunit;

public class SlotTests
{
    private readonly ItemRegistry _r = DefaultItems.CreateDefault();

    [Fact]
    public void Slot_AcceptsAnyStackByDefault()
    {
        var s = new Slot(0, SlotKind.General);
        Assert.True(s.Accepts(new ItemStack(_r.Get("cobblestone"), 1)));
        Assert.True(s.Accepts(new ItemStack(_r.Get("apple"), 1)));
    }

    [Fact]
    public void RestrictedSlot_OnlyAcceptsCategory()
    {
        var s = new RestrictedSlot(5, ItemCategory.Food);
        Assert.True(s.Accepts(new ItemStack(_r.Get("apple"), 1)));
        Assert.False(s.Accepts(new ItemStack(_r.Get("cobblestone"), 1)));
    }

    [Fact]
    public void Set_StoresStack()
    {
        var s = new Slot(0, SlotKind.General);
        var stack = new ItemStack(_r.Get("dirt"), 5);
        s.Set(stack);
        Assert.False(s.IsEmpty);
        Assert.Same(stack, s.Stack);
    }

    [Fact]
    public void Take_RemovesAndReturnsStack()
    {
        var s = new Slot(0, SlotKind.General);
        var stack = new ItemStack(_r.Get("dirt"), 5);
        s.Set(stack);
        var taken = s.Take();
        Assert.Same(stack, taken);
        Assert.True(s.IsEmpty);
    }

    [Fact]
    public void RestrictedSlot_RejectsSetOfWrongCategory()
    {
        var s = new RestrictedSlot(0, ItemCategory.Tools);
        Assert.Throws<InvalidOperationException>(() =>
            s.Set(new ItemStack(_r.Get("apple"), 1)));
    }
}
