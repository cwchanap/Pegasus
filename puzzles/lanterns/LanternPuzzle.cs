using System.Linq;
using Godot;
using Pegasus.Core;

namespace Pegasus.Puzzles;

/// <summary>
/// The shrine lanterns (rules in <see cref="LanternPath"/>). Clicking a lit lantern puts it
/// out. Using the matches on a dark one lights it if it is next from the gate, or the flame
/// dies with no other cost. With nothing in hand he looks at it: the soot on each lantern,
/// heaviest by the gate, is the clue in the night world.
/// </summary>
public partial class LanternPuzzle : Puzzle
{
    private const string Matches = "matchbox";
    private static readonly Color LitColor = new(1f, 0.8f, 0.42f);
    private static readonly Color DarkColor = new(0.13f, 0.15f, 0.22f);

    private LanternPath _path = new();
    private Hotspot[] _lanterns = System.Array.Empty<Hotspot>();

    public override void _Ready()
    {
        _lanterns = Enumerable.Range(1, LanternPath.Count).Select(i => GetNode<Hotspot>($"LanternPath/Lantern{i}")).ToArray();
        _path = new LanternPath(Solved);
        Refresh();
    }

    public override void Use(Hotspot spot, string? item)
    {
        var i = System.Array.IndexOf(_lanterns, spot);
        if (i < 0 || Solved)
        {
            base.Use(spot, item);
            return;
        }
        if (_path.Lit[i])
        {
            _path.Snuff(i);
            Refresh();
        }
        else if (item != Matches)
        {
            base.Use(spot, item);
        }
        else if (!_path.TryLight(i))
        {
            Say("Lanterns_Gutter");
        }
        else
        {
            Refresh();
            if (_path.Solved) Solve("Lanterns_Solved");
        }
    }

    private void Refresh()
    {
        for (var i = 0; i < _lanterns.Length; i++)
        {
            _lanterns[i].GetNode<ColorRect>("Lamp").Color = _path.Lit[i] ? LitColor : DarkColor;
            _lanterns[i].GetNode<CanvasItem>("Glow").Visible = _path.Lit[i];
        }
    }
}
