using Godot;
using Pegasus.World;

namespace Pegasus.Day;

/// <summary>An Interactable that opens a shop. The DayLocation hands it to the HUD.</summary>
public partial class ShopSpot : Interactable
{
    [Export] public ShopData? Shop { get; set; }

    public override void _Ready() => Prompt = "Browse";
}
