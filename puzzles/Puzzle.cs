using System.Collections.Generic;
using System.Linq;
using Godot;
using Pegasus.Core;

namespace Pegasus.Puzzles;

/// <summary>
/// The puzzle contract. A puzzle is a scene whose root derives from this, opened in the
/// <see cref="CloseUp"/> frame. Each child Control is one view (a wall), named for the
/// title ("LanternPath" shows as "LANTERN PATH"); the frame shows one at a time with
/// arrows between them. Hotspots inside the views call <see cref="Use"/>. Keep the rules
/// in a pure class under puzzles/rules so tests can check them. Partial progress lives in
/// the scene and resets when he steps back; only <see cref="SolvedFlag"/> is saved.
/// </summary>
public partial class Puzzle : Control
{
    private const string NothingNode = "CloseUp_Nothing";

    /// <summary>Place name for the title bar, e.g. "Shrine".</summary>
    [Export] public string Title { get; set; } = "";

    /// <summary>Flag set in GameState once solved. The 3D world reads it to show the result.</summary>
    [Export] public string SolvedFlag { get; set; } = "";

    public bool Solved => SolvedFlag != "" && Game.State.HasFlag(SolvedFlag);

    public List<Control> Views => GetChildren().OfType<Control>().ToList();

    internal CloseUp? Frame { get; set; }

    /// <summary>
    /// He clicked <paramref name="spot"/>, holding <paramref name="item"/> (an item id) or
    /// nothing. By default he looks at it, or nothing happens if he holds an item.
    /// </summary>
    public virtual void Use(Hotspot spot, string? item) => Say(item == null ? spot.YarnNode : NothingNode);

    /// <summary>Play a Yarn node through the dialogue box, over the close-up.</summary>
    protected void Say(string node) => Frame?.Say(node);

    /// <summary>Mark it solved, play <paramref name="node"/>, then step back to the world.</summary>
    protected void Solve(string node)
    {
        if (SolvedFlag != "") Game.State.SetFlag(SolvedFlag);
        Frame?.Say(node, closeAfter: true);
    }
}
