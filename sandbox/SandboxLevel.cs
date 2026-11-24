using Godot;
using Pegasus.World;

namespace Pegasus.Sandbox;

/// <summary>Grey-box test scene for the shared world kit: walk, follow camera, interact.</summary>
public partial class SandboxLevel : Node3D
{
    [Export] public Label? InfoLabel { get; set; }
    [Export] public Interactable? Lantern { get; set; }
    [Export] public OmniLight3D? LanternLight { get; set; }
    [Export] public Interactable? Sign { get; set; }

    public override void _Ready()
    {
        GD.Print("Sandbox ready");
        if (Lantern != null) Lantern.Interacted += ToggleLantern;
        if (Sign != null) Sign.Interacted += () => Say("The sign reads: \"Grey boxes only. Art arrives later.\"");
    }

    private void ToggleLantern()
    {
        if (LanternLight == null) return;
        LanternLight.Visible = !LanternLight.Visible;
        Say(LanternLight.Visible ? "The lantern flickers on." : "The lantern goes dark.");
    }

    private void Say(string text)
    {
        GD.Print(text);
        if (InfoLabel != null) InfoLabel.Text = text;
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (e.IsActionPressed("pause")) GetTree().Quit();
    }
}
