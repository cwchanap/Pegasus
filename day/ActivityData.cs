using Godot;
using Pegasus.Core;
using Pegasus.Dialogue;

namespace Pegasus.Day;

/// <summary>
/// Something to do by day. It spends one slot and gives stats, money or bond.
/// One .tres per activity under content/activities/; an ActivitySpot in a scene
/// offers it, and the place's LocationData lists it for the city map.
/// A hangout names a character (With) and a Yarn scene: it plays node
/// "{YarnScene}{level}" for bond levels 1 to 3, then "{YarnScene}Again".
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

    [ExportGroup("Hangout")]
    [Export] public CharacterProfile? With { get; set; }
    [Export] public int Bond { get; set; }
    /// <summary>Yarn node prefix, e.g. "Aoi_Hangout".</summary>
    [Export] public string YarnScene { get; set; } = "";

    public Gains Gains => new(Knowledge, Guts, Fitness, Money, With?.Id ?? "", Bond);

    public string Describe(GameState s) => Gains.Describe(s, With?.DisplayName);

    /// <summary>The Yarn node to play now, or "" for a plain activity.</summary>
    public string SceneFor(GameState s)
    {
        if (YarnScene == "") return "";
        var level = (With == null ? 0 : s.GetBond(With.Id)) + 1;
        return level <= GameState.MaxBond ? $"{YarnScene}{level}" : $"{YarnScene}Again";
    }
}
