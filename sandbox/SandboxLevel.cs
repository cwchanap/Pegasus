using Godot;

namespace Pegasus.Sandbox;

/// <summary>Grey-box test scene. Proves the C# build, input map and 3D setup work.</summary>
public partial class SandboxLevel : Node3D
{
    [Export] public Label? InputLabel { get; set; }

    public override void _Ready()
    {
        GD.Print("Sandbox ready");
    }

    public override void _Process(double delta)
    {
        if (InputLabel == null) return;
        var move = Input.GetVector("move_left", "move_right", "move_forward", "move_back");
        InputLabel.Text = $"move {move}  interact {Input.IsActionPressed("interact")}  menu {Input.IsActionPressed("menu")}";
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (e.IsActionPressed("pause")) GetTree().Quit();
    }
}
