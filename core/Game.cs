using System;

namespace Pegasus.Core;

/// <summary>
/// Holds the one live <see cref="GameState"/>. Anything can read <c>Game.State</c>.
/// Nodes that subscribe to <see cref="Changed"/> must unsubscribe in _ExitTree.
/// </summary>
public static class Game
{
    public static GameState State { get; private set; } = new();

    /// <summary>Any change to the state, or the state being replaced by new game / load.</summary>
    public static event Action? Changed;

    static Game() => State.Changed += RaiseChanged;

    public static void NewGame() => Load(new GameState { Money = GameState.StartingMoney });

    /// <summary>Loads the save slot. False when there is no save.</summary>
    public static bool Continue()
    {
        var saved = SaveService.Load();
        if (saved == null) return false;
        Load(saved);
        return true;
    }

    /// <summary>A save failed (the old save stays). The toast tells the player.</summary>
    public static event Action? SaveFailed;

    /// <summary>False if the write failed (logged, and <see cref="SaveFailed"/> raised); the old save stays.</summary>
    public static bool Save()
    {
        if (SaveService.Save(State)) return true;
        SaveFailed?.Invoke();
        return false;
    }

    /// <summary>Replace the live state (new game, save slot, or a dev fixture).</summary>
    public static void Load(GameState state)
    {
        State.Changed -= RaiseChanged;
        State = state;
        State.Changed += RaiseChanged;
        RaiseChanged();
    }

    private static void RaiseChanged() => Changed?.Invoke();
}
