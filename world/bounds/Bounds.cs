using Godot;

namespace Pegasus.World;

/// <summary>
/// Invisible walls around a rectangular walkable area, centred on this node.
/// Drop one into any scene with open edges so the player cannot walk off the world.
/// </summary>
public partial class Bounds : StaticBody3D
{
    /// <summary>Walkable width (x) and depth (z).</summary>
    [Export] public Vector2 Size { get; set; } = new(10, 10);
    [Export] public float Height { get; set; } = 4;

    private const float Thickness = 1;

    public override void _Ready()
    {
        var half = Size / 2 + Vector2.One * (Thickness / 2);
        AddWall(new Vector3(0, 0, -half.Y), new Vector3(Size.X + 2 * Thickness, Height, Thickness));
        AddWall(new Vector3(0, 0, half.Y), new Vector3(Size.X + 2 * Thickness, Height, Thickness));
        AddWall(new Vector3(-half.X, 0, 0), new Vector3(Thickness, Height, Size.Y));
        AddWall(new Vector3(half.X, 0, 0), new Vector3(Thickness, Height, Size.Y));
    }

    private void AddWall(Vector3 at, Vector3 size) => AddChild(new CollisionShape3D
    {
        Shape = new BoxShape3D { Size = size },
        Position = at + Vector3.Up * size.Y / 2,
    });
}
