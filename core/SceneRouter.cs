using System.Threading.Tasks;
using Godot;

namespace Pegasus.Core;

/// <summary>
/// Autoload. Changes scenes behind a fade to black, with an optional caption on the
/// black screen ("3:00 AM"). The game is paused while the screen is dark.
/// </summary>
public partial class SceneRouter : CanvasLayer
{
    public static SceneRouter Instance { get; private set; } = null!;

    private const float FadeSeconds = 0.4f;
    private const float CaptionSeconds = 1.4f;

    private ColorRect _fade = null!;
    private Label _caption = null!;

    public bool Busy { get; private set; }

    public override void _EnterTree()
    {
        Instance = this;
        Layer = 100;
        ProcessMode = ProcessModeEnum.Always;

        _fade = new ColorRect { Color = Colors.Black, MouseFilter = Control.MouseFilterEnum.Ignore, Modulate = Colors.Transparent };
        _fade.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        AddChild(_fade);

        _caption = new Label
        {
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Modulate = Colors.Transparent,
        };
        _caption.AddThemeFontSizeOverride("font_size", 34);
        _caption.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        AddChild(_caption);
    }

    public async void GoTo(string scenePath, string? caption = null)
    {
        if (Busy) return;
        Busy = true;
        InputLock.Acquire(nameof(SceneRouter));
        _fade.MouseFilter = Control.MouseFilterEnum.Stop;
        GetTree().Paused = true;

        await Tween(_fade, 1f, FadeSeconds);
        if (caption != null)
        {
            _caption.Text = caption;
            await Tween(_caption, 1f, FadeSeconds);
            await ToSignal(GetTree().CreateTimer(CaptionSeconds, processAlways: true), SceneTreeTimer.SignalName.Timeout);
        }

        GetTree().ChangeSceneToFile(scenePath);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);

        if (caption != null) await Tween(_caption, 0f, FadeSeconds);
        GetTree().Paused = false;
        await Tween(_fade, 0f, FadeSeconds);
        _fade.MouseFilter = Control.MouseFilterEnum.Ignore;
        InputLock.Release(nameof(SceneRouter));
        Busy = false;
    }

    private async Task Tween(CanvasItem item, float alpha, float seconds)
    {
        var tween = CreateTween();
        tween.TweenProperty(item, "modulate:a", alpha, seconds);
        await ToSignal(tween, Godot.Tween.SignalName.Finished);
    }
}
