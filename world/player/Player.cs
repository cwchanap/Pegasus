using Godot;
using Pegasus.Core;

namespace Pegasus.World;

/// <summary>
/// The one player controller for both worlds. Moves on the ground plane relative to
/// the active camera's facing, and shows a billboarded 2D sprite: the outfit Wardrobe
/// picks, facing the way he walks, bobbing once a step.
/// </summary>
public partial class Player : CharacterBody3D
{
    public const string Group = "player";

    [Export] public float Speed { get; set; } = 4.5f;
    [Export] public float Acceleration { get; set; } = 30f;
    [Export] public Sprite3D? Sprite { get; set; }
    [Export] public SpriteSet? Summer { get; set; }
    [Export] public SpriteSet? Coat { get; set; }
    /// <summary>Footstep sound for this ground (floor by day, snow at night).</summary>
    [Export] public Sfx Step { get; set; } = Sfx.StepFloor;

    private const float StepLength = 1.4f;
    private const float BobHeight = 0.05f;
    private float _sinceStep;

    private enum Facing { Front, Back, Side }
    private SpriteSet? _outfit;
    private float _spriteY, _lift;

    private readonly float _gravity = (float)ProjectSettings.GetSetting("physics/3d/default_gravity");

    public override void _EnterTree() => AddToGroup(Group);

    public override void _Ready()
    {
        _outfit = Wardrobe.Wears(Game.State) == Outfit.Coat ? Coat : Summer;
        if (Sprite == null) return;
        _spriteY = Sprite.Position.Y;
        Face(Facing.Front, left: false);
    }

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
        Footsteps(dt);
        Animate(input, dt);
    }

    private void Animate(Vector2 input, float dt)
    {
        if (Sprite == null) return;
        if (input.LengthSquared() > 0.01f)
        {
            // Face the way he actually goes (sliding along a wall shows the slide), or the way he
            // pushes when he is stuck. On screen, +Y is toward the camera, so it shows his front.
            var (right, forward) = CameraAxes();
            var moved = GetRealVelocity();
            var onScreen = new Vector2(moved.Dot(right), -moved.Dot(forward));
            var dir = onScreen.LengthSquared() > 0.25f ? onScreen : input;
            Face(Mathf.Abs(dir.X) >= Mathf.Abs(dir.Y) ? Facing.Side : dir.Y > 0 ? Facing.Front : Facing.Back, dir.X < 0);
        }

        // One gentle bob per step, landing as the footstep plays; it settles when he stops.
        var walking = IsOnFloor() && new Vector2(Velocity.X, Velocity.Z).Length() >= 0.5f;
        var target = walking ? Mathf.Sin(Mathf.Pi * _sinceStep / StepLength) * BobHeight : 0;
        _lift = Mathf.MoveToward(_lift, target, BobHeight * 12 * dt);
        Sprite.Position = Sprite.Position with { Y = _spriteY + _lift };
    }

    private void Face(Facing facing, bool left)
    {
        if (Sprite == null || _outfit == null) return;
        Sprite.Texture = facing switch
        {
            Facing.Front => _outfit.Front,
            Facing.Back => _outfit.Back,
            _ => _outfit.Side,
        };
        Sprite.FlipH = facing == Facing.Side && left;
    }

    private void Footsteps(float dt)
    {
        var speed = new Vector2(Velocity.X, Velocity.Z).Length();
        if (!IsOnFloor() || speed < 0.5f) { _sinceStep = StepLength * 0.7f; return; } // first step comes quickly
        _sinceStep += speed * dt;
        if (_sinceStep < StepLength) return;
        _sinceStep = 0;
        AudioService.PlaySfx(Step);
    }

    private Vector3 CameraRelative(Vector2 input)
    {
        if (input == Vector2.Zero) return Vector3.Zero;
        var (right, forward) = CameraAxes();
        var dir = right * input.X - forward * input.Y;
        return dir.LengthSquared() > 1 ? dir.Normalized() : dir;
    }

    /// <summary>The active camera's right and forward, flat on the ground.</summary>
    private (Vector3 Right, Vector3 Forward) CameraAxes()
    {
        var basis = GetViewport().GetCamera3D()?.GlobalBasis ?? Basis.Identity;
        return ((basis.X with { Y = 0 }).Normalized(), (-basis.Z with { Y = 0 }).Normalized());
    }
}
