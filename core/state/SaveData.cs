using System.Text.Json;
using System.Text.Json.Serialization;

namespace Pegasus.Core;

/// <summary>GameState to and from JSON. One format, no versioning or migration.</summary>
public static class SaveData
{
    private static readonly JsonSerializerOptions Options = new()
    {
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() },
    };

    public static string ToJson(GameState state) => JsonSerializer.Serialize(state, Options);

    public static GameState FromJson(string json) =>
        JsonSerializer.Deserialize<GameState>(json, Options) ?? new GameState();
}
