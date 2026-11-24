using Godot;
using Pegasus.Core;
using Pegasus.Dialogue;
using Pegasus.UI;
using Pegasus.World;

namespace Pegasus.Night;

/// <summary>
/// The snow world at 3 AM. Warmth starts full (Items.MaxWarmth) and drains while he can
/// move; when it runs out he wakes in bed, which costs nothing but the rest of the night.
/// The home door ends the night early. Both go through DayCycle.WakeUp. The first night
/// opens with a few lines of narration and one tip about Warmth.
/// </summary>
public partial class SnowArea : Node3D
{
    private const string LightItem = "flashlight";
    private const float LowWarmth = 0.25f;

    [Export] public Interactable? HomeDoor { get; set; }
    [Export] public NightHud? Hud { get; set; }
    [Export] public Light3D? BikeLight { get; set; }
    [Export] public Music Music { get; set; } = Music.Night;

    private int _max;
    private float _warmth;
    private bool _over;

    private static bool HasLight => Game.State.HasItem(LightItem);

    public override void _Ready()
    {
        AudioService.PlayMusic(Music);
        if (HomeDoor != null) HomeDoor.Interacted += () =>
        {
            AudioService.PlaySfx(Sfx.Door);
            End(null);
        };
        _max = Items.MaxWarmth();
        _warmth = _max;
        if (BikeLight != null) BikeLight.Visible = false;
        Hud?.SetWarmth(_warmth, _max);
        Hud?.SetLight(HasLight ? Items.Name(LightItem) : null, false);
        SceneRouter.Instance.WhenIdle(Arrive);
    }

    private static void Arrive()
    {
        if (Game.State.HasFlag(Flags.NightSeen)) return;
        Game.State.SetFlag(Flags.NightSeen);
        Conversation.Start("Night_First", () => Toast.Show("Warmth drains out here"));
    }

    public override void _Process(double delta)
    {
        if (_over || !InputLock.IsFree) return;
        var before = _warmth;
        _warmth = Mathf.Max(0, _warmth - Warmth.DrainPerSecond * (float)delta);
        if (before >= _max * LowWarmth && _warmth < _max * LowWarmth) AudioService.PlaySfx(Sfx.WarmthLow);
        Hud?.SetWarmth(_warmth, _max);
        if (_warmth <= 0)
        {
            Game.State.SetFlag(Flags.WhitedOut);
            AudioService.PlaySfx(Sfx.Whiteout);
            End("Too cold. Everything goes white.");
        }
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
