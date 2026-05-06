namespace InventoryCore.Inventory;

using InventoryCore.Events;
using InventoryCore.Items;
using InventoryCore.Slots;

public interface IInventory
{
    int Capacity { get; }
    int HotbarSize { get; }
    IReadOnlyList<Slot> Slots { get; }
    IEnumerable<Slot> HotbarSlots { get; }
    IEnumerable<Slot> GeneralSlots { get; }
    Slot GetSlot(int index);
    int TryAdd(ItemStack stack);
    int Remove(string itemId, int count);
    int Count(string itemId);
    event EventHandler<InventoryChangedEventArgs>? Changed;
}
