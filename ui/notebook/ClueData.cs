using Godot;

namespace Pegasus.UI;

/// <summary>
/// One notebook entry, in content/clues/{flag}.tres: the file name is the clue flag
/// (clue_*) that writes it. The entry records what he saw or heard, standing alone, with
/// no objectives or hints.
/// </summary>
[GlobalClass]
public partial class ClueData : Resource
{
    /// <summary>Heading, e.g. "Shrine · Lanterns".</summary>
    [Export] public string Title { get; set; } = "";

    [Export(PropertyHint.MultilineText)] public string Text { get; set; } = "";

    /// <summary>Sort key; entries group by place (shrine 10s, underpass 20s, Kissa Mori 30s).</summary>
    [Export] public int Order { get; set; }

    /// <summary>Heard by day (from a friend) rather than seen at night; colours the heading.</summary>
    [Export] public bool ByDay { get; set; }
}
