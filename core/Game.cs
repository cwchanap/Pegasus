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

    public static void NewGame() => Load(new GameState());

    /// <summary>Loads the save slot. False when there is no save.</summary>
    public static bool Continue()
    {
        var saved = SaveService.Load();
        if (saved == null) return false;
        Load(saved);
        return true;
    }

    public static void Save() => SaveService.Save(State);

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
