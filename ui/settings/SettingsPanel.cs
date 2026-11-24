using Godot;
using Pegasus.Core;

namespace Pegasus.UI;

/// <summary>Volumes and fullscreen, shared by the title screen and the menu's System page.
/// Each row is named for its audio bus. Changes apply and save at once (core/Settings).</summary>
public partial class SettingsPanel : VBoxContainer
{
    public Control First => GetNode<Control>($"{Settings.Buses[0]}/Slider");

    public override void _Ready()
    {
        foreach (var bus in Settings.Buses)
        {
            var slider = GetNode<HSlider>($"{bus}/Slider");
            var label = GetNode<Label>($"{bus}/Label");
            slider.SetValueNoSignal(Settings.Volume(bus));
            slider.ValueChanged += value => Settings.SetVolume(bus, (float)value);
            // A slider's own focus mark is faint; light up its row's name instead.
            slider.FocusEntered += () => label.ThemeTypeVariation = "BodyTextOn";
            slider.FocusExited += () => label.ThemeTypeVariation = "BodyText";
        }
        var fullscreen = GetNode<Button>("%Fullscreen");
        fullscreen.SetPressedNoSignal(Settings.Fullscreen);
        ShowFullscreen(fullscreen);
        fullscreen.Toggled += on =>
        {
            Settings.SetFullscreen(on);
            ShowFullscreen(fullscreen);
        };
    }

    private static void ShowFullscreen(Button button) => button.Text = $"Fullscreen: {(button.ButtonPressed ? "On" : "Off")}";
}
