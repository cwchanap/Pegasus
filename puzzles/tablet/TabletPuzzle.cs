using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Pegasus.Puzzles;

/// <summary>
/// The stone tablet in the shrine hall: five carved signs. Each sign is gated on
/// reading the old script (Knowledge, or Aoi's notes); until then they are only carvings.
/// Reading a sign writes its word beside it, and reading all five solves it. There is no
/// wrong try, so it has no rules class.
/// </summary>
public partial class TabletPuzzle : Puzzle
{
    private List<Hotspot> _signs = new();
    private readonly HashSet<Hotspot> _read = new();

    public override void _Ready()
    {
        _signs = FindChildren("Sign*", "", true, false).OfType<Hotspot>().ToList();
        if (Solved) _signs.ForEach(Reveal);
    }

    public override void Use(Hotspot spot, string? item)
    {
        // Aoi's notes held over a sign is just reading it with the notes.
        if (item == "aoi_notes") item = null;
        // Once read, any sign gives the whole rule again.
        if (item == null && Solved && _signs.Contains(spot))
        {
            Say("Tablet_Rule");
            return;
        }
        if (item != null || !_signs.Contains(spot) || Solved)
        {
            base.Use(spot, item);
            return;
        }
        Reveal(spot);
        if (_read.Count == _signs.Count) Solve("Tablet_Read");
        else Say(spot.YarnNode);
    }

    private void Reveal(Hotspot sign)
    {
        _read.Add(sign);
        sign.GetNode<CanvasItem>("Word").Visible = true;
    }
}
