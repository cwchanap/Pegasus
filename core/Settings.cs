using Godot;

namespace Pegasus.Core;

/// <summary>
/// Player settings: volumes and fullscreen. Kept in user://settings.cfg, apart from the save,
/// so they survive a new game. Loaded and applied at boot (by <see cref="AudioService"/>);
/// every change applies at once; <see cref="Save"/> writes them (the settings panel calls it
/// when a drag ends or the panel closes, not on every slider tick).
/// </summary>
public static class Settings
{
    public const string Path = "user://settings.cfg";
    public static readonly string[] Buses = { "Master", "Music", "SFX" };

    private static readonly ConfigFile File = new();

    /// <summary>Volume of a bus, 0 to 1.</summary>
    public static float Volume(string bus) => (float)File.GetValue("volume", bus, 0.8f);

    public static bool Fullscreen => (bool)File.GetValue("display", "fullscreen", false);

    public static void SetVolume(string bus, float value)
    {
        File.SetValue("volume", bus, Mathf.Clamp(value, 0f, 1f));
        Apply();
    }

    public static void SetFullscreen(bool on)
    {
        File.SetValue("display", "fullscreen", on);
        Apply();
        Save();
    }

    public static void Save() => File.Save(Path);

    public static void Load()
    {
        if (FileAccess.FileExists(Path)) File.Load(Path); // no file yet: the defaults
        Apply();
    }

    private static void Apply()
    {
        foreach (var bus in Buses)
        {
            var index = AudioServer.GetBusIndex(bus);
            var volume = Volume(bus);
            AudioServer.SetBusVolumeDb(index, Mathf.LinearToDb(volume));
            AudioServer.SetBusMute(index, volume <= 0.001f);
        }
        var mode = DisplayServer.WindowGetMode();
        var isFullscreen = mode is DisplayServer.WindowMode.Fullscreen or DisplayServer.WindowMode.ExclusiveFullscreen;
        if (Fullscreen != isFullscreen && DisplayServer.GetName() != "headless")
            DisplayServer.WindowSetMode(Fullscreen ? DisplayServer.WindowMode.Fullscreen : DisplayServer.WindowMode.Windowed);
    }
}
