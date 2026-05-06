namespace InventoryCore.Cli;

using InventoryCore.Inventory;
using InventoryCore.Items;
using InventoryCore.Registry;

public static class Program
{
    public static int Main(string[] args)
    {
        var registry = DefaultItems.CreateDefault();
        var inventory = new Inventory(InventoryConfig.Default());

        Console.WriteLine("InventoryCore CLI");
        Console.WriteLine($"slots={inventory.Capacity} hotbar={inventory.HotbarSize}");
        Console.WriteLine("commands: add <itemId> <count>, remove <itemId> <count>, count <itemId>, list, items, quit");

        while (true)
        {
            Console.Write("> ");
            var line = Console.ReadLine();
            if (line == null) return 0;
            var parts = line.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length == 0) continue;

            try
            {
                switch (parts[0])
                {
                    case "quit":
                    case "exit":
                        return 0;
                    case "add":
                        HandleAdd(parts, registry, inventory);
                        break;
                    case "remove":
                        HandleRemove(parts, inventory);
                        break;
                    case "count":
                        HandleCount(parts, inventory);
                        break;
                    case "list":
                        HandleList(inventory);
                        break;
                    case "items":
                        HandleItems(registry);
                        break;
                    default:
                        Console.WriteLine($"unknown command: {parts[0]}");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"error: {ex.Message}");
            }
        }
    }

    private static void HandleAdd(string[] parts, ItemRegistry registry, Inventory inv)
    {
        if (parts.Length != 3) { Console.WriteLine("usage: add <itemId> <count>"); return; }
        var def = registry.Get(parts[1]);
        var count = int.Parse(parts[2]);
        var totalAdded = 0;
        var remaining = count;
        while (remaining > 0)
        {
            var thisStack = Math.Min(remaining, def.MaxStack);
            var stack = new ItemStack(def, thisStack);
            var added = inv.TryAdd(stack);
            totalAdded += added;
            if (added < thisStack) break;
            remaining -= thisStack;
        }
        Console.WriteLine($"added {totalAdded}/{count}");
    }

    private static void HandleRemove(string[] parts, Inventory inv)
    {
        if (parts.Length != 3) { Console.WriteLine("usage: remove <itemId> <count>"); return; }
        var removed = inv.Remove(parts[1], int.Parse(parts[2]));
        Console.WriteLine($"removed {removed}");
    }

    private static void HandleCount(string[] parts, Inventory inv)
    {
        if (parts.Length != 2) { Console.WriteLine("usage: count <itemId>"); return; }
        Console.WriteLine(inv.Count(parts[1]));
    }

    private static void HandleList(Inventory inv)
    {
        for (int i = 0; i < inv.Capacity; i++)
        {
            var s = inv.GetSlot(i);
            if (s.IsEmpty) continue;
            Console.WriteLine($"[{i}] {s.Stack!.Definition.Id} x{s.Stack.Count}");
        }
    }

    private static void HandleItems(ItemRegistry registry)
    {
        foreach (var def in registry.All)
            Console.WriteLine($"{def.Id} ({def.Category}) max={def.MaxStack}");
    }
}
