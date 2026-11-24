using Godot;

namespace Pegasus.Core;

/// <summary>One save slot at user://save.json. Saved when he goes to bed and when he wakes. Dev scenes pass their own path.</summary>
public static class SaveService
{
    public const string Path = "user://save.json";

    public static bool Exists => FileAccess.FileExists(Path);

    /// <summary>Writes a temp file and renames it over the slot, so a failed write keeps the old save.</summary>
    public static void Save(GameState state, string path = Path)
    {
        var temp = path + ".tmp";
        using (var file = FileAccess.Open(temp, FileAccess.ModeFlags.Write))
        {
            if (file == null) { GD.PushError($"Could not write {temp}: {FileAccess.GetOpenError()}"); return; }
            file.StoreString(SaveData.ToJson(state));
        }
        var error = DirAccess.RenameAbsolute(ProjectSettings.GlobalizePath(temp), ProjectSettings.GlobalizePath(path));
        if (error != Error.Ok) GD.PushError($"Could not replace {path}: {error}");
    }

    /// <summary>The saved state, or null when there is none or it can't be read (then the title offers only New game).</summary>
    public static GameState? Load(string path = Path)
    {
        if (!FileAccess.FileExists(path)) return null;
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        try { return SaveData.FromJson(file.GetAsText()); }
        catch (System.Text.Json.JsonException e)
        {
            GD.PushError($"Could not read {path}: {e.Message}");
            return null;
        }
    }

    public static void Delete()
    {
        if (Exists) DirAccess.RemoveAbsolute(ProjectSettings.GlobalizePath(Path));
    }
}
