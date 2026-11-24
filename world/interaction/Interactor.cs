using System.Collections.Generic;
using System.Linq;
using Godot;
using Pegasus.Core;

namespace Pegasus.World;

/// <summary>
/// Area3D on the player. Tracks interactables in range, focuses the nearest one,
/// and triggers it on the "interact" action.
/// </summary>
public partial class Interactor : Area3D
{
    [Signal] public delegate void FocusChangedEventHandler(Interactable? focused);

    public Interactable? Focused { get; private set; }

    private readonly List<Interactable> _inRange = new();
    private Player? _player;

    public override void _Ready()
    {
        _player = GetParent() as Player;
        AreaEntered += area => { if (area is Interactable i) _inRange.Add(i); };
        AreaExited += area => { if (area is Interactable i) _inRange.Remove(i); };
    }

    public override void _PhysicsProcess(double delta)
    {
        var canAct = _player?.Controllable ?? true;
        var nearest = canAct
            ? _inRange
                .Where(i => IsInstanceValid(i) && i.Enabled)
                .OrderBy(i => i.GlobalPosition.DistanceSquaredTo(GlobalPosition))
                .FirstOrDefault()
            : null;
        if (nearest == Focused) return;
        Focused = nearest;
        EmitSignal(SignalName.FocusChanged, Focused!);
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (Focused == null || !e.IsActionPressed("interact") || SceneRouter.Instance?.Busy == true) return;
        GetViewport().SetInputAsHandled();
        Focused.Interact();
    }
}
