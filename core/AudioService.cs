using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace Pegasus.Core;

/// <summary>Music cues. File: content/audio/music/{snake_case}.ogg. Every track must loop seamlessly.</summary>
public enum Music
{
    /// <summary>No music; for a puzzle, keep the world's.</summary>
    None,
    /// <summary>Title screen and end card. About 1 to 2 min.</summary>
    Title,
    /// <summary>Osaka by day: apartment, city map, places. About 2 min.</summary>
    Day,
    /// <summary>The snow world at 3 AM. About 2 min.</summary>
    Night,
    /// <summary>Kissa Mori, the locked room. Tension. About 1 to 2 min.</summary>
    LockedRoom,
}

/// <summary>Sound cues. File: content/audio/sfx/{snake_case}.ogg. One-shots, under 2 s unless noted.</summary>
public enum Sfx
{
    /// <summary>Any button pressed (menus, choices, hotspots).</summary>
    UiClick,
    /// <summary>The Tab menu opens.</summary>
    MenuOpen,
    /// <summary>A door: leaving a place by day, the home door at night.</summary>
    Door,
    /// <summary>One footstep indoors or on pavement (played every ~0.35 s while walking).</summary>
    StepFloor,
    /// <summary>One footstep in deep snow.</summary>
    StepSnow,
    /// <summary>He gets or buys an item.</summary>
    ItemGet,
    /// <summary>A puzzle is solved.</summary>
    PuzzleSolved,
    /// <summary>A new notebook entry (pencil scratch).</summary>
    Notebook,
    /// <summary>Warmth drops below a quarter.</summary>
    WarmthLow,
    /// <summary>Warmth runs out and everything goes white. About 2 s.</summary>
    Whiteout,
}

/// <summary>
/// Autoload. Plays music (looping, crossfaded) on the Music bus and one-shot sounds on the SFX bus.
/// Cues are the enums above; a cue whose file doesn't exist yet is skipped (with a warning in debug
/// builds), so the game runs silent until the sound tickets land. Loads <see cref="Settings"/> first,
/// since they set the bus volumes.
/// </summary>
public partial class AudioService : Node
{
    private const float FadeSeconds = 1.5f;

    private static AudioService? _instance;
    private static readonly HashSet<string> Missing = new();

    private AudioStreamPlayer _a = null!, _b = null!;
    private Tween? _fade;

    /// <summary>The music playing now (or that would be, if its file is missing).</summary>
    public static Music Current { get; private set; }

    public override void _EnterTree()
    {
        _instance = this;
        ProcessMode = ProcessModeEnum.Always;
        Settings.Load();
        _a = new AudioStreamPlayer { Bus = "Music" };
        _b = new AudioStreamPlayer { Bus = "Music" };
        AddChild(_a);
        AddChild(_b);
        // Every button clicks, wherever it is built.
        GetTree().NodeAdded += node =>
        {
            if (node is BaseButton button) button.Pressed += () => PlaySfx(Sfx.UiClick);
        };
    }

    /// <summary>Crossfade to <paramref name="music"/>. The same cue again does nothing.</summary>
    public static void PlayMusic(Music music)
    {
        if (_instance == null || music == Current) return;
        Current = music;
        _instance.Crossfade(music == Music.None ? null : Load("music", music.ToString()));
    }

    public static void PlaySfx(Sfx sfx)
    {
        if (_instance == null || Load("sfx", sfx.ToString()) is not { } stream) return;
        var player = new AudioStreamPlayer { Stream = stream, Bus = "SFX" };
        _instance.AddChild(player);
        player.Finished += player.QueueFree;
        player.Play();
    }

    private void Crossfade(AudioStream? stream)
    {
        _fade?.Kill();
        (_a, _b) = (_b, _a); // _a is now the incoming player, _b the outgoing one
        var incoming = _a;
        var outgoing = _b;
        var outFrom = Mathf.DbToLinear(outgoing.VolumeDb);
        incoming.Stream = stream;
        incoming.VolumeDb = Mathf.LinearToDb(0);
        if (stream != null) incoming.Play();
        // Fade in linear volume; a tween over decibels stays near silent for most of the fade.
        _fade = CreateTween();
        _fade.TweenMethod(Callable.From<float>(t =>
        {
            incoming.VolumeDb = Mathf.LinearToDb(t);
            outgoing.VolumeDb = Mathf.LinearToDb(outFrom * (1 - t));
        }), 0f, 1f, FadeSeconds);
        _fade.TweenCallback(Callable.From(outgoing.Stop));
    }

    private static AudioStream? Load(string folder, string cue)
    {
        var path = $"res://content/audio/{folder}/{Snake(cue)}.ogg";
        if (!ResourceLoader.Exists(path))
        {
            if (OS.IsDebugBuild() && Missing.Add(path)) GD.PushWarning($"AudioService: no file yet at {path}");
            return null;
        }
        var stream = GD.Load<AudioStream>(path);
        if (folder == "music" && stream is AudioStreamOggVorbis ogg) ogg.Loop = true;
        return stream;
    }

    private static string Snake(string name) =>
        string.Concat(name.Select((c, i) => i > 0 && char.IsUpper(c) ? "_" + char.ToLowerInvariant(c) : char.ToLowerInvariant(c).ToString()));
}
