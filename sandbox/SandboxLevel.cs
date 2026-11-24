using Godot;
using Pegasus.Conditions;
using Pegasus.Core;
using Pegasus.World;

namespace Pegasus.Sandbox;

/// <summary>
/// Grey-box test scene for the shared kit: walk, interact, conditions and save.
/// Starts from the save slot if there is one. Tab saves, Esc quits.
/// </summary>
public partial class SandboxLevel : Node3D
{
    private const string GateCondition = "flag:lantern_lit & guts >= 1";

    [Export] public Label? InfoLabel { get; set; }
    [Export] public Label? StateLabel { get; set; }
    [Export] public Interactable? Lantern { get; set; }
    [Export] public OmniLight3D? LanternLight { get; set; }
    [Export] public Interactable? Sign { get; set; }
    [Export] public Interactable? GateButton { get; set; }
    [Export] public Node3D? Gate { get; set; }

    public override void _Ready()
    {
        if (Game.Continue()) Say("Loaded the save slot.");
        Game.Changed += Rebuild;
        Rebuild();

        if (Lantern != null) Lantern.Interacted += ToggleLantern;
        if (Sign != null) Sign.Interacted += ReadSign;
        if (GateButton != null) GateButton.Interacted += TryGate;
        GD.Print("Sandbox ready");
    }

    public override void _ExitTree() => Game.Changed -= Rebuild;

    /// <summary>Scene follows the state, never the other way round.</summary>
    private void Rebuild()
    {
        var s = Game.State;
        if (LanternLight != null) LanternLight.Visible = s.HasFlag("lantern_lit");
        if (Gate != null) Gate.Visible = !s.HasFlag("gate_open");
        if (GateButton != null) GateButton.Enabled = !s.HasFlag("gate_open");
        if (StateLabel != null)
            StateLabel.Text = $"Guts {s.GetStat(Stat.Guts)} · flags [{string.Join(", ", s.Flags)}]";
    }

    private void ToggleLantern()
    {
        var lit = !Game.State.HasFlag("lantern_lit");
        Game.State.SetFlag("lantern_lit", lit);
        Say(lit ? "The lantern flickers on." : "The lantern goes dark.");
    }

    private void ReadSign()
    {
        Game.State.AddStat(Stat.Guts, 1);
        Say("The sign reads: \"Be brave.\" Guts +1");
    }

    private void TryGate()
    {
        var condition = Condition.Parse(GateCondition);
        if (!condition.IsMet(Game.State))
        {
            Say($"Locked. Needs {condition.Describe()}.");
            return;
        }
        Game.State.SetFlag("gate_open");
        Say("The gate swings open.");
    }

    private void Say(string text)
    {
        GD.Print(text);
        if (InfoLabel != null) InfoLabel.Text = text;
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (e.IsActionPressed("menu"))
        {
            Game.Save();
            Say($"Saved to {ProjectSettings.GlobalizePath(SaveService.Path)}");
        }
        else if (e.IsActionPressed("pause")) GetTree().Quit();
    }
}
