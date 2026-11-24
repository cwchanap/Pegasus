using Godot;
using Pegasus.World;

namespace Pegasus.UI;

/// <summary>
/// "[E] Examine" label that floats over whatever the player's Interactor has focused.
/// Drop it into any scene that has a Player.
/// </summary>
public partial class InteractPrompt : Control
{
    [Export] public Label? VerbLabel { get; set; }

    private Interactable? _target;

    public override void _Ready()
    {
        Visible = false;
        var player = GetTree().GetFirstNodeInGroup(Player.Group) as Node;
        var interactor = player?.GetNodeOrNull<Interactor>("Interactor");
        if (interactor != null) interactor.FocusChanged += OnFocusChanged;
    }

    private void OnFocusChanged(Interactable? focused)
    {
        _target = focused;
        if (VerbLabel != null && focused != null) VerbLabel.Text = focused.Prompt;
    }

    public override void _Process(double delta)
    {
        var camera = GetViewport().GetCamera3D();
        if (_target == null || !IsInstanceValid(_target) || camera == null || camera.IsPositionBehind(_target.PromptPosition))
        {
            Visible = false;
            return;
        }
        Visible = true;
        Position = camera.UnprojectPosition(_target.PromptPosition) - Size / 2;
    }
}
