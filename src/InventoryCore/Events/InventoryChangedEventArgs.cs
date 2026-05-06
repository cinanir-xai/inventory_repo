namespace InventoryCore.Events;

public sealed class InventoryChangedEventArgs : EventArgs
{
    public IReadOnlyList<int> AffectedSlotIndices { get; }
    public string Reason { get; }

    public InventoryChangedEventArgs(IReadOnlyList<int> affectedSlots, string reason)
    {
        AffectedSlotIndices = affectedSlots ?? throw new ArgumentNullException(nameof(affectedSlots));
        Reason = reason ?? throw new ArgumentNullException(nameof(reason));
    }
}
