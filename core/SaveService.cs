using Godot;

namespace Pegasus.Core;

/// <summary>One save slot at user://save.json. Saved when he goes to bed and when he wakes.</summary>
public static class SaveService
{
    public const string Path = "user://save.json";

    public static bool Exists => FileAccess.FileExists(Path);

    public static void Save(GameState state)
    {
        using var file = FileAccess.Open(Path, FileAccess.ModeFlags.Write);
        file.StoreString(SaveData.ToJson(state));
    }

    public static GameState? Load()
    {
        if (!Exists) return null;
        using var file = FileAccess.Open(Path, FileAccess.ModeFlags.Read);
        return SaveData.FromJson(file.GetAsText());
    }

    public static void Delete()
    {
        if (Exists) DirAccess.RemoveAbsolute(ProjectSettings.GlobalizePath(Path));
    }
}
