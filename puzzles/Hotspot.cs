using Godot;

namespace Pegasus.Puzzles;

/// <summary>
/// A clickable area in a close-up view. Put the placeholder (later illustrated) art inside
/// it with mouse_filter = Ignore. By default a click plays <see cref="YarnNode"/> through
/// the dialogue box; a puzzle can handle it instead in <see cref="Puzzle.Use"/>.
/// </summary>
public partial class Hotspot : Button
{
    /// <summary>Name shown in the tag beside it while it has focus, e.g. "Offering box".</summary>
    [Export] public string Label { get; set; } = "";

    /// <summary>What he sees when he looks at it. Set a clue flag in the node for the notebook.</summary>
    [Export] public string YarnNode { get; set; } = "";

    public override void _Ready()
    {
        ThemeTypeVariation = "Hotspot";
        MouseEntered += GrabFocus;
    }
}
