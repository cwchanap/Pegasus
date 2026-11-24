using Godot;
using Pegasus.Conditions;
using Pegasus.Core;
using Pegasus.Dialogue;

namespace Pegasus.World;

/// <summary>
/// Something the player can examine or use. Put it on an Area3D with a collision
/// shape; the player's Interactor picks the nearest one in range. With
/// <see cref="Requires"/> set, it only works while that condition holds; otherwise it
/// plays <see cref="LockedYarnNode"/>, which may hint at the kind of help he needs.
/// </summary>
public partial class Interactable : Area3D
{
    [Signal] public delegate void InteractedEventHandler();

    /// <summary>Verb shown in the prompt, e.g. "Examine", "Open", "Talk".</summary>
    [Export] public string Prompt { get; set; } = "Examine";

    /// <summary>Height above the origin where the prompt floats.</summary>
    [Export] public float PromptHeight { get; set; } = 1.5f;

    [Export] public bool Enabled { get; set; } = true;

    /// <summary>Condition to use it, e.g. "flag:lanterns_lit". Empty: always.</summary>
    [Export] public string Requires { get; set; } = "";

    /// <summary>Played instead while <see cref="Requires"/> isn't met.</summary>
    [Export] public string LockedYarnNode { get; set; } = "";

    private Condition? _requires;

    public Vector3 PromptPosition => GlobalPosition + Vector3.Up * PromptHeight;

    public bool IsOpen => (_requires ??= Condition.Parse(Requires)).IsMet(Game.State);

    public void Interact()
    {
        if (!IsOpen)
        {
            Conversation.Start(LockedYarnNode);
            return;
        }
        EmitSignal(SignalName.Interacted);
        OnInteract();
    }

    /// <summary>What a subclass does once the requirement is met.</summary>
    protected virtual void OnInteract() { }
}
