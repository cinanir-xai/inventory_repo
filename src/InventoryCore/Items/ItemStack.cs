namespace InventoryCore.Items;

public sealed class ItemStack
{
    public ItemDefinition Definition { get; }
    public int Count { get; private set; }
    public Dictionary<string, IItemProperty> Properties { get; }

    public ItemStack(ItemDefinition definition, int count = 1, IEnumerable<IItemProperty>? properties = null)
    {
        if (definition == null) throw new ArgumentNullException(nameof(definition));
        if (count < 1) throw new ArgumentOutOfRangeException(nameof(count));
        if (count > definition.MaxStack)
            throw new ArgumentOutOfRangeException(nameof(count), $"count exceeds maxStack {definition.MaxStack}");

        Definition = definition;
        Count = count;
        Properties = new Dictionary<string, IItemProperty>();

        if (properties != null)
        {
            foreach (var p in properties)
                Properties[p.Key] = p;
        }
        else
        {
            foreach (var kv in definition.DefaultProperties)
                Properties[kv.Key] = kv.Value.Clone();
        }
    }

    public bool HasInstanceState => Properties.Values.Any(p => p.IsInstanceSpecific);

    public int FreeSpace => Definition.MaxStack - Count;

    public bool CanMergeWith(ItemStack other)
    {
        if (other == null) return false;
        if (!ReferenceEquals(Definition, other.Definition)) return false;
        if (HasInstanceState || other.HasInstanceState) return false;
        return true;
    }

    public int TryMerge(ItemStack other)
    {
        if (!CanMergeWith(other)) return 0;
        var moved = Math.Min(FreeSpace, other.Count);
        if (moved <= 0) return 0;
        Count += moved;
        other.Count -= moved;
        return moved;
    }

    public ItemStack? Split(int amount)
    {
        if (amount < 1) throw new ArgumentOutOfRangeException(nameof(amount));
        if (amount >= Count) return null;
        Count -= amount;
        return new ItemStack(Definition, amount, Properties.Values.Select(p => p.Clone()));
    }

    public ItemStack Clone() =>
        new ItemStack(Definition, Count, Properties.Values.Select(p => p.Clone()));

    public void SetCount(int newCount)
    {
        if (newCount < 0) throw new ArgumentOutOfRangeException(nameof(newCount));
        if (newCount > Definition.MaxStack)
            throw new ArgumentOutOfRangeException(nameof(newCount));
        Count = newCount;
    }
}
