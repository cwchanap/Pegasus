using System;

namespace Pegasus.Core;

/// <summary>
/// The slot clock. Each day has two slots (afternoon, evening); every activity spends
/// one. Bed is open once it is evening. Sleeping starts the night; waking starts the
/// next day's afternoon.
/// </summary>
public static class Clock
{
    public const int LastDay = 5;

    public static bool CanSpendSlot(GameState s) => s.Slot is TimeSlot.Afternoon or TimeSlot.Evening;

    public static bool CanSleep(GameState s) => s.Slot is TimeSlot.Evening or TimeSlot.Late;

    public static void SpendSlot(GameState s)
    {
        if (!CanSpendSlot(s)) throw new InvalidOperationException($"No slot left on day {s.Day} ({s.Slot})");
        s.Mutate(() => s.Slot = s.Slot == TimeSlot.Afternoon ? TimeSlot.Evening : TimeSlot.Late);
    }

    public static void Sleep(GameState s)
    {
        if (!CanSleep(s)) throw new InvalidOperationException($"Too early to sleep ({s.Slot})");
        s.Mutate(() => s.Slot = TimeSlot.Night);
    }

    public static void Wake(GameState s)
    {
        if (s.Slot != TimeSlot.Night) throw new InvalidOperationException($"Not night ({s.Slot})");
        s.Mutate(() => { s.Day++; s.Slot = TimeSlot.Afternoon; });
    }

    /// <summary>True once the last night is over.</summary>
    public static bool IsSliceOver(GameState s) => s.Day > LastDay;

    public static string Describe(GameState s) => s.Slot switch
    {
        TimeSlot.Night => $"Night {s.Day}",
        TimeSlot.Late => $"Day {s.Day} · Late evening",
        _ => $"Day {s.Day} · {s.Slot}",
    };
}
