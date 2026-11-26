using System;

namespace Pegasus.Dialogue;

/// <summary>The portrait a line shows: <c>#mood:smile</c>, <c>#mood:worried</c>, or neutral when untagged.</summary>
public enum Mood { Neutral, Smile, Worried }

/// <summary>
/// Reads the hashtags Yarn keeps on lines and choices. Pure C# (no Godot types), so
/// tests/ compiles it directly.
/// </summary>
public static class LineTags
{
    public const string NeedsTag = "needs:";
    public const string MoodTag = "mood:";

    /// <summary>The condition text of a "#needs:..." tag on a choice, or null.</summary>
    public static string? Needs(string[] tags) => Value(tags, NeedsTag);

    /// <summary>The mood of a "#mood:..." tag on a line; neutral when there is none or it is unknown.</summary>
    public static Mood GetMood(string[] tags) => TryMood(Value(tags, MoodTag), out var mood) ? mood : Mood.Neutral;

    public static bool TryMood(string? text, out Mood mood)
    {
        mood = Mood.Neutral;
        return text != null && !int.TryParse(text, out _) && Enum.TryParse(text, ignoreCase: true, out mood);
    }

    private static string? Value(string[] tags, string prefix)
    {
        foreach (var tag in tags)
            if (tag.StartsWith(prefix)) return tag[prefix.Length..];
        return null;
    }
}
