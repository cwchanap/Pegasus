using Pegasus.Conditions;
using Pegasus.Core;
using Xunit;

namespace Pegasus.Tests;

public class ConditionTests
{
    private static GameState State()
    {
        var s = new GameState();
        s.AddStat(Stat.Guts, 2);
        s.AddMoney(500);
        s.AddBond("aoi", 1);
        s.GiveItem("coat");
        s.SetFlag("shrine_solved");
        return s;
    }

    [Theory]
    [InlineData("", true)]
    [InlineData("guts >= 2", true)]
    [InlineData("Guts>=3", false)]
    [InlineData("knowledge >= 1", false)]
    [InlineData("money >= 500", true)]
    [InlineData("money >= 501", false)]
    [InlineData("bond:aoi >= 1", true)]
    [InlineData("bond:ren >= 1", false)]
    [InlineData("item:coat", true)]
    [InlineData("item:flashlight", false)]
    [InlineData("flag:shrine_solved", true)]
    [InlineData("!flag:shrine_solved", false)]
    [InlineData("guts >= 3 | item:coat", true)]
    [InlineData("guts >= 2 & item:flashlight", false)]
    [InlineData("knowledge >= 2 | bond:aoi >= 1 & item:coat", true)]
    [InlineData("(knowledge >= 2 | bond:aoi >= 1) & item:flashlight", false)]
    [InlineData("!(item:flashlight | flag:nope)", true)]
    public void Evaluates(string text, bool expected) =>
        Assert.Equal(expected, Condition.Check(text, State()));

    [Theory]
    [InlineData("guts")]
    [InlineData("guts >= x")]
    [InlineData("charm >= 1")]
    [InlineData("item:coat |")]
    [InlineData("(item:coat")]
    [InlineData("item:coat item:hat")]
    [InlineData("item")]
    [InlineData("flag:")]
    [InlineData("bond: >= 1")]
    [InlineData("money:foo >= 3")]
    [InlineData("guts:x >= 1")]
    public void RejectsBadSyntax(string text) =>
        Assert.Throws<ConditionSyntaxException>(() => Condition.Parse(text));

    [Theory]
    [InlineData("guts >= 2 | item:flashlight", "Guts 2")]
    [InlineData("knowledge >= 1 & money >= 300", "Knowledge 1 and ¥300")]
    [InlineData("flag:secret & bond:aoi >= 2", "")]
    public void DescribesOnlyStatsAndMoney(string text, string expected) =>
        Assert.Equal(expected, Condition.Parse(text).Describe());
}
