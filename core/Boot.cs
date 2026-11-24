using System.Linq;
using Godot;

namespace Pegasus.Core;

/// <summary>Main scene. Continues the save (or starts fresh with `-- --new-game`) and enters the world.</summary>
public partial class Boot : Node
{
    public override void _Ready()
    {
        var fresh = OS.GetCmdlineUserArgs().Contains("--new-game");
        if (fresh || !Game.Continue()) Game.NewGame();
        CallDeferred(nameof(Enter));
    }

    private void Enter() => DayCycle.Enter();
}
