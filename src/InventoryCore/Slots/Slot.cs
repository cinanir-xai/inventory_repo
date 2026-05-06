namespace InventoryCore.Slots;

using InventoryCore.Items;

public class Slot
{
    public int Index { get; }
    public SlotKind Kind { get; }
    public ItemStack? Stack { get; private set; }

    public Slot(int index, SlotKind kind)
    {
        if (index < 0) throw new ArgumentOutOfRangeException(nameof(index));
        Index = index;
        Kind = kind;
    }

    public bool IsEmpty => Stack == null;

    public virtual bool Accepts(ItemStack stack) => stack != null;

    public void Set(ItemStack? stack)
    {
        if (stack != null && !Accepts(stack))
            throw new InvalidOperationException("slot does not accept this stack");
        Stack = stack;
    }

    public ItemStack? Take()
    {
        var s = Stack;
        Stack = null;
        return s;
    }
}
