using Godot;
using Pegasus.World;

namespace Pegasus.Day;

/// <summary>An Interactable that offers an activity. The DayLocation hands it to the HUD panel.</summary>
public partial class ActivitySpot : Interactable
{
    [Export] public ActivityData? Activity { get; set; }

    public override void _Ready()
    {
        if (Activity != null) Prompt = Activity.Verb;
    }
}
