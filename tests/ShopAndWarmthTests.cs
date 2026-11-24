using Pegasus.Core;
using Xunit;

namespace Pegasus.Tests;

public class ShopAndWarmthTests
{
    private static (string, int) Lookup(string id) => id switch
    {
        "down_coat" => ("Down coat", 300),
        "boots" => ("Boots", 150),
        _ => (id, 0),
    };

    [Fact]
    public void BuyingTakesMoneyOnceAndNeedsEnough()
    {
        var s = new GameState { Money = 10000 };
        Assert.True(s.TryBuy("down_coat", 9800));
        Assert.Equal(200, s.Money);
        Assert.True(s.HasItem("down_coat"));
        Assert.False(s.TryBuy("down_coat", 0));
        Assert.False(s.TryBuy("boots", 6500));
        Assert.Equal(200, s.Money);
    }

    [Fact]
    public void WarmthAddsGearAndFitness()
    {
        var s = new GameState();
        Assert.Equal(Warmth.Base, Warmth.Max(s, Lookup));
        Assert.Equal("", Warmth.Describe(s, Lookup));
        s.GiveItem("down_coat");
        s.GiveItem("flashlight");
        s.AddStat(Stat.Fitness, 2);
        Assert.Equal(Warmth.Base + 300 + 100, Warmth.Max(s, Lookup));
        Assert.Equal("Down coat +300 · Fitness +100", Warmth.Describe(s, Lookup));
    }

    [Fact]
    public void NightLengthGrowsWithGearAndFitness()
    {
        var s = new GameState();
        Assert.Equal(15f, Warmth.Seconds(Warmth.Max(s, Lookup)), 3); // a tease: short of the shrine (about 24 s away)
        s.GiveItem("down_coat");
        Assert.Equal(60f, Warmth.Seconds(Warmth.Max(s, Lookup)), 3); // the shrine and some wandering
        s.GiveItem("boots");
        s.AddStat(Stat.Fitness, GameState.MaxStat);
        Assert.Equal(105f, Warmth.Seconds(Warmth.Max(s, Lookup)), 3); // shrine, underpass and arcade in one night
    }
}
