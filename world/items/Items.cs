using System.Collections.Generic;
using Godot;
using Pegasus.Core;

namespace Pegasus.World;

/// <summary>Looks up item data by id (content/items/{id}.tres).</summary>
public static class Items
{
    private const string Folder = "res://content/items/";
    private static readonly Dictionary<string, ItemData?> Cache = new();

    public static ItemData? Get(string id)
    {
        if (Cache.TryGetValue(id, out var cached)) return cached;
        var path = $"{Folder}{id}.tres";
        var item = ResourceLoader.Exists(path) ? GD.Load<ItemData>(path) : null;
        if (item == null) GD.PushWarning($"Items: no item data for '{id}' (expected {path}).");
        return Cache[id] = item;
    }

    public static string Name(string id) => Get(id)?.DisplayName ?? id;

    public static (string Name, int Warmth) WarmthOf(string id) => (Name(id), Get(id)?.Warmth ?? 0);

    /// <summary>Tonight's full Warmth for the current state.</summary>
    public static int MaxWarmth() => Warmth.Max(Game.State, WarmthOf);
}
