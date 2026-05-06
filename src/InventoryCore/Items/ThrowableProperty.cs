namespace InventoryCore.Items;

public sealed class ThrowableProperty : IItemProperty
{
    public string Key => "throwable";
    public bool IsInstanceSpecific => false;
    public int Damage { get; }
    public int Range { get; }

    public ThrowableProperty(int damage, int range)
    {
        if (damage < 0) throw new ArgumentOutOfRangeException(nameof(damage));
        if (range < 0) throw new ArgumentOutOfRangeException(nameof(range));
        Damage = damage;
        Range = range;
    }

    public IItemProperty Clone() => new ThrowableProperty(Damage, Range);
}
