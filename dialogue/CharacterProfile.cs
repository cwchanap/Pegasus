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
    [Export] public Texture2D? Portrait { get; set; }
    /// <summary>Billboard sprite in 3D scenes.</summary>
    [Export] public Texture2D? Sprite { get; set; }
    /// <summary>Show bond pips in the name plate.</summary>
    [Export] public bool HasBond { get; set; } = true;
}
