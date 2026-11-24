using Godot;
using Pegasus.Conditions;
using Pegasus.Core;
using Pegasus.World;

namespace Pegasus.Sandbox;

/// <summary>
/// Grey-box test scene for the shared kit: walk, interact, conditions and save.
/// Starts from its own save slot (sandbox_save.json) if there is one. F5 saves, Esc quits.
/// </summary>
public partial class SandboxLevel : Node3D
{
    /// <summary>Its own slot, so sandbox runs never touch the game's save.</summary>
    private const string SavePath = "user://sandbox_save.json";
    private const string GateCondition = "flag:lantern_lit & guts >= 1";

    [Export] public Label? InfoLabel { get; set; }
    [Export] public Label? StateLabel { get; set; }
    [Export] public Interactable? Lantern { get; set; }
    [Export] public OmniLight3D? LanternLight { get; set; }
    [Export] public Interactable? Sign { get; set; }
    [Export] public Interactable? GateButton { get; set; }
    [Export] public Node3D? Gate { get; set; }
    [Export] public CollisionShape3D? GateShape { get; set; }

    public override void _Ready()
    {
        if (SaveService.Load(SavePath) is { } saved)
        {
            Game.Load(saved);
            Say("Loaded the sandbox save.");
        }
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
        GateShape?.SetDeferred(CollisionShape3D.PropertyName.Disabled, s.HasFlag("gate_open"));
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
            Say("It won't budge. Maybe with some light, and some nerve.");
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
        if (e is InputEventKey { Pressed: true, Echo: false, Keycode: Key.F5 })
        {
            SaveService.Save(Game.State, SavePath);
            Say($"Saved to {ProjectSettings.GlobalizePath(SavePath)}");
        }
    }
}
