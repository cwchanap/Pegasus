namespace Pegasus.Core;

/// <summary>
/// The day/night loop: go to bed, wake at 3 AM in the snow world, return to bed,
/// wake next morning. Saves at bedtime and on waking. Both saves happen at the start
/// of a night or a day, so the state never needs to record a current location.
/// Lighting the stove in Kissa Mori ends the slice (<see cref="Finish"/>); after that the
/// game only shows the end card.
/// </summary>
public static class DayCycle
{
    public static string SceneFor(GameState s) =>
        s.HasFlag(Flags.Ended) ? Scenes.EndCard : s.Slot == TimeSlot.Night ? Scenes.Night : Scenes.Apartment;

    /// <summary>Enter the world wherever the current state says (new game or loaded save).</summary>
    public static void Enter() => SceneRouter.Instance.GoTo(SceneFor(Game.State), Caption(Game.State));

    public static void GoToBed()
    {
        if (SceneRouter.Instance.Busy) return;
        Clock.Sleep(Game.State);
        Game.Save();
        SceneRouter.Instance.GoTo(Scenes.Night, Caption(Game.State));
    }

    /// <summary>End the night: walking home, or Warmth running out (<paramref name="why"/> is shown first).</summary>
    public static void WakeUp(string? why = null)
    {
        if (SceneRouter.Instance.Busy) return;
        Clock.Wake(Game.State);
        Game.Save();
        var caption = Caption(Game.State);
        SceneRouter.Instance.GoTo(Scenes.Apartment, why == null ? caption : $"{why}\n\n{caption}");
    }

    /// <summary>The end of the slice: no next day. Saved first, so Continue goes straight to the end card.
    /// <paramref name="caption"/> is the last line, shown on the black between.</summary>
    public static void Finish(string caption)
    {
        if (SceneRouter.Instance.Busy) return;
        Game.State.SetFlag(Flags.Ended);
        Game.Save();
        SceneRouter.Instance.GoTo(Scenes.EndCard, caption);
    }

    private static string? Caption(GameState s) =>
        s.HasFlag(Flags.Ended) ? null : s.Slot == TimeSlot.Night ? "3:00 AM" : $"Day {s.Day} · Morning";
}
