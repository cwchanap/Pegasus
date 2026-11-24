using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

namespace Pegasus.Core;

/// <summary>
/// Autoload. Changes scenes behind a fade to black, with an optional caption on the
/// black screen ("3:00 AM"). The game is paused while the screen is dark.
/// Interlude does the same without changing scene, e.g. to pass time on an activity.
/// </summary>
public partial class SceneRouter : CanvasLayer
{
    public static SceneRouter Instance { get; private set; } = null!;

    private const float FadeSeconds = 0.4f;
    private const float CaptionSeconds = 1.4f;

    private ColorRect _fade = null!;
    private Label _caption = null!;

    public bool Busy { get; private set; }

    private readonly List<Action> _whenIdle = new();

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

    public void GoTo(string scenePath, string? caption = null) => Run(caption, async () =>
    {
        GetTree().ChangeSceneToFile(scenePath);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
        await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
    });

    /// <summary>Fade out, show the caption, run <paramref name="whileDark"/>, fade back in. Same scene.</summary>
    public void Interlude(string? caption, Action whileDark) => Run(caption, () =>
    {
        whileDark();
        return Task.CompletedTask;
    });

    /// <summary>Run <paramref name="action"/> once no fade is up: now, or as soon as the current fade-in ends.
    /// A scene's _Ready runs in the dark, so this is how it says something when the player can see it.</summary>
    public void WhenIdle(Action action)
    {
        if (Busy) _whenIdle.Add(action);
        else action();
    }

    private async void Run(string? caption, Func<Task> whileDark)
    {
        if (Busy) return;
        Busy = true;
        InputLock.Acquire(nameof(SceneRouter));
        _fade.MouseFilter = Control.MouseFilterEnum.Stop;
        GetTree().Paused = true;
        try
        {
            await Tween(_fade, 1f, FadeSeconds);
            if (!string.IsNullOrEmpty(caption))
            {
                _caption.Text = caption;
                await Tween(_caption, 1f, FadeSeconds);
                await ToSignal(GetTree().CreateTimer(CaptionSeconds, processAlways: true), SceneTreeTimer.SignalName.Timeout);
            }
            await whileDark();
            if (!string.IsNullOrEmpty(caption)) await Tween(_caption, 0f, FadeSeconds);
        }
        catch (Exception e)
        {
            GD.PushError($"SceneRouter: {e}");
        }
        finally
        {
            // Always come back from black, even if the work in the dark failed.
            _caption.Modulate = Colors.Transparent;
            GetTree().Paused = false;
            await Tween(_fade, 0f, FadeSeconds);
            _fade.MouseFilter = Control.MouseFilterEnum.Ignore;
            InputLock.Release(nameof(SceneRouter));
            Busy = false;
            var due = _whenIdle.ToArray();
            _whenIdle.Clear();
            foreach (var action in due) action();
        }
    }

    private async Task Tween(CanvasItem item, float alpha, float seconds)
    {
        var tween = CreateTween();
        tween.TweenProperty(item, "modulate:a", alpha, seconds);
        await ToSignal(tween, Godot.Tween.SignalName.Finished);
    }
}
