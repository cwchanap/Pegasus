using Godot;
using Pegasus.Dialogue;

namespace Pegasus.World;

/// <summary>Someone standing in a scene. Talking starts their Yarn node.</summary>
public partial class Npc : Node3D
{
    [Export] public CharacterProfile? Profile { get; set; }
    [Export] public string YarnNode { get; set; } = "";
    [Export] public Sprite3D? Sprite { get; set; }
    [Export] public Interactable? Talk { get; set; }

    public override void _Ready()
    {
        if (Sprite != null && Profile?.Sprite != null) Sprite.Texture = Profile.Sprite;
        if (Talk != null) Talk.Interacted += () => Conversation.Start(YarnNode);
    }
}
