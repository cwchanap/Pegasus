using System;
using System.Linq;
using Pegasus.Puzzles;
using Xunit;

namespace Pegasus.Tests;

public class CafeRulesTests
{
    [Fact]
    public void LockOpensOnTheTabletRuleOnly()
    {
        var dials = new GlyphLock();
        Assert.False(dials.Open);
        Set(dials, Glyph.Fire, Glyph.Wake, Glyph.Sleep);
        Assert.True(dials.Open);
    }

    [Fact]
    public void WrongOrderStaysShut()
    {
        var dials = new GlyphLock();
        Set(dials, Glyph.Sleep, Glyph.Wake, Glyph.Fire);
        Assert.False(dials.Open);
    }

    [Fact]
    public void DialsWrapRoundAndGuessingTakesOverAHundredTries()
    {
        var dials = new GlyphLock();
        for (var i = 0; i < GlyphLock.Signs; i++) dials.Turn(0);
        Assert.Equal(Glyph.Gate, dials.Dials[0]);
        Assert.True(GlyphLock.Settings >= 100);
    }

    [Fact]
    public void NewspaperAndCharcoalMakeKindlingEitherWay()
    {
        Assert.Equal("kindling", Recipes.Combine("newspaper", "charcoal"));
        Assert.Equal("kindling", Recipes.Combine("charcoal", "newspaper"));
        Assert.Null(Recipes.Combine("matchbox", "charcoal"));
    }

    private static void Set(GlyphLock dials, params Glyph[] signs)
    {
        for (var d = 0; d < signs.Length; d++)
            for (var turns = 0; dials.Dials[d] != signs[d]; turns++)
            {
                Assert.True(turns < Enum.GetValues<Glyph>().Length, $"dial {d} never shows {signs[d]}");
                dials.Turn(d);
            }
    }
}
