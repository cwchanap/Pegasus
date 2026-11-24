using Godot;
using Pegasus.Dialogue;

namespace Pegasus.World;

/// <summary>Something to look at: plays its Yarn node through the dialogue box.</summary>
public partial class Examine : Interactable
{
    [Export] public string YarnNode { get; set; } = "";

    protected override void OnInteract() => Conversation.Start(YarnNode);
}
