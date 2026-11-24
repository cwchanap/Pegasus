using System.Linq;
using Godot;
using Pegasus.Core;

namespace Pegasus.UI;

/// <summary>
/// Main scene. Continue (when there is a save and it hasn't ended), New game (confirms before it
/// replaces a save that can still be continued), Settings and Quit. Dev runs can skip it with `-- --new-game`, `-- --continue` or `-- --state=file.json`.
/// </summary>
public partial class Title : Control
{
    [Export] public Music Music { get; set; } = Music.Title;

    private GameState? _saved;
    private static bool _booted; // the dev skip args apply to the first title only

    public override void _Ready()
    {
        AudioService.PlayMusic(Music);
        var args = _booted ? [] : OS.GetCmdlineUserArgs();
        _booted = true;
        // A --state fixture the dev driver loaded (it then turns saving off) wins over the save and a fresh game.
        if (args.Length > 0 && SaveService.Off) { Callable.From(DayCycle.Enter).CallDeferred(); return; }
        if (args.Contains("--new-game")) { Callable.From(StartNew).CallDeferred(); return; }
        if (args.Contains("--continue")) { Callable.From(ContinueOrNew).CallDeferred(); return; }

        _saved = SaveService.Load();
        var canContinue = _saved != null && !_saved.HasFlag(Flags.Ended);
        Button("Continue").Visible = canContinue;
        Button("Continue").Pressed += () => { Game.Load(_saved!); DayCycle.Enter(); };
        Button("NewGame").Pressed += () => { if (canContinue) Show("%Confirm", Button("StartOver")); else StartNew(); };
        Button("StartOver").Pressed += StartNew;
        Button("Cancel").Pressed += ShowButtons;
        Button("Settings").Pressed += () => Show("%SettingsPage", GetNode<SettingsPanel>("%SettingsPage/Panel").First);
        Button("Back").Pressed += ShowButtons;
        Button("Quit").Pressed += () => GetTree().Quit();
        ShowButtons();
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (!e.IsActionPressed("ui_cancel") || GetNode<Control>("%Buttons").Visible) return;
        GetViewport().SetInputAsHandled();
        ShowButtons();
    }

    private static void StartNew()
    {
        Game.NewGame();
        Game.Save();
        DayCycle.Enter();
    }

    private static void ContinueOrNew()
    {
        if (!Game.Continue()) Game.NewGame();
        DayCycle.Enter();
    }

    private void ShowButtons()
    {
        Show("%Buttons", Button(Button("Continue").Visible ? "Continue" : "NewGame"));
    }

    /// <summary>Show one of the three groups (buttons, confirm, settings) and focus a control in it.</summary>
    private void Show(string group, Control focus)
    {
        foreach (var name in new[] { "%Buttons", "%Confirm", "%SettingsPage" })
            GetNode<Control>(name).Visible = name == group;
        focus.GrabFocus();
    }

    private Button Button(string name) => GetNode<Button>($"%{name}");
}
