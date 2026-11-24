using Godot;
using Pegasus.Core;

namespace Pegasus.UI;

/// <summary>
/// The last screen of the slice: how many days it took, and back to the title. DayCycle.Enter shows it for an
/// ended save (<see cref="Flags.Ended"/>), so Continue never drops him back into the loop.
/// </summary>
public partial class EndCard : Control
{
    [Export] public Music Music { get; set; } = Music.Title;

    public override void _Ready()
    {
        AudioService.PlayMusic(Music);
        var days = Game.State.Day;
        GetNode<Label>("%Days").Text = $"You found your way out of the cold in {days} {(days == 1 ? "day" : "days")}.";
        var leave = GetNode<Button>("%Leave");
        leave.Pressed += Leave;
        leave.GrabFocus();
    }

    private static void Leave() => SceneRouter.Instance.GoTo(Scenes.Title);
}
