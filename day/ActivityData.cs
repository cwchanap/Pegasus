using Godot;
using Pegasus.Core;

namespace Pegasus.Day;

/// <summary>
/// Something to do by day. It spends one slot and gives stats or money.
/// One .tres per activity under content/activities/; an ActivitySpot in a scene
/// offers it, and the place's LocationData lists it for the city map.
/// </summary>
[GlobalClass]
public partial class ActivityData : Resource
{
    [Export] public string Id { get; set; } = "";
    /// <summary>Panel title and map row, e.g. "Study at the library".</summary>
    [Export] public string Title { get; set; } = "";
    /// <summary>Prompt and button verb, e.g. "Study".</summary>
    [Export] public string Verb { get; set; } = "";
    /// <summary>Shown on the black screen while the time passes.</summary>
    [Export(PropertyHint.MultilineText)] public string Outcome { get; set; } = "";

    [ExportGroup("Gains")]
    [Export] public int Knowledge { get; set; }
    [Export] public int Guts { get; set; }
    [Export] public int Fitness { get; set; }
    [Export] public int Money { get; set; }

    public Gains Gains => new(Knowledge, Guts, Fitness, Money);
}
