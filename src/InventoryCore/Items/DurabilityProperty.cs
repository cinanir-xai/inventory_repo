namespace InventoryCore.Items;

public sealed class DurabilityProperty : IItemProperty
{
    public string Key => "durability";
    public bool IsInstanceSpecific => true;
    public int Current { get; set; }
    public int Max { get; }

    public DurabilityProperty(int max, int? current = null)
    {
        if (max <= 0) throw new ArgumentOutOfRangeException(nameof(max));
        Max = max;
        Current = current ?? max;
        if (Current < 0 || Current > Max)
            throw new ArgumentOutOfRangeException(nameof(current));
    }

    public IItemProperty Clone() => new DurabilityProperty(Max, Current);
}
