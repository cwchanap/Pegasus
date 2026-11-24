using Godot;

namespace Pegasus.World;

/// <summary>
/// One thing he can carry. One .tres per item at content/items/{Id}.tres; GameState
/// keeps only the id. Warmth is how much the item adds to the night meter (gear).
/// </summary>
[GlobalClass]
public partial class ItemData : Resource
{
    [Export] public string Id { get; set; } = "";
    [Export] public string DisplayName { get; set; } = "";
    [Export(PropertyHint.MultilineText)] public string Description { get; set; } = "";
    /// <summary>Shop price in yen; 0 if it isn't sold.</summary>
    [Export] public int Price { get; set; }
    [Export] public int Warmth { get; set; }
    /// <summary>Placeholder swatch until the A6 icon lands in Icon.</summary>
    [Export] public Color IconColor { get; set; } = Colors.White;
    [Export] public Texture2D? Icon { get; set; }
}
