namespace InventoryCore.Registry;

using InventoryCore.Items;

public sealed class ItemRegistry
{
    private readonly Dictionary<string, ItemDefinition> _items = new();

    public void Register(ItemDefinition definition)
    {
        if (definition == null) throw new ArgumentNullException(nameof(definition));
        if (_items.ContainsKey(definition.Id))
            throw new InvalidOperationException($"item already registered: {definition.Id}");
        _items[definition.Id] = definition;
    }

    public ItemDefinition Get(string id)
    {
        if (!_items.TryGetValue(id, out var def))
            throw new KeyNotFoundException($"unknown item id: {id}");
        return def;
    }

    public bool TryGet(string id, out ItemDefinition? definition)
    {
        return _items.TryGetValue(id, out definition);
    }

    public IReadOnlyCollection<ItemDefinition> All => _items.Values;

    public int Count => _items.Count;
}
