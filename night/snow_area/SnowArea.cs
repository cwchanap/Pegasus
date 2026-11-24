using Pegasus.Core;
using Pegasus.World;
using Godot;

namespace Pegasus.Night;

/// <summary>The snow world. Grey-box placeholder until C10; going back in ends the night.</summary>
public partial class SnowArea : Node3D
{
    [Export] public Interactable? HomeDoor { get; set; }

    public override void _Ready()
    {
        if (HomeDoor != null) HomeDoor.Interacted += DayCycle.WakeUp;
    }
}
