using System.Collections.Generic;

namespace Pegasus.Core;

/// <summary>
/// Who currently owns the player's input. Dialogue, menus, puzzles and scene fades each
/// take a lock under their own name; the player only moves and interacts when none is held.
/// Several owners can hold it at once, so closing one never frees the player under another.
/// </summary>
public static class InputLock
{
    private static readonly HashSet<string> Owners = new();

    public static bool IsFree => Owners.Count == 0;

    public static void Acquire(string owner) => Owners.Add(owner);

    public static void Release(string owner) => Owners.Remove(owner);
}
