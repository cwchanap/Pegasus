using Pegasus.Core;
using Xunit;

namespace Pegasus.Tests;

public class GainsTests
{
    [Fact]
    public void AppliesStatsAndMoney()
    {
        var s = new GameState();
        new Gains(Guts: 1, Money: 3000).Apply(s);
        Assert.Equal(1, s.GetStat(Stat.Guts));
        Assert.Equal(3000, s.Money);
    }

    [Fact]
    public void StatsAndBondsStopAtTheCap()
    {
        var s = new GameState();
        s.AddStat(Stat.Fitness, 5);
        s.AddBond("aoi", 9);
        s.AddStat(Stat.Guts, -2);
        Assert.Equal(GameState.MaxStat, s.GetStat(Stat.Fitness));
        Assert.Equal(GameState.MaxBond, s.GetBond("aoi"));
        Assert.Equal(0, s.GetStat(Stat.Guts));
    }

    [Fact]
    public void DescribesMoneyFirstAndMarksMaxedStats()
    {
        var s = new GameState();
        Assert.Equal("Money +¥3,000 · Guts +1", new Gains(Guts: 1, Money: 3000).Describe(s));
        s.AddStat(Stat.Knowledge, 3);
        Assert.Equal("Knowledge (max)", new Gains(Knowledge: 1).Describe(s));
        Assert.Equal("Nothing", new Gains().Describe(s));
    }
}
