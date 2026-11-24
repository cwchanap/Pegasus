using Godot;

namespace Pegasus.World;

/// <summary>
/// Tilted follow camera. Its rotation is fixed per scene (from Distance, Height and
/// Yaw), so movement directions stay steady; only the position follows the target,
/// using the target's interpolated transform so it stays smooth on high-refresh screens.
/// </summary>
public partial class FollowCamera : Camera3D
{
    [Export] public Node3D? Target { get; set; }
    [Export] public float Distance { get; set; } = 7f;
    [Export] public float Height { get; set; } = 4f;
    [Export] public float YawDegrees { get; set; } = 0f;
    [Export] public float LookHeight { get; set; } = 1f;
    [Export] public float Smoothing { get; set; } = 8f;

    private Vector3 Offset => new Vector3(0, Height, Distance).Rotated(Vector3.Up, Mathf.DegToRad(YawDegrees));

    public override void _Ready()
    {
        TopLevel = true;
        // Moved in _Process from the interpolated target, so it must not be interpolated again.
        PhysicsInterpolationMode = PhysicsInterpolationModeEnum.Off;
        Target ??= GetTree().GetFirstNodeInGroup(Player.Group) as Node3D;
        if (Target != null) Snap();
    }

    public override void _Process(double delta)
    {
        if (Target == null) return;
        var weight = 1f - Mathf.Exp(-Smoothing * (float)delta);
        GlobalPosition = GlobalPosition.Lerp(TargetPosition() + Offset, weight);
    }

    /// <summary>Jump straight to the follow position and set the fixed rotation, e.g. after a scene change.</summary>
    public void Snap()
    {
        var target = TargetPosition();
        GlobalPosition = target + Offset;
        LookAt(target + Vector3.Up * LookHeight);
    }

    private Vector3 TargetPosition() => Target!.GetGlobalTransformInterpolated().Origin;
}
