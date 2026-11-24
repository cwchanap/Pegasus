using Godot;

namespace Pegasus.UI;

/// <summary>Autoload. One short line of feedback near the bottom of the screen.</summary>
public partial class Toast : CanvasLayer
{
    private static Toast? _instance;
    private Label _label = null!;
    private Tween? _tween;

    public static void Show(string text) => _instance?.Display(text);

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

    private void Display(string text)
    {
        GD.Print($"[toast] {text}");
        _label.Text = text;
        _tween?.Kill();
        _tween = CreateTween();
        _tween.TweenProperty(_label, "modulate:a", 1f, 0.15f);
        _tween.TweenInterval(2.5f);
        _tween.TweenProperty(_label, "modulate:a", 0f, 0.6f);
    }
}
