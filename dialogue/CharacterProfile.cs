using Godot;

namespace Pegasus.Dialogue;

/// <summary>
/// Who someone is in dialogue and in the world. Saved as a .tres under
/// content/characters/. A Yarn line "Aoi: ..." finds the profile whose DisplayName is "Aoi".
/// </summary>
[GlobalClass]
public partial class CharacterProfile : Resource
{
    /// <summary>Key in GameState.Bonds and in conditions (bond:aoi).</summary>
    [Export] public string Id { get; set; } = "";
    [Export] public string DisplayName { get; set; } = "";
    /// <summary>Shown beside the name, e.g. "Folklore club".</summary>
    [Export] public string Subtitle { get; set; } = "";
    [Export] public Color Color { get; set; } = new(0.98f, 0.36f, 0.24f);
    /// <summary>Neutral portrait, for untagged lines and any mood without its own.</summary>
    [Export] public Texture2D? Portrait { get; set; }
    [Export] public Texture2D? PortraitSmile { get; set; }
    [Export] public Texture2D? PortraitWorried { get; set; }
    /// <summary>Billboard sprite in 3D scenes, facing front (people stand still in their places).</summary>
    [Export] public Texture2D? Sprite { get; set; }
    /// <summary>Show bond pips in the name plate.</summary>
    [Export] public bool HasBond { get; set; } = true;

    public Texture2D? PortraitFor(Mood mood) => mood switch
    {
        Mood.Smile => PortraitSmile ?? Portrait,
        Mood.Worried => PortraitWorried ?? Portrait,
        _ => Portrait,
    };
}
