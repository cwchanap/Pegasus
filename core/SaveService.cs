using Godot;

namespace Pegasus.Core;

/// <summary>One save slot at user://save.json. Saved when he goes to bed and when he wakes. Dev scenes pass their own path.</summary>
public static class SaveService
{
    public const string Path = "user://save.json";

    public static bool Exists => FileAccess.FileExists(Path);

    /// <summary>Set by dev runs that start from a --state fixture, so they never overwrite the real save.</summary>
    public static bool Off { get; set; }

    /// <summary>Writes a temp file and renames it over the slot, so a failed write keeps the old save. False if it failed (logged).</summary>
    public static bool Save(GameState state, string path = Path)
    {
        if (Off) return true;
        var temp = path + ".tmp";
        using (var file = FileAccess.Open(temp, FileAccess.ModeFlags.Write))
        {
            if (file == null) return Fail($"Could not open {temp}: {FileAccess.GetOpenError()}");
            if (!file.StoreString(SaveData.ToJson(state))) return Fail($"Could not write {temp}: {file.GetError()}");
        }
        var error = DirAccess.RenameAbsolute(ProjectSettings.GlobalizePath(temp), ProjectSettings.GlobalizePath(path));
        return error == Error.Ok || Fail($"Could not replace {path}: {error}");
    }

    /// <summary>The saved state, or null when there is none or it can't be read (then the title offers only New game).</summary>
    public static GameState? Load(string path = Path)
    {
        if (!FileAccess.FileExists(path)) return null;
        using var file = FileAccess.Open(path, FileAccess.ModeFlags.Read);
        if (file == null) { Fail($"Could not open {path}: {FileAccess.GetOpenError()}"); return null; }
        try { return SaveData.FromJson(file.GetAsText()); }
        catch (System.Text.Json.JsonException e)
        {
            Fail($"Could not read {path}: {e.Message}");
            return null;
        }
    }

    public static void Delete()
    {
        if (Exists) DirAccess.RemoveAbsolute(ProjectSettings.GlobalizePath(Path));
    }

    private static bool Fail(string message)
    {
        GD.PushError(message);
        return false;
    }
}
