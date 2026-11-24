using System.Collections.Generic;

namespace Pegasus.Core;

/// <summary>What an activity gives: stat points and money.</summary>
public record Gains(int Knowledge = 0, int Guts = 0, int Fitness = 0, int Money = 0)
{
    private IEnumerable<(Stat Stat, int Amount)> StatGains()
    {
        if (Knowledge != 0) yield return (Stat.Knowledge, Knowledge);
        if (Guts != 0) yield return (Stat.Guts, Guts);
        if (Fitness != 0) yield return (Stat.Fitness, Fitness);
    }

    public void Apply(GameState s)
    {
        foreach (var (stat, amount) in StatGains()) s.AddStat(stat, amount);
        if (Money != 0) s.AddMoney(Money);
    }

    /// <summary>"Money +¥3,000 · Guts +1". A stat already at its cap reads "Guts (max)".</summary>
    public string Describe(GameState s)
    {
        var parts = new List<string>();
        if (Money != 0) parts.Add($"Money {(Money > 0 ? "+" : "-")}{Format.Yen(System.Math.Abs(Money))}");
        foreach (var (stat, amount) in StatGains())
            parts.Add(amount > 0 && s.GetStat(stat) >= GameState.MaxStat ? $"{stat} (max)" : $"{stat} {amount:+0;-0}");
        return parts.Count == 0 ? "Nothing" : string.Join(" · ", parts);
    }
}
