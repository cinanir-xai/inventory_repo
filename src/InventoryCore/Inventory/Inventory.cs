namespace InventoryCore.Inventory;

using InventoryCore.Events;
using InventoryCore.Items;
using InventoryCore.Slots;

public class Inventory : IInventory
{
    private readonly Slot[] _slots;

    public int Capacity => _slots.Length;
    public int HotbarSize { get; }
    public IReadOnlyList<Slot> Slots => _slots;

    public IEnumerable<Slot> HotbarSlots
    {
        get
        {
            for (int i = 0; i < HotbarSize; i++) yield return _slots[i];
        }
    }

    public IEnumerable<Slot> GeneralSlots
    {
        get
        {
            for (int i = HotbarSize; i < _slots.Length; i++) yield return _slots[i];
        }
    }

    public event EventHandler<InventoryChangedEventArgs>? Changed;

    public Inventory(InventoryConfig config)
    {
        if (config == null) throw new ArgumentNullException(nameof(config));
        if (config.TotalSlots <= 0) throw new ArgumentOutOfRangeException(nameof(config));
        if (config.HotbarSize < 0 || config.HotbarSize > config.TotalSlots)
            throw new ArgumentOutOfRangeException(nameof(config));

        HotbarSize = config.HotbarSize;
        _slots = new Slot[config.TotalSlots];
        for (int i = 0; i < config.TotalSlots; i++)
        {
            if (config.RestrictedSlots.TryGetValue(i, out var cat))
                _slots[i] = new RestrictedSlot(i, cat);
            else if (i < config.HotbarSize)
                _slots[i] = new Slot(i, SlotKind.Hotbar);
            else
                _slots[i] = new Slot(i, SlotKind.General);
        }
    }

    public Slot GetSlot(int index)
    {
        if (index < 0 || index >= _slots.Length)
            throw new ArgumentOutOfRangeException(nameof(index));
        return _slots[index];
    }

    public int TryAdd(ItemStack incoming)
    {
        if (incoming == null) throw new ArgumentNullException(nameof(incoming));
        if (incoming.Count <= 0) return 0;

        var affected = new List<int>();
        var initial = incoming.Count;

        for (int i = 0; i < _slots.Length; i++)
        {
            if (incoming.Count == 0) break;
            var s = _slots[i];
            if (s.IsEmpty) continue;
            if (!s.Accepts(incoming)) continue;
            var existing = s.Stack!;
            var moved = existing.TryMerge(incoming);
            if (moved > 0) affected.Add(i);
        }

        for (int i = 0; i < _slots.Length; i++)
        {
            if (incoming.Count == 0) break;
            var s = _slots[i];
            if (!s.IsEmpty) continue;
            if (!s.Accepts(incoming)) continue;
            var placed = new ItemStack(
                incoming.Definition,
                incoming.Count,
                incoming.Properties.Values.Select(p => p.Clone()));
            s.Set(placed);
            incoming.SetCount(0);
            affected.Add(i);
        }

        var added = initial - incoming.Count;
        if (added > 0)
            Changed?.Invoke(this, new InventoryChangedEventArgs(affected, "TryAdd"));
        return added;
    }

    public int Remove(string itemId, int count)
    {
        if (string.IsNullOrEmpty(itemId)) throw new ArgumentException("itemId required", nameof(itemId));
        if (count <= 0) return 0;

        var affected = new List<int>();
        var remaining = count;

        for (int i = 0; i < _slots.Length; i++)
        {
            if (remaining == 0) break;
            var s = _slots[i];
            if (s.IsEmpty) continue;
            if (s.Stack!.Definition.Id != itemId) continue;
            var stack = s.Stack;
            var take = Math.Min(remaining, stack.Count);
            stack.SetCount(stack.Count - take);
            remaining -= take;
            if (stack.Count == 0)
                s.Set(null);
            affected.Add(i);
        }

        var removed = count - remaining;
        if (removed > 0)
            Changed?.Invoke(this, new InventoryChangedEventArgs(affected, "Remove"));
        return removed;
    }

    public int Count(string itemId)
    {
        int total = 0;
        foreach (var s in _slots)
        {
            if (s.IsEmpty) continue;
            if (s.Stack!.Definition.Id == itemId)
                total += s.Stack.Count;
        }
        return total;
    }

    protected void RaiseChanged(IReadOnlyList<int> affected, string reason)
    {
        Changed?.Invoke(this, new InventoryChangedEventArgs(affected, reason));
    }
}
