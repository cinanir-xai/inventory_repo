namespace InventoryCore.Slots;

using InventoryCore.Items;

public sealed class RestrictedSlot : Slot
{
    public ItemCategory AllowedCategory { get; }

    public RestrictedSlot(int index, ItemCategory allowed)
        : base(index, SlotKind.Restricted)
    {
        AllowedCategory = allowed;
    }

    public override bool Accepts(ItemStack stack)
    {
        if (stack == null) return false;
        return stack.Definition.Category == AllowedCategory;
    }
}
