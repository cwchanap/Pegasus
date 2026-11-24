using System.Collections.Generic;
using Godot;

namespace Pegasus.Dev;

/// <summary>
/// Dev-only autoload for scripted runs, active in debug builds. User args (after `--`):
///   --shot=out.png        save a screenshot and quit
///   --shot-frames=N       physics frame (60/s) to take it on (default 90)
///   --hold=action@A-B     hold an input action from frame A to frame B (repeatable)
///   --tap=action@A        press an action for one frame at frame A (repeatable)
///   --state=file.json     start from this game state (same JSON as the save file)
/// </summary>
public partial class DevDriver : Node
{
    private string? _shotPath;
    private int _shotFrame = 90;
    private int _frame;
    private readonly List<(string Action, int From, int To)> _inputs = new();

    public override void _Ready()
    {
        SetPhysicsProcess(false);
        if (!OS.IsDebugBuild()) return;
        foreach (var arg in OS.GetCmdlineUserArgs())
        {
            var (key, value) = Split(arg, '=');
            switch (key)
            {
                case "--shot": _shotPath = value; break;
                case "--shot-frames": _shotFrame = int.Parse(value); break;
                case "--state": Pegasus.Core.Game.Load(Pegasus.Core.SaveData.FromJson(System.IO.File.ReadAllText(value))); break;
                case "--hold":
                {
                    var (action, range) = Split(value, '@');
                    var (from, to) = Split(range, '-');
                    _inputs.Add((action, int.Parse(from), int.Parse(to)));
                    break;
                }
                case "--tap":
                {
                    var (action, at) = Split(value, '@');
                    _inputs.Add((action, int.Parse(at), int.Parse(at)));
                    break;
                }
            }
        }
        SetPhysicsProcess(_shotPath != null || _inputs.Count > 0);
    }

    public override void _PhysicsProcess(double delta)
    {
        _frame++;
        foreach (var (action, from, to) in _inputs)
        {
            if (_frame == from) Press(action, true);
            if (_frame == to + 1) Press(action, false);
        }
        if (_shotPath == null || _frame < _shotFrame) return;
        GetViewport().GetTexture().GetImage().SavePng(ProjectSettings.GlobalizePath(_shotPath));
        GD.Print($"Screenshot saved to {_shotPath}");
        if (GetTree().GetFirstNodeInGroup("player") is Node3D player)
            GD.Print($"Player at {player.GlobalPosition}");
        GD.Print($"State: {Pegasus.Core.SaveData.ToJson(Pegasus.Core.Game.State).ReplaceLineEndings("")}");
        GetTree().Quit();
    }

    private static void Press(string action, bool pressed) =>
        Input.ParseInputEvent(new InputEventAction { Action = action, Pressed = pressed, Strength = pressed ? 1 : 0 });

    private static (string, string) Split(string s, char sep)
    {
        var i = s.IndexOf(sep);
        return i < 0 ? (s, "") : (s[..i], s[(i + 1)..]);
    }
}
