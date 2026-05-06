namespace InventoryCore.Items;

public interface IItemProperty
{
    string Key { get; }
    bool IsInstanceSpecific { get; }
    IItemProperty Clone();
}
