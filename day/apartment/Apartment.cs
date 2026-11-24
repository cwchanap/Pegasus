using Godot;
using Pegasus.Core;
using Pegasus.UI;
using Pegasus.World;

namespace Pegasus.Day;

/// <summary>His room by day. Bed ends the day once it is evening.</summary>
public partial class Apartment : Node3D
{
    [Export] public Interactable? Bed { get; set; }
    [Export] public Interactable? Desk { get; set; }
    [Export] public Label? ClockLabel { get; set; }

    public override void _Ready()
    {
        if (Bed != null) Bed.Interacted += OnBed;
        if (Desk != null) Desk.Interacted += OnDesk;
        Game.Changed += Rebuild;
        Rebuild();
    }

    public override void _ExitTree() => Game.Changed -= Rebuild;

    private void Rebuild()
    {
        // Temporary until the day HUD (C7).
        if (ClockLabel != null) ClockLabel.Text = Clock.Describe(Game.State);
    }

    private void OnBed()
    {
        if (Clock.CanSleep(Game.State)) DayCycle.GoToBed();
        else Toast.Show("It's too early to sleep.");
    }

    // Placeholder activity until activities land (C7).
    private void OnDesk()
    {
        if (!Clock.CanSpendSlot(Game.State))
        {
            Toast.Show("It's late. Time for bed.");
            return;
        }
        Clock.SpendSlot(Game.State);
        Toast.Show("You study for a while.");
    }
}
