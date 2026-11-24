using Godot;
using Pegasus.Dialogue;

namespace Pegasus.World;

/// <summary>
/// Someone standing in a scene. Talking starts their Yarn node. With no node, Talk is
/// off and something else next to them (e.g. a hangout ActivitySpot) does the talking.
/// </summary>
public partial class Npc : Node3D
{
    [Export] public CharacterProfile? Profile { get; set; }
    [Export] public string YarnNode { get; set; } = "";
    [Export] public Sprite3D? Sprite { get; set; }
    [Export] public Interactable? Talk { get; set; }

    public override void _Ready()
    {
        if (Sprite != null && Profile?.Sprite != null) Sprite.Texture = Profile.Sprite;
        if (Talk == null) return;
        Talk.Enabled = YarnNode != "";
        Talk.Interacted += () => Conversation.Start(YarnNode);
    }
}
