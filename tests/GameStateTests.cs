using Pegasus.Core;
using Xunit;

namespace Pegasus.Tests;

public class GameStateTests
{
    [Fact]
    public void MethodsRaiseChanged()
    {
        var s = new GameState();
        var count = 0;
        s.Changed += () => count++;
        s.AddStat(Stat.Fitness, 1);
        s.AddMoney(100);
        s.GiveItem("coat");
        s.SetFlag("met_aoi");
        s.AddBond("aoi", 1);
        Assert.Equal(5, count);
    }

    [Fact]
    public void RoundTripsThroughJson()
    {
        var s = new GameState { Day = 3, Slot = TimeSlot.Evening };
        s.AddStat(Stat.Knowledge, 2);
        s.AddMoney(1200);
        s.AddBond("ren", 2);
        s.GiveItem("flashlight");
        s.SetFlag("tablet_read");

        var json = SaveData.ToJson(s);
        var loaded = SaveData.FromJson(json);

        Assert.Contains("\"Evening\"", json);
        Assert.Equal(3, loaded.Day);
        Assert.Equal(TimeSlot.Evening, loaded.Slot);
        Assert.Equal(2, loaded.GetStat(Stat.Knowledge));
        Assert.Equal(1200, loaded.Money);
        Assert.Equal(2, loaded.GetBond("ren"));
        Assert.True(loaded.HasItem("flashlight"));
        Assert.True(loaded.HasFlag("tablet_read"));
    }
}
