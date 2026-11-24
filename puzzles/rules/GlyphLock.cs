using System;
using System.Linq;

namespace Pegasus.Puzzles;

/// <summary>The five signs carved on the shrine tablet, in its order.</summary>
public enum Glyph { Gate, Cold, Sleep, Fire, Wake }

/// <summary>
/// The lock on the coffee shop stove (pure, tested): three dials of the tablet's signs.
/// It opens on the tablet's rule, "only a fire wakes the sleeper": FIRE, WAKE, SLEEP.
/// With 125 settings, guessing takes longer than walking back to read the tablet.
/// </summary>
public sealed class GlyphLock
{
    public const int DialCount = 3;
    public static readonly int Signs = Enum.GetValues<Glyph>().Length;
    public static readonly Glyph[] Answer = { Glyph.Fire, Glyph.Wake, Glyph.Sleep };

    public Glyph[] Dials { get; } = new Glyph[DialCount];

    public static int Settings => (int)Math.Pow(Signs, DialCount);

    /// <summary>Turn one dial to the next sign, wrapping round.</summary>
    public void Turn(int dial) => Dials[dial] = (Glyph)(((int)Dials[dial] + 1) % Signs);

    public bool Open => Dials.SequenceEqual(Answer);
}
