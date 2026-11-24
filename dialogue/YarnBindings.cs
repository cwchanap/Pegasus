using System;
using Pegasus.Conditions;
using Pegasus.Core;
using Pegasus.World;
using YarnSpinnerGodot;

namespace Pegasus.Dialogue;

/// <summary>
/// The only way Yarn scripts touch the game. GameState stays the single source of
/// truth: scripts read it with met("...") and change it with commands. Do not keep game
/// facts in Yarn $variables; they are not saved.
///
///   &lt;&lt;if met("guts >= 2 | item:flashlight")&gt;&gt;   branch on any condition
///   &lt;&lt;stat guts 1&gt;&gt;  &lt;&lt;money 500&gt;&gt;  &lt;&lt;money -300&gt;&gt;
///   &lt;&lt;give coat&gt;&gt;  &lt;&lt;take coat&gt;&gt;  &lt;&lt;flag name&gt;&gt; (clue_* writes the notebook)  &lt;&lt;unflag name&gt;&gt;  &lt;&lt;bond aoi 1&gt;&gt;
///   &lt;&lt;wake_up "You fell asleep in the cold."&gt;&gt;   end the night (last line of a node)
///   &lt;&lt;end_game "You wake to cicadas."&gt;&gt;         end the slice and go to the end card (last line)
///
/// A choice that needs something is tagged, not wrapped in &lt;&lt;if&gt;&gt;, so it stays visible
/// but locked with its requirement: <c>-&gt; Tell her about the snow #needs:guts&gt;=3</c>.
/// &lt;&lt;if&gt;&gt; on a choice hides it entirely.
/// </summary>
public static class YarnBindings
{
    public const string NeedsTag = "needs:";
    public const string CluePrefix = "clue_";

    /// <summary>Raised when &lt;&lt;flag clue_*&gt;&gt; sets a clue flag for the first time.</summary>
    public static event Action<string>? ClueNoted;

    public static void Register(DialogueRunner runner)
    {
        runner.AddFunction("met", (Func<string, bool>)(text => Condition.Check(text, Game.State)));
        runner.AddCommandHandler("stat", (Action<string, int>)((stat, amount) => Game.State.AddStat(Enum.Parse<Stat>(stat, ignoreCase: true), amount)));
        runner.AddCommandHandler("money", (Action<int>)(amount => Game.State.AddMoney(amount)));
        runner.AddCommandHandler("give", (Action<string>)(item => { Items.Get(item); /* warns if the id has no item data */ Game.State.GiveItem(item); AudioService.PlaySfx(Sfx.ItemGet); }));
        runner.AddCommandHandler("take", (Action<string>)(item => Game.State.TakeItem(item)));
        runner.AddCommandHandler("flag", (Action<string>)(flag =>
        {
            var noted = flag.StartsWith(CluePrefix) && !Game.State.HasFlag(flag);
            Game.State.SetFlag(flag);
            if (noted) ClueNoted?.Invoke(flag);
        }));
        runner.AddCommandHandler("unflag", (Action<string>)(flag => Game.State.SetFlag(flag, false)));
        runner.AddCommandHandler("bond", (Action<string, int>)((who, amount) => Game.State.AddBond(who, amount)));
        runner.AddCommandHandler("wake_up", (Action<string>)(why => DayCycle.WakeUp(why)));
        runner.AddCommandHandler("end_game", (Action<string>)(caption => DayCycle.Finish(caption)));
    }

    /// <summary>The condition text of a "#needs:..." tag on a choice, or null.</summary>
    public static string? Needs(string[] metadata)
    {
        foreach (var tag in metadata)
            if (tag.StartsWith(NeedsTag)) return tag[NeedsTag.Length..];
        return null;
    }
}
