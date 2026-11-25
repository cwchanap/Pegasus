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

    /// <summary>Starts a node. False if one is already running or the node does not exist.</summary>
    public static bool Start(string node)
    {
        if (Active) return false;
        if (!Instance.Runner!.Dialogue.NodeExists(node))
        {
            GD.PushError($"Conversation: no Yarn node named '{node}'.");
            return false;
        }
        Active = true;
        Instance._node = node;
        InputLock.Acquire(nameof(Conversation));
        Instance.Runner!.StartDialogueForget(node);
        return true;
    }

    /// <summary>Starts a node and runs <paramref name="then"/> once it ends, or at once if it could not start.</summary>
    public static bool Start(string node, Action then)
    {
        void Done(string _)
        {
            Finished -= Done;
            then();
        }
        Finished += Done;
        if (Start(node)) return true;
        Done(node);
        return false;
    }

    /// <summary>Ends the running conversation early; Finished still fires.</summary>
    public static void End()
    {
        if (Active) Instance.Runner!.StopForget();
    }

    public CharacterProfile? FindCharacter(string? displayName) =>
        displayName == null ? null : Characters.FirstOrDefault(c => c.DisplayName == displayName);

    // The runner emits onDialogueComplete before it disposes that conversation's cancellation
    // source, so a Finished handler that starts the next node right away would have its new
    // source torn down (and could no longer be stopped). Finish on the next idle instead; Active
    // and the lock stay held until then.
    private void OnComplete() => Callable.From(Complete).CallDeferred();

    private void Complete()
    {
        Active = false;
        // Release a moment later so the key that closed the box does not also trigger the world,
        // unless a Finished handler has already started the next conversation.
        GetTree().CreateTimer(0.1).Timeout += () =>
        {
            if (!Active) InputLock.Release(nameof(Conversation));
        };
        Finished?.Invoke(_node);
    }
}
