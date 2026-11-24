using System.Linq;
using Godot;

namespace Pegasus.Dev;

/// <summary>
/// Dev-only autoload. Run with `-- --shot=out.png [--shot-frames=90]` to save a
/// screenshot of the running scene and quit. Does nothing otherwise.
/// </summary>
public partial class DevScreenshot : Node
{
    private string? _path;
    private int _framesLeft = 90;

    public override void _Ready()
    {
        SetProcess(false);
        if (!OS.IsDebugBuild()) return;
        foreach (var arg in OS.GetCmdlineUserArgs())
        {
            if (arg.StartsWith("--shot=")) _path = arg["--shot=".Length..];
            else if (arg.StartsWith("--shot-frames=")) _framesLeft = int.Parse(arg["--shot-frames=".Length..]);
        }
        SetProcess(_path != null);
    }

    public override void _Process(double delta)
    {
        if (--_framesLeft > 0) return;
        var image = GetViewport().GetTexture().GetImage();
        image.SavePng(ProjectSettings.GlobalizePath(_path!));
        GD.Print($"Screenshot saved to {_path}");
        GetTree().Quit();
    }
}
