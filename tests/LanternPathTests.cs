using Pegasus.Puzzles;
using Xunit;

namespace Pegasus.Tests;

public class LanternPathTests
{
    [Fact]
    public void StartsThreeLitTwoDarkAndUnsolved()
    {
        var path = new LanternPath();
        Assert.Equal(new[] { true, false, true, true, false }, path.Lit);
        Assert.False(path.Solved);
    }

    [Fact]
    public void WrongOrderDoesNotCatch()
    {
        var path = new LanternPath();
        Assert.False(path.TryLight(1)); // later lanterns are still lit
        Assert.False(path.TryLight(4)); // an earlier lantern is dark
        Assert.Equal(new[] { true, false, true, true, false }, path.Lit);
    }

    [Fact]
    public void PuttingOutTheWrongOnesThenLightingFromTheGateSolvesIt()
    {
        var path = new LanternPath();
        path.Snuff(3);
        path.Snuff(2);
        for (var i = 1; i < LanternPath.Count; i++) Assert.True(path.TryLight(i));
        Assert.True(path.Solved);
    }

    [Fact]
    public void ASolvedPathStartsAllLit() => Assert.True(new LanternPath(solved: true).Solved);
}
