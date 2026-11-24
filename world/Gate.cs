using System.Linq;
using Godot;
using Pegasus.Conditions;
using Pegasus.Core;

namespace Pegasus.World;

/// <summary>
/// Something that blocks the way until a condition holds, e.g. the dark underpass with
/// "guts >= 2 | item:flashlight". While closed it is solid and visible, and its child
/// Interactables (an <see cref="Examine"/> with the locked line) are on. Once open it
/// hides, stops colliding and turns them off.
/// </summary>
public partial class Gate : StaticBody3D
{
    [Export] public string OpenWhen { get; set; } = "";

    private Condition _condition = new Always();
    private uint _layer;

    public override void _Ready()
    {
        _condition = Condition.Parse(OpenWhen);
        _layer = CollisionLayer;
        Game.Changed += Refresh;
        Refresh();
    }

    public override void _ExitTree() => Game.Changed -= Refresh;

    private void Refresh()
    {
        var open = _condition.IsMet(Game.State);
        Visible = !open;
        CollisionLayer = open ? 0 : _layer;
        foreach (var spot in FindChildren("*", "", true, false).OfType<Interactable>()) spot.Enabled = !open;
    }
}
