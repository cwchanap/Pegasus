using Godot;
using Pegasus.Core;

namespace Pegasus.UI;

/// <summary>
/// The night HUD from the mockup: only the Warmth bar (bottom left) and the held light
/// (bottom right, shown once he owns one). The snow area feeds it. As Warmth runs low,
/// frost creeps in from the screen edges so the white-out never comes unannounced.
/// </summary>
public partial class NightHud : CanvasLayer
{
    /// <summary>The night HUD in the current scene, if any (the close-up shows Warmth from it).</summary>
    public static NightHud? Current { get; private set; }

    /// <summary>How full the Warmth meter is, 0 to 1.</summary>
    public float Share { get; private set; } = 1;

    /// <summary>Below this share of the meter frost starts to creep in; it is thickest at zero.</summary>
    private const float FrostShare = 0.5f;

    public override void _EnterTree() => Current = this;

    public override void _ExitTree()
    {
        if (Current == this) Current = null;
    }

    public void SetWarmth(float current, int max)
    {
        var bar = GetNode<ProgressBar>("%Warmth");
        bar.MaxValue = max;
        bar.Value = current;
        Share = max > 0 ? current / max : 0;
        bar.ThemeTypeVariation = BarStyle(Share);
        GetNode<CanvasItem>("%Frost").Modulate = new Color(1, 1, 1, FrostAlpha(Share));
    }

    /// <summary>0 above <see cref="FrostShare"/>, rising to 1 at an empty meter.</summary>
    public static float FrostAlpha(float share) => Mathf.Clamp(1 - share / FrostShare, 0, 1);

    /// <summary>Show the held light by name, or hide the slot when <paramref name="name"/> is null.</summary>
    public void SetLight(string? name, bool on)
    {
        GetNode<Control>("%Held").Visible = name != null;
        GetNode<Label>("%HeldName").Text = name?.ToUpperInvariant() ?? "";
        GetNode<PanelContainer>("%Slot").ThemeTypeVariation = on ? "HeldSlotOn" : "HeldSlot";
    }

    public static string BarStyle(float share) => share < Warmth.LowShare ? "WarmthBarLow" : "WarmthBar";
}
