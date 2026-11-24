using Godot;
using Pegasus.Core;

namespace Pegasus.UI;

/// <summary>
/// The night HUD from the mockup: only the Warmth bar (bottom left) and the held light
/// (bottom right, shown once he owns one). The snow area feeds it.
/// </summary>
public partial class NightHud : CanvasLayer
{
    /// <summary>The night HUD in the current scene, if any (the close-up shows Warmth from it).</summary>
    public static NightHud? Current { get; private set; }

    /// <summary>How full the Warmth meter is, 0 to 1.</summary>
    public float Share { get; private set; } = 1;

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
    }

    /// <summary>Show the held light by name, or hide the slot when <paramref name="name"/> is null.</summary>
    public void SetLight(string? name, bool on)
    {
        GetNode<Control>("%Held").Visible = name != null;
        GetNode<Label>("%HeldName").Text = name?.ToUpperInvariant() ?? "";
        GetNode<PanelContainer>("%Slot").ThemeTypeVariation = on ? "HeldSlotOn" : "HeldSlot";
    }

    public static string BarStyle(float share) => share < Warmth.LowShare ? "WarmthBarLow" : "WarmthBar";
}
