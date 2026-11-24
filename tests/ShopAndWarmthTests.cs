using Pegasus.Core;
using Xunit;

namespace Pegasus.Tests;

public class ShopAndWarmthTests
{
    private static (string, int) Lookup(string id) => id switch
    {
        "down_coat" => ("Down coat", 40),
        "boots" => ("Boots", 20),
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
        Assert.Equal(Warmth.Base + 40 + 20, Warmth.Max(s, Lookup));
        Assert.Equal("Down coat +40 · Fitness +20", Warmth.Describe(s, Lookup));
    }
}
