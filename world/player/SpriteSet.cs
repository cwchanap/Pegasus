using Godot;

namespace Pegasus.World;

/// <summary>
/// One outfit's billboard sprites: front (walking toward the camera), back (walking away)
/// and side, drawn facing right and flipped for left. Saved under content/characters/.
/// </summary>
[GlobalClass]
public partial class SpriteSet : Resource
{
    [Export] public Texture2D? Front { get; set; }
    [Export] public Texture2D? Back { get; set; }
    [Export] public Texture2D? Side { get; set; }
}
