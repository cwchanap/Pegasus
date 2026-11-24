namespace Pegasus.Core;

/// <summary>
/// The day/night loop: go to bed, wake at 3 AM in the snow world, return to bed,
/// wake next morning. Saves at bedtime and on waking. Both saves happen at the start
/// of a night or a day, so the state never needs to record a current location.
/// </summary>
public static class DayCycle
{
    public static string SceneFor(GameState s) => s.Slot == TimeSlot.Night ? Scenes.Night : Scenes.Apartment;

    /// <summary>Enter the world wherever the current state says (new game or loaded save).</summary>
    public static void Enter() => SceneRouter.Instance.GoTo(SceneFor(Game.State), Caption(Game.State));

    public static void GoToBed()
    {
        if (SceneRouter.Instance.Busy) return;
        Clock.Sleep(Game.State);
        Game.Save();
        SceneRouter.Instance.GoTo(Scenes.Night, Caption(Game.State));
    }

    public static void WakeUp()
    {
        if (SceneRouter.Instance.Busy) return;
        Clock.Wake(Game.State);
        Game.Save();
        SceneRouter.Instance.GoTo(Scenes.Apartment, Caption(Game.State));
    }

    private static string Caption(GameState s) =>
        s.Slot == TimeSlot.Night ? "3:00 AM" : $"Day {s.Day} · Morning";
}
