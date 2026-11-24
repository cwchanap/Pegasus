using Godot;

namespace Pegasus.World;

/// <summary>
/// Something the player can examine or use. Put it on an Area3D with a collision
/// shape; the player's Interactor picks the nearest one in range.
/// </summary>
public partial class Interactable : Area3D
{
    [Signal] public delegate void InteractedEventHandler();

    /// <summary>Verb shown in the prompt, e.g. "Examine", "Open", "Talk".</summary>
    [Export] public string Prompt { get; set; } = "Examine";

    /// <summary>Height above the origin where the prompt floats.</summary>
    [Export] public float PromptHeight { get; set; } = 1.5f;

    [Export] public bool Enabled { get; set; } = true;

    public Vector3 PromptPosition => GlobalPosition + Vector3.Up * PromptHeight;

    public virtual void Interact() => EmitSignal(SignalName.Interacted);
}
