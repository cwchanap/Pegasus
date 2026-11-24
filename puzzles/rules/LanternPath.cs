using System.Linq;

namespace Pegasus.Puzzles;

/// <summary>
/// The shrine lantern rule (pure, tested). The lanterns are lit from the gate inward, one
/// each night, so a lantern only catches when every lantern before it is lit and every one
/// after it is dark. He finds three lit and two dark, out of order, and has to put the
/// wrong ones out first. Snuffing is always allowed and nothing is ever lost.
/// </summary>
public sealed class LanternPath
{
    public const int Count = 5;

    /// <summary>What he finds: the first, third and fourth lit (gate first).</summary>
    private static readonly bool[] Found = { true, false, true, true, false };

    public bool[] Lit { get; }

    public LanternPath(bool solved = false) =>
        Lit = solved ? Enumerable.Repeat(true, Count).ToArray() : (bool[])Found.Clone();

    public bool Solved => Lit.All(l => l);

    public void Snuff(int i) => Lit[i] = false;

    /// <summary>Light lantern <paramref name="i"/> (0 is by the gate). False if it doesn't catch.</summary>
    public bool TryLight(int i)
    {
        if (Lit[i] || Lit.Take(i).Any(l => !l) || Lit.Skip(i + 1).Any(l => l)) return false;
        Lit[i] = true;
        return true;
    }
}
