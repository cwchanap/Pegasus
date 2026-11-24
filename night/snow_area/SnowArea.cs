using Godot;
using Pegasus.Core;
using Pegasus.UI;
using Pegasus.World;

namespace Pegasus.Night;

/// <summary>
/// The snow world at 3 AM. Warmth starts full (Items.MaxWarmth) and drains while he can
/// move; when it runs out he wakes in bed, which costs nothing but the rest of the night.
/// The home door ends the night early. Both go through DayCycle.WakeUp.
/// </summary>
public partial class SnowArea : Node3D
{
    private const string LightItem = "flashlight";

    [Export] public Interactable? HomeDoor { get; set; }
    [Export] public NightHud? Hud { get; set; }
    [Export] public Light3D? BikeLight { get; set; }

    private int _max;
    private float _warmth;
    private bool _over;

    private static bool HasLight => Game.State.HasItem(LightItem);

    public override void _Ready()
    {
        if (HomeDoor != null) HomeDoor.Interacted += () => End(null);
        _max = Items.MaxWarmth();
        _warmth = _max;
        if (BikeLight != null) BikeLight.Visible = false;
        Hud?.SetWarmth(_warmth, _max);
        Hud?.SetLight(HasLight ? Items.Name(LightItem) : null, false);
    }

    public override void _Process(double delta)
    {
        if (_over || !InputLock.IsFree) return;
        _warmth = Mathf.Max(0, _warmth - Warmth.DrainPerSecond * (float)delta);
        Hud?.SetWarmth(_warmth, _max);
        if (_warmth <= 0) End("Too cold. Everything goes white.");
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (!e.IsActionPressed("light") || !HasLight || BikeLight == null || !InputLock.IsFree) return;
        GetViewport().SetInputAsHandled();
        BikeLight.Visible = !BikeLight.Visible;
        Hud?.SetLight(Items.Name(LightItem), BikeLight.Visible);
    }

    private void End(string? why)
    {
        _over = true;
        DayCycle.WakeUp(why);
    }
}
