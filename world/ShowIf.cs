using Godot;
using Pegasus.Conditions;
using Pegasus.Core;

namespace Pegasus.World;

/// <summary>
/// Shows this node (and its children) only while a condition holds, e.g. "flag:lanterns_lit"
/// for a lantern's flame or "!flag:lanterns_lit" for the snow on a door. Visual only: it
/// doesn't switch collision, so keep solid shapes out of it.
/// </summary>
public partial class ShowIf : Node3D
{
    [Export] public string Condition { get; set; } = "";

    private Condition _condition = new Always();

    public override void _Ready()
    {
        _condition = Conditions.Condition.Parse(Condition);
        Game.Changed += Refresh;
        Refresh();
    }

    public override void _ExitTree() => Game.Changed -= Refresh;

    private void Refresh() => Visible = _condition.IsMet(Game.State);
}
