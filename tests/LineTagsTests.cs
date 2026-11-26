using Pegasus.Dialogue;
using Xunit;

namespace Pegasus.Tests;

public class LineTagsTests
{
    [Theory]
    [InlineData(new[] { "mood:smile" }, Mood.Smile)]
    [InlineData(new[] { "line:a1b2", "mood:worried" }, Mood.Worried)]
    [InlineData(new[] { "mood:Smile" }, Mood.Smile)]
    [InlineData(new string[0], Mood.Neutral)]
    [InlineData(new[] { "line:a1b2" }, Mood.Neutral)]
    [InlineData(new[] { "mood:grumpy" }, Mood.Neutral)]
    [InlineData(new[] { "mood:1" }, Mood.Neutral)]
    public void MoodComesFromTheTagAndDefaultsToNeutral(string[] tags, Mood expected) =>
        Assert.Equal(expected, LineTags.GetMood(tags));

    [Fact]
    public void NeedsReadsTheConditionAndIgnoresOtherTags()
    {
        Assert.Equal("guts>=2|item:flashlight", LineTags.Needs(new[] { "mood:smile", "needs:guts>=2|item:flashlight" }));
        Assert.Null(LineTags.Needs(new[] { "mood:smile" }));
    }
}
