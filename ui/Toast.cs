using System.Collections.Generic;
using Godot;
using Pegasus.Core;

namespace Pegasus.UI;

/// <summary>
/// Autoload. One short line of feedback near the bottom of the screen. Lines wait their turn,
/// none starts while a fade is up, and the one showing waits out a fade, so a toast shown
/// just before a fade is still seen after it.
/// </summary>
public partial class Toast : CanvasLayer
{
    private static Toast? _instance;
    private readonly Queue<string> _waiting = new();
    private Label _label = null!;
    private Tween? _tween;

    public static void Show(string text) => _instance?.Enqueue(text);

    public override void _EnterTree()
    {
        _instance = this;
        Layer = 50;
        ProcessMode = ProcessModeEnum.Always;
        var row = new CenterContainer
        {
            MouseFilter = Control.MouseFilterEnum.Ignore,
            AnchorLeft = 0, AnchorRight = 1, AnchorTop = 1, AnchorBottom = 1,
            OffsetTop = -130, OffsetBottom = -86,
        };
        _label = new Label { ThemeTypeVariation = "ToastLabel", Modulate = Colors.Transparent };
        row.AddChild(_label);
        AddChild(row);
    }

    private void Enqueue(string text)
    {
        GD.Print($"[toast] {text}");
        _waiting.Enqueue(text);
    }

    public override void _Process(double delta)
    {
        var busy = SceneRouter.Instance.Busy;
        // The line on screen holds still behind a fade, so it keeps the rest of its time for after.
        if (_tween?.IsValid() == true)
        {
            if (busy) _tween.Pause(); else _tween.Play();
            return;
        }
        if (busy || !_waiting.TryDequeue(out var text)) return;
        _label.Text = text;
        _tween = CreateTween();
        _tween.TweenProperty(_label, "modulate:a", 1f, 0.15f);
        _tween.TweenInterval(2.5f);
        _tween.TweenProperty(_label, "modulate:a", 0f, 0.6f);
    }
}
