using Godot;
using Pegasus.Core;

namespace Pegasus.UI;

/// <summary>
/// The last screen of the slice: how many days it took, and the way out. Boot shows it for an
/// ended save (<see cref="Flags.Ended"/>), so Continue never drops him back into the loop.
/// </summary>
public partial class EndCard : Control
{
    public override void _Ready()
    {
        var days = Game.State.Day;
        GetNode<Label>("%Days").Text = $"You found your way out of the cold in {days} {(days == 1 ? "day" : "days")}.";
        var leave = GetNode<Button>("%Leave");
        leave.Pressed += Leave;
        leave.GrabFocus();
    }

    /// <summary>Until there is a title screen, leaving quits the game. The title screen goes here.</summary>
    private void Leave() => GetTree().Quit();
}
