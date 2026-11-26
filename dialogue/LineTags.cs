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

    /// <summary>Exactly one mood name, any case. (Enum.TryParse would also take "1" and "smile,worried".)</summary>
    public static bool TryMood(string? text, out Mood mood)
    {
        foreach (var known in Enum.GetValues<Mood>())
            if (string.Equals(known.ToString(), text, StringComparison.OrdinalIgnoreCase))
            {
                mood = known;
                return true;
            }
        mood = Mood.Neutral;
        return false;
    }

    private static string? Value(string[] tags, string prefix)
    {
        foreach (var tag in tags)
            if (tag.StartsWith(prefix)) return tag[prefix.Length..];
        return null;
    }
}
