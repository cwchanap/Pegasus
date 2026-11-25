using Godot;
using Godot.Collections;

namespace Pegasus.World;

/// <summary>
/// An area where tall scenery would hide him from the follow camera (under a bridge, say):
/// while the player is inside, the listed meshes fade out. Visual only, so collision stays.
/// </summary>
public partial class Cutaway : Area3D
{
    [Export] public Array<GeometryInstance3D> Fade { get; set; } = new();
    [Export(PropertyHint.Range, "0,1")] public float Transparency { get; set; } = 0.8f;

    private Tween? _tween;

    public override void _Ready()
    {
        BodyEntered += body => { if (body.IsInGroup(Player.Group)) To(Transparency); };
        BodyExited += body => { if (body.IsInGroup(Player.Group)) To(0f); };
    }

    private void To(float amount)
    {
        _tween?.Kill();
        _tween = CreateTween().SetParallel();
        foreach (var mesh in Fade)
            _tween.TweenProperty(mesh, GeometryInstance3D.PropertyName.Transparency.ToString(), amount, 0.2);
    }
}
