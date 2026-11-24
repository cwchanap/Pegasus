using Godot;

namespace Pegasus.UI;

/// <summary>Small helpers for code-built UI until the A6 UI kit brings a real theme.</summary>
public static class Ui
{
    public static readonly Color Navy = new("1c1f3b");
    public static readonly Color Cream = new("fbf1e3");
    public static readonly Color Orange = new("ff5a3c");
    public static readonly Color Yellow = new("ffd24d");
    public static readonly Color Teal = new("1fa39a");

    public static StyleBoxFlat Box(Color bg, int radius, Color? border = null, int borderWidth = 3,
        int padH = 0, int padV = 2, (int tl, int tr, int br, int bl)? corners = null)
    {
        var box = new StyleBoxFlat { BgColor = bg };
        if (corners is var (tl, tr, br, bl))
        {
            box.CornerRadiusTopLeft = tl; box.CornerRadiusTopRight = tr;
            box.CornerRadiusBottomRight = br; box.CornerRadiusBottomLeft = bl;
        }
        else box.SetCornerRadiusAll(radius);
        if (border is { } b) { box.BorderColor = b; box.SetBorderWidthAll(borderWidth); }
        box.ContentMarginLeft = box.ContentMarginRight = padH;
        box.ContentMarginTop = box.ContentMarginBottom = padV;
        return box;
    }

    public static Label Text(string text, int size, Color color)
    {
        var label = new Label { Text = text, MouseFilter = Control.MouseFilterEnum.Ignore };
        label.AddThemeFontSizeOverride("font_size", size);
        label.AddThemeColorOverride("font_color", color);
        return label;
    }

    public static PanelContainer Tag(string text, Color bg, Color fg, int size = 14)
    {
        var panel = new PanelContainer { MouseFilter = Control.MouseFilterEnum.Ignore, SizeFlagsVertical = Control.SizeFlags.ShrinkCenter };
        panel.AddThemeStyleboxOverride("panel", Box(bg, 6, padH: 8));
        panel.AddChild(Text(text, size, fg));
        return panel;
    }
}
