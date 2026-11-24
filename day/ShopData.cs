using Godot;
using Godot.Collections;
using Pegasus.World;

namespace Pegasus.Day;

/// <summary>A counter that sells items. Browsing is free; prices are on the items.</summary>
[GlobalClass]
public partial class ShopData : Resource
{
    [Export] public string Id { get; set; } = "";
    [Export] public string Title { get; set; } = "";
    [Export] public Array<ItemData> Stock { get; set; } = new();
}
