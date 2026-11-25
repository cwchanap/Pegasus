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
            slider.ValueChanged += value => Settings.SetVolume(bus, (float)value);
            // A slider's own focus mark is faint; light up its row's name instead.
            slider.FocusEntered += () => label.ThemeTypeVariation = "BodyTextOn";
            slider.FocusExited += () => label.ThemeTypeVariation = "BodyText";
        }
        var fullscreen = GetNode<Button>("%Fullscreen");
        fullscreen.Toggled += on =>
        {
            Settings.SetFullscreen(on);
            ShowFullscreen(fullscreen);
        };
        Refresh();
    }

    // The menu's panel lives in an autoload, so it reads the settings again each time it shows
    // (the title screen's panel may have changed them since).
    public override void _Notification(int what)
    {
        if (what == NotificationVisibilityChanged && IsNodeReady() && IsVisibleInTree()) Refresh();
    }

    private void Refresh()
    {
        foreach (var bus in Settings.Buses)
            GetNode<HSlider>($"{bus}/Slider").SetValueNoSignal(Settings.Volume(bus));
        var fullscreen = GetNode<Button>("%Fullscreen");
        fullscreen.SetPressedNoSignal(Settings.Fullscreen);
        ShowFullscreen(fullscreen);
    }

    private static void ShowFullscreen(Button button) => button.Text = $"Fullscreen: {(button.ButtonPressed ? "On" : "Off")}";
}
