using Godot;
using Pegasus.Core;
using Pegasus.Dialogue;
using Pegasus.UI;
using Pegasus.World;

namespace Pegasus.Day;

/// <summary>His room by day. Bed ends the day once it is evening; the door leads to the city map.
/// The desk is an ActivitySpot (horror film), handled by DayLocation. The first afternoon opens
/// with the intro and two control tips; the morning after the first whiteout gets one line
/// about winter gear. That line is the only guidance the night ever gives.</summary>
public partial class Apartment : DayLocation
{
    [Export] public Interactable? Bed { get; set; }

    public override void _Ready()
    {
        base._Ready();
        if (Bed != null) Bed.Interacted += OnBed;
        SceneRouter.Instance.WhenIdle(Arrive);
    }

    private void Arrive()
    {
        var s = Game.State;
        if (!s.HasFlag(Flags.IntroSeen))
        {
            s.SetFlag(Flags.IntroSeen);
            Conversation.Start("Home_Intro", () =>
            {
                Toast.Show("WASD or the arrow keys to walk");
                Toast.Show("E or Space to look, talk and use");
            });
        }
        else if (s.HasFlag(Flags.WhitedOut) && !s.HasFlag(Flags.GearHint))
        {
            s.SetFlag(Flags.GearHint);
            Conversation.Start("Home_GearHint");
        }
    }

    private void OnBed()
    {
        if (Clock.CanSleep(Game.State)) DayCycle.GoToBed();
        else Toast.Show("It's too early to sleep.");
    }
}
