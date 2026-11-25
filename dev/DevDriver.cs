using System.Collections.Generic;
using Godot;

namespace Pegasus.Dev;

/// <summary>
/// Dev-only autoload for scripted runs, active in debug builds. User args (after `--`):
///   --shot=out.png        save a screenshot and quit
///   --shot-frames=N       physics frame (60/s) to take it on (default 90)
///   --hold=action@A-B     hold an input action from frame A to frame B (repeatable)
///   --tap=action@A        press an action for one frame at frame A (repeatable)
///   --key=KEY@A           press a physical key (a Godot Key name: D, Escape, Tab, Space) at frame A (repeatable)
///   --state=file.json     start from this game state (same JSON as the save file)
///   --open=res://x.tscn   open this puzzle scene in the close-up on frame 5 (pass a gameplay
///                         scene too: the title's fade holds the input lock then); exits 1 if it can't
///   --click=X,Y@A         left-click at viewport position X,Y on frame A (repeatable)
///   --at=X,Z              put the player at X,Z on frame 2 (skip the walk to a spot; needs a
///                         gameplay scene, exits 1 if there is no player)
///   --talk[=F]            advance any conversation by itself (picks the focused, first open choice), until frame F if given
/// Frames count while the tree is paused too (fades, the Tab menu), so a shot can catch them.
/// </summary>
public partial class DevDriver : Node
{
    private string? _shotPath;
    private int _shotFrame = 90;
    private int _frame;
    private readonly List<(string Action, int From, int To)> _inputs = new();
    private readonly List<(Vector2 At, int Frame)> _clicks = new();
    private readonly List<(Key Key, int Frame)> _keys = new();
    private string? _puzzle;
    private Vector3? _at;
    private bool _talk;
    private int _talkUntil = int.MaxValue;

    public override void _Ready()
    {
        SetPhysicsProcess(false);
        ProcessMode = ProcessModeEnum.Always;
        if (!OS.IsDebugBuild()) return;
        foreach (var arg in OS.GetCmdlineUserArgs())
        {
            var (key, value) = Split(arg, '=');
            switch (key)
            {
                case "--shot": _shotPath = value; break;
                case "--shot-frames": _shotFrame = int.Parse(value); break;
                case "--state":
                    try { Pegasus.Core.Game.Load(Pegasus.Core.SaveData.FromJson(System.IO.File.ReadAllText(value))); }
                    catch (System.Exception e) when (e is System.IO.IOException or System.Text.Json.JsonException)
                    {
                        GD.PushError($"--state: could not load {value}: {e.Message}");
                        GetTree().Quit(1);
                        return;
                    }
                    Pegasus.Core.SaveService.Off = true; // a fixture run never touches the real save
                    break;
                case "--hold":
                {
                    var (action, range) = Split(value, '@');
                    var (from, to) = Split(range, '-');
                    _inputs.Add((action, int.Parse(from), int.Parse(to)));
                    break;
                }
                case "--open": _puzzle = value; break;
                case "--talk": _talk = true; if (value != "") _talkUntil = int.Parse(value); break;
                case "--at":
                {
                    var (x, z) = Split(value, ',');
                    _at = new Vector3(float.Parse(x), 0, float.Parse(z));
                    break;
                }
                case "--click":
                {
                    var (xy, at) = Split(value, '@');
                    var (x, y) = Split(xy, ',');
                    _clicks.Add((new Vector2(float.Parse(x), float.Parse(y)), int.Parse(at)));
                    break;
                }
                case "--key":
                {
                    var (name, at) = Split(value, '@');
                    _keys.Add((System.Enum.Parse<Key>(name, true), int.Parse(at)));
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
        SetPhysicsProcess(_shotPath != null || _inputs.Count > 0 || _keys.Count > 0 || _clicks.Count > 0 || _puzzle != null || _at != null || _talk);
    }

    public override void _PhysicsProcess(double delta)
    {
        _frame++;
        foreach (var (action, from, to) in _inputs)
        {
            if (_frame == from) Press(action, true);
            if (_frame == to + 1) Press(action, false);
        }
        foreach (var (key, frame) in _keys)
            if (_frame == frame || _frame == frame + 1)
                Input.ParseInputEvent(new InputEventKey { PhysicalKeycode = key, Keycode = key, Pressed = _frame == frame });
        var talk = _talk && _frame < _talkUntil;
        if (talk && Pegasus.Dialogue.Conversation.Active && _frame % 14 == 0) Press("interact", true);
        if (talk && _frame % 14 == 1) Press("interact", false);
        if (_frame == 2 && _at is { } at0)
        {
            if (GetTree().GetFirstNodeInGroup("player") is Node3D p) p.GlobalPosition = at0;
            else
            {
                GD.PushError("--at: no player on frame 2 (pass a gameplay scene)");
                GetTree().Quit(1);
                return;
            }
        }
        if (_frame == 5 && _puzzle != null)
        {
            Pegasus.Puzzles.CloseUp.Open(GD.Load<PackedScene>(_puzzle));
            if (!Pegasus.Puzzles.CloseUp.IsOpen)
            {
                GD.PushError($"--open: could not open {_puzzle} on frame 5 (input lock busy; pass a gameplay scene)");
                GetTree().Quit(1);
                return; // or a shot due this frame would quit again with 0
            }
        }
        foreach (var (at, frame) in _clicks)
        {
            if (_frame == frame - 1) Input.ParseInputEvent(new InputEventMouseMotion { Position = at, GlobalPosition = at });
            if (_frame == frame) Click(at, true);
            if (_frame == frame + 1) Click(at, false);
        }
        if (_shotPath == null || _frame < _shotFrame) return;
        var error = GetViewport().GetTexture().GetImage().SavePng(ProjectSettings.GlobalizePath(_shotPath));
        if (error != Error.Ok)
        {
            GD.PushError($"Could not save the screenshot to {_shotPath}: {error}");
            GetTree().Quit(1);
            return;
        }
        GD.Print($"Screenshot saved to {_shotPath}");
        if (GetTree().GetFirstNodeInGroup("player") is Node3D player)
            GD.Print($"Player at {player.GlobalPosition}");
        GD.Print($"Scene: {GetTree().CurrentScene?.SceneFilePath}");
        GD.Print($"State: {Pegasus.Core.SaveData.ToJson(Pegasus.Core.Game.State).ReplaceLineEndings("")}");
        GetTree().Quit();
    }

    private static void Press(string action, bool pressed) =>
        Input.ParseInputEvent(new InputEventAction { Action = action, Pressed = pressed, Strength = pressed ? 1 : 0 });

    private static void Click(Vector2 at, bool pressed) =>
        Input.ParseInputEvent(new InputEventMouseButton { Position = at, GlobalPosition = at, ButtonIndex = MouseButton.Left, Pressed = pressed });

    private static (string, string) Split(string s, char sep)
    {
        var i = s.IndexOf(sep);
        return i < 0 ? (s, "") : (s[..i], s[(i + 1)..]);
    }
}
