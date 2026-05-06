namespace InventoryCore.Items;

public sealed class ItemDefinition
{
    public string Id { get; }
    public string DisplayName { get; }
    public ItemCategory Category { get; }
    public int MaxStack { get; }
    public float Weight { get; }
    public IReadOnlyList<string> Tags { get; }
    public IReadOnlyDictionary<string, IItemProperty> DefaultProperties { get; }

    public ItemDefinition(
        string id,
        string displayName,
        ItemCategory category,
        int maxStack,
        float weight,
        IEnumerable<string>? tags = null,
        IEnumerable<IItemProperty>? defaultProperties = null)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("id required", nameof(id));
        if (string.IsNullOrWhiteSpace(displayName)) throw new ArgumentException("displayName required", nameof(displayName));
        if (maxStack <= 0) throw new ArgumentOutOfRangeException(nameof(maxStack));
        if (weight < 0) throw new ArgumentOutOfRangeException(nameof(weight));

        Id = id;
        DisplayName = displayName;
        Category = category;
        MaxStack = maxStack;
        Weight = weight;
        Tags = (tags ?? Array.Empty<string>()).ToList().AsReadOnly();
        DefaultProperties = (defaultProperties ?? Array.Empty<IItemProperty>())
            .ToDictionary(p => p.Key, p => p);
    }
}
