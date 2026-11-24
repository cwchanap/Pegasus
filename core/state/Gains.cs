using System.Collections.Generic;

namespace Pegasus.Core;

/// <summary>What an activity gives: stat points, money, and bond with one character (by id).</summary>
public record Gains(int Knowledge = 0, int Guts = 0, int Fitness = 0, int Money = 0, string BondWith = "", int Bond = 0)
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
        if (BondWith != "" && Bond != 0) s.AddBond(BondWith, Bond);
    }

    /// <summary>"Money +¥3,000 · Guts +1". A stat already at its cap reads "Guts (max)".
    /// Bond reads "Bond with {name} +1" when a name is given.</summary>
    public string Describe(GameState s, string? bondName = null)
    {
        var parts = new List<string>();
        if (Money != 0) parts.Add($"Money {(Money > 0 ? "+" : "-")}{Format.Yen(System.Math.Abs(Money))}");
        foreach (var (stat, amount) in StatGains())
            parts.Add(amount > 0 && s.GetStat(stat) >= GameState.MaxStat ? $"{stat} (max)" : $"{stat} {amount:+0;-0}");
        if (BondWith != "" && Bond != 0)
        {
            var bond = bondName == null ? "Bond" : $"Bond with {bondName}";
            parts.Add(Bond > 0 && s.GetBond(BondWith) >= GameState.MaxBond ? $"{bond} (max)" : $"{bond} {Bond:+0;-0}");
        }
        return parts.Count == 0 ? "Nothing" : string.Join(" · ", parts);
    }
}
