using Pegasus.Core;
using Xunit;

namespace Pegasus.Tests;

public class WardrobeTests
{
    [Theory]
    [InlineData(TimeSlot.Afternoon, false, Outfit.Summer)]
    [InlineData(TimeSlot.Afternoon, true, Outfit.Summer)]
    [InlineData(TimeSlot.Late, true, Outfit.Summer)]
    [InlineData(TimeSlot.Night, false, Outfit.Summer)]
    [InlineData(TimeSlot.Night, true, Outfit.Coat)]
    public void TheCoatIsForNightsOnceHeOwnsIt(TimeSlot slot, bool ownsCoat, Outfit expected)
    {
        var s = new GameState { Slot = slot };
        if (ownsCoat) s.GiveItem(Wardrobe.CoatItem);
        Assert.Equal(expected, Wardrobe.Wears(s));
    }
}
