using Godot;
using Pegasus.Conditions;
using Pegasus.Core;

namespace Pegasus.Puzzles;

/// <summary>
/// A clickable area in a close-up view, fitted over a painted object. Art drawn on it (a dial
/// face, a carved glyph) is a child with mouse_filter = Ignore and show_behind_parent, so the
/// focus ring stays on top. By default a click plays <see cref="YarnNode"/> through
/// the dialogue box; a puzzle can handle it instead in <see cref="Puzzle.Use"/>. With
/// <see cref="Requires"/> set, a click plays <see cref="LockedYarnNode"/> until it holds.
/// </summary>
public partial class Hotspot : Button
{
    /// <summary>Name shown in the tag beside it while it has focus, e.g. "Offering box".</summary>
    [Export] public string Label { get; set; } = "";

    /// <summary>What he sees when he looks at it. Set a clue flag in the node for the notebook.</summary>
    [Export] public string YarnNode { get; set; } = "";

    /// <summary>Condition to use it, e.g. "knowledge >= 2 | item:aoi_notes". Empty: always.</summary>
    [Export] public string Requires { get; set; } = "";

    /// <summary>Played instead while <see cref="Requires"/> isn't met.</summary>
    [Export] public string LockedYarnNode { get; set; } = "";

    private Condition? _requires;

    public bool IsOpen => (_requires ??= Condition.Parse(Requires)).IsMet(Game.State);

    public override void _Ready()
    {
        ThemeTypeVariation = "Hotspot";
        MouseEntered += GrabFocus;
    }
}
