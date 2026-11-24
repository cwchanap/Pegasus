using System;
using System.Linq;
using Godot;
using Godot.Collections;
using Pegasus.Core;
using YarnSpinnerGodot;

namespace Pegasus.Dialogue;

/// <summary>
/// Autoload (ui/dialogue/conversation.tscn). Runs one Yarn node at a time through the
/// dialogue box and holds the input lock while it does.
/// </summary>
public partial class Conversation : CanvasLayer
{
    public static Conversation Instance { get; private set; } = null!;

    [Export] public DialogueRunner? Runner { get; set; }
    [Export] public Array<CharacterProfile> Characters { get; set; } = new();

    /// <summary>Raised when a conversation ends, with the node it started from.</summary>
    public static event Action<string>? Finished;

    public static bool Active { get; private set; }

    private string _node = "";

    public override void _EnterTree() => Instance = this;

    public override void _Ready()
    {
        ProcessMode = ProcessModeEnum.Always;
        YarnBindings.Register(Runner!);
        Runner!.onDialogueComplete += OnComplete;
    }

    public static void Start(string node)
    {
        if (Active) return;
        Active = true;
        Instance._node = node;
        InputLock.Acquire(nameof(Conversation));
        Instance.Runner!.StartDialogueForget(node);
    }

    public CharacterProfile? FindCharacter(string? displayName) =>
        displayName == null ? null : Characters.FirstOrDefault(c => c.DisplayName == displayName);

    private void OnComplete()
    {
        Active = false;
        // Release a frame later so the key that closed the box does not also trigger the world.
        GetTree().CreateTimer(0.1).Timeout += () => InputLock.Release(nameof(Conversation));
        Finished?.Invoke(_node);
    }
}
