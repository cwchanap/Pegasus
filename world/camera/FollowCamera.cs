using Godot;

namespace Pegasus.World;

/// <summary>
/// Tilted follow camera with a fixed yaw per scene. Finds the player by group
/// unless a target is set.
/// </summary>
public partial class FollowCamera : Camera3D
{
    [Export] public Node3D? Target { get; set; }
    [Export] public float Distance { get; set; } = 7f;
    [Export] public float Height { get; set; } = 4f;
    [Export] public float YawDegrees { get; set; } = 0f;
    [Export] public float LookHeight { get; set; } = 1f;
    [Export] public float Smoothing { get; set; } = 6f;

    public override void _Ready()
    {
        TopLevel = true;
        Target ??= GetTree().GetFirstNodeInGroup(Player.Group) as Node3D;
        if (Target != null) Snap();
    }

    public override void _Process(double delta)
    {
        if (Target == null) return;
        var weight = 1f - Mathf.Exp(-Smoothing * (float)delta);
        GlobalPosition = GlobalPosition.Lerp(DesiredPosition(), weight);
        LookAt(Target.GlobalPosition + Vector3.Up * LookHeight);
    }

    /// <summary>Jump straight to the follow position, e.g. after a scene change.</summary>
    public void Snap()
    {
        GlobalPosition = DesiredPosition();
        LookAt(Target!.GlobalPosition + Vector3.Up * LookHeight);
    }

    private Vector3 DesiredPosition()
    {
        var back = new Vector3(0, 0, Distance).Rotated(Vector3.Up, Mathf.DegToRad(YawDegrees));
        return Target!.GlobalPosition + back + Vector3.Up * Height;
    }
}
