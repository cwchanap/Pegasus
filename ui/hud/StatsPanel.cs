using Godot;
using Pegasus.Core;

namespace Pegasus.UI;

/// <summary>Top-right stats panel from the HUD mockup: a pip per stat level, then money.</summary>
public partial class StatsPanel : PanelContainer
{
    public override void _Ready()
    {
        foreach (var stat in System.Enum.GetValues<Stat>())
        {
            var pips = GetNode<HBoxContainer>($"%{stat}");
            for (var i = 0; i < GameState.MaxStat; i++)
                pips.AddChild(new Panel { CustomMinimumSize = new Vector2(30, 10), SizeFlagsVertical = SizeFlags.ShrinkCenter, MouseFilter = MouseFilterEnum.Ignore });
        }
        Game.Changed += Rebuild;
        Rebuild();
    }

    public override void _ExitTree() => Game.Changed -= Rebuild;

    private void Rebuild()
    {
        foreach (var stat in System.Enum.GetValues<Stat>())
        {
            var level = Game.State.GetStat(stat);
            var pips = GetNode<HBoxContainer>($"%{stat}").GetChildren();
            for (var i = 0; i < pips.Count; i++)
                ((Panel)pips[i]).ThemeTypeVariation = i < level ? "PipOn" : "PipOff";
        }
        GetNode<Label>("%Money").Text = Format.Yen(Game.State.Money);
    }
}
