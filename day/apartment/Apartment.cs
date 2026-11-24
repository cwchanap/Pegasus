using Godot;
using Pegasus.Core;
using Pegasus.UI;
using Pegasus.World;

namespace Pegasus.Day;

/// <summary>His room by day. Bed ends the day once it is evening; the door leads to the city map.
/// The desk is an ActivitySpot (horror film), handled by DayLocation.</summary>
public partial class Apartment : DayLocation
{
    [Export] public Interactable? Bed { get; set; }

    public override void _Ready()
    {
        base._Ready();
        if (Bed != null) Bed.Interacted += OnBed;
    }

    private void OnBed()
    {
        if (Clock.CanSleep(Game.State)) DayCycle.GoToBed();
        else Toast.Show("It's too early to sleep.");
    }
}
