using Godot;
using Pegasus.World;

namespace Pegasus.Puzzles;

/// <summary>An Interactable in the 3D world that opens a puzzle scene in the close-up frame.</summary>
public partial class PuzzleSpot : Interactable
{
    [Export] public PackedScene? Puzzle { get; set; }

    public override void Interact()
    {
        base.Interact();
        if (Puzzle != null) CloseUp.Open(Puzzle);
    }
}
