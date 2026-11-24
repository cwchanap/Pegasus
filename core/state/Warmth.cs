using System;
using System.Collections.Generic;
using System.Linq;

namespace Pegasus.Core;

/// <summary>
/// How long he can stay out at night. The meter starts full at Max each night and
/// drains outdoors, only while he can move: thinking time (dialogue, menus, close-ups)
/// is free, walking time costs. Gear he owns and Fitness raise Max. Item warmth comes
/// from item data, so callers pass a lookup from item id to (name, warmth).
/// </summary>
public static class Warmth
{
    public const int Base = 100;
    public const int PerFitness = 10;

    /// <summary>How many seconds of walking Base lasts. The one number to tune night length.</summary>
    public const int BaseSeconds = 90;

    public const float DrainPerSecond = (float)Base / BaseSeconds;

    /// <summary>Seconds of walking a full meter of <paramref name="max"/> lasts.</summary>
    public static float Seconds(int max) => max / DrainPerSecond;

    /// <summary>Below this share of the meter the HUD shows the cold setting in.</summary>
    public const float LowShare = 0.25f;

    public static List<(string Source, int Amount)> Bonuses(GameState s, Func<string, (string Name, int Warmth)> item)
    {
        var bonuses = s.Items.Select(item).Where(i => i.Warmth > 0).Select(i => (i.Name, i.Warmth)).ToList();
        var fitness = s.GetStat(Stat.Fitness) * PerFitness;
        if (fitness > 0) bonuses.Add(("Fitness", fitness));
        return bonuses;
    }

    public static int Max(GameState s, Func<string, (string Name, int Warmth)> item) =>
        Base + Bonuses(s, item).Sum(b => b.Amount);

    /// <summary>"Down coat +40 · Fitness +20", or "" with nothing extra.</summary>
    public static string Describe(GameState s, Func<string, (string Name, int Warmth)> item) =>
        string.Join(" · ", Bonuses(s, item).Select(b => $"{b.Source} +{b.Amount}"));
}
