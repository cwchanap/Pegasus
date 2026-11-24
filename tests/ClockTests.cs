using System;
using Pegasus.Core;
using Xunit;

namespace Pegasus.Tests;

public class ClockTests
{
    [Fact]
    public void FullDayRunsAfternoonEveningLateNightNextAfternoon()
    {
        var s = new GameState();
        Assert.False(Clock.CanSleep(s));
        Clock.SpendSlot(s);
        Assert.Equal(TimeSlot.Evening, s.Slot);
        Assert.True(Clock.CanSleep(s));
        Clock.SpendSlot(s);
        Assert.Equal(TimeSlot.Late, s.Slot);
        Assert.False(Clock.CanSpendSlot(s));
        Clock.Sleep(s);
        Assert.Equal(TimeSlot.Night, s.Slot);
        Clock.Wake(s);
        Assert.Equal(2, s.Day);
        Assert.Equal(TimeSlot.Afternoon, s.Slot);
    }

    [Fact]
    public void CanSkipTheEveningByGoingToBed()
    {
        var s = new GameState { Slot = TimeSlot.Evening };
        Clock.Sleep(s);
        Assert.Equal(TimeSlot.Night, s.Slot);
    }

    [Fact]
    public void RejectsOutOfOrderSteps()
    {
        Assert.Throws<InvalidOperationException>(() => Clock.Sleep(new GameState()));
        Assert.Throws<InvalidOperationException>(() => Clock.Wake(new GameState()));
        Assert.Throws<InvalidOperationException>(() => Clock.SpendSlot(new GameState { Slot = TimeSlot.Late }));
    }

    [Fact]
    public void SliceIsOverAfterTheLastNight()
    {
        var s = new GameState { Day = Clock.LastDay, Slot = TimeSlot.Night };
        Assert.False(Clock.IsSliceOver(s));
        Clock.Wake(s);
        Assert.True(Clock.IsSliceOver(s));
    }
}
