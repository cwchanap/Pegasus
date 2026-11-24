using Godot;
using Pegasus.Core;

namespace Pegasus.UI;

/// <summary>
/// The night HUD from the mockup: only the Warmth bar (bottom left) and the held light
/// (bottom right, shown once he owns one). The snow area feeds it.
/// </summary>
public partial class NightHud : CanvasLayer
{
    public void SetWarmth(float current, int max)
    {
        var bar = GetNode<ProgressBar>("%Warmth");
        bar.MaxValue = max;
        bar.Value = current;
        bar.ThemeTypeVariation = current < max * Warmth.LowShare ? "WarmthBarLow" : "WarmthBar";
    }

    /// <summary>Show the held light by name, or hide the slot when <paramref name="name"/> is null.</summary>
    public void SetLight(string? name, bool on)
    {
        GetNode<Control>("%Held").Visible = name != null;
        GetNode<Label>("%HeldName").Text = name?.ToUpperInvariant() ?? "";
        GetNode<PanelContainer>("%Slot").ThemeTypeVariation = on ? "HeldSlotOn" : "HeldSlot";
    }
}
