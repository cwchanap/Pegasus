namespace Pegasus.Core;

/// <summary>
/// Story flags the code itself sets and reads. Flags that only content uses (clues, puzzle
/// steps, bonds' side facts) are named in the Yarn scripts and scenes instead.
/// </summary>
public static class Flags
{
    /// <summary>The opening narration has played (first afternoon in the apartment).</summary>
    public const string IntroSeen = "intro_seen";

    /// <summary>The first-night narration has played (first step into the snow).</summary>
    public const string NightSeen = "night_seen";

    /// <summary>Warmth has run out at least once.</summary>
    public const string WhitedOut = "whited_out";

    /// <summary>The morning-after line about winter gear has played.</summary>
    public const string GearHint = "gear_hint";

    /// <summary>The slice is over: the stove in Kissa Mori is lit. Boot shows the end card.</summary>
    public const string Ended = "ended";
}
