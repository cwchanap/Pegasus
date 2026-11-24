using Godot;
using Pegasus.Core;

namespace Pegasus.World;

/// <summary>
/// The one player controller for both worlds. Moves on the ground plane relative to
/// the active camera's facing, and shows a billboarded 2D sprite.
/// </summary>
public partial class Player : CharacterBody3D
{
    public const string Group = "player";

    [Export] public float Speed { get; set; } = 4.5f;
    [Export] public float Acceleration { get; set; } = 30f;
    [Export] public Sprite3D? Sprite { get; set; }

    private readonly float _gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");

    public override void _EnterTree() => AddToGroup(Group);

    public override void _PhysicsProcess(double delta)
    {
        var dt = (float)delta;
        var input = InputLock.IsFree
            ? Input.GetVector("move_left", "move_right", "move_forward", "move_back")
            : Vector2.Zero;

        var direction = CameraRelative(input);
        var target = direction * Speed;
        var velocity = Velocity;
        velocity.X = Mathf.MoveToward(velocity.X, target.X, Acceleration * dt);
        velocity.Z = Mathf.MoveToward(velocity.Z, target.Z, Acceleration * dt);
        velocity.Y = IsOnFloor() ? 0 : velocity.Y - _gravity * dt;
        Velocity = velocity;
        MoveAndSlide();

        if (Sprite != null && Mathf.Abs(input.X) > 0.1f) Sprite.FlipH = input.X < 0;
    }

    private Vector3 CameraRelative(Vector2 input)
    {
        if (input == Vector2.Zero) return Vector3.Zero;
        var camera = GetViewport().GetCamera3D();
        var basis = camera?.GlobalBasis ?? Basis.Identity;
        var forward = -basis.Z with { Y = 0 };
        var right = basis.X with { Y = 0 };
        var dir = right.Normalized() * input.X - forward.Normalized() * input.Y;
        return dir.LengthSquared() > 1 ? dir.Normalized() : dir;
    }
}
