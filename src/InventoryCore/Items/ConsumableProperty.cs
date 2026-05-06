namespace InventoryCore.Items;

public sealed class ConsumableProperty : IItemProperty
{
    public string Key => "consumable";
    public bool IsInstanceSpecific => false;
    public int HungerRestore { get; }
    public int HealthRestore { get; }

    public ConsumableProperty(int hungerRestore, int healthRestore)
    {
        HungerRestore = hungerRestore;
        HealthRestore = healthRestore;
    }

    public IItemProperty Clone() => new ConsumableProperty(HungerRestore, HealthRestore);
}
