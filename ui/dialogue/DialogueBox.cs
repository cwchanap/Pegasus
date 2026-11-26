using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Godot;
using Pegasus.Conditions;
using Pegasus.Core;
using Pegasus.Dialogue;
using YarnSpinnerGodot;

namespace Pegasus.UI;

/// <summary>
/// Yarn dialogue presenter laid out like the dialogue mockup: portrait on the left,
/// name plate with bond pips, text box along the bottom, numbered choices top right.
/// Locked choices (#needs:...) stay visible with their requirement.
/// </summary>
public partial class DialogueBox : Control, DialoguePresenterBase
{
    private const float CharsPerSecond = 60f;

    public List<IActionMarkupHandler> ActionMarkupHandlers { get; } = new();

    private TextureRect _portrait = null!;
    private PanelContainer _plate = null!;
    private Label _name = null!, _subtitle = null!, _text = null!, _hint = null!;
    private VBoxContainer _options = null!;
    private Tween? _reveal;
    private TaskCompletionSource? _advance;
    private TaskCompletionSource<DialogueOption?>? _choice;
    private readonly List<(Button Button, DialogueOption Option)> _buttons = new();

    public override void _Ready()
    {
        SetAnchorsPreset(LayoutPreset.FullRect);
        MouseFilter = MouseFilterEnum.Ignore;
        Visible = false;
        Build();
    }

    // ---- DialoguePresenterBase ----

    public YarnTask OnDialogueStartedAsync()
    {
        _text.Text = "";
        Visible = true;
        return YarnTask.CompletedTask;
    }

    public YarnTask OnDialogueCompleteAsync()
    {
        Visible = false;
        return YarnTask.CompletedTask;
    }

    public async YarnTask RunLineAsync(LocalizedLine line, LineCancellationToken token)
    {
        ShowSpeaker(Conversation.Instance.FindCharacter(line.CharacterName), line.CharacterName, LineTags.GetMood(line.Metadata));
        _text.Text = line.TextWithoutCharacterName.Text;
        _hint.Text = "Next";
        _text.VisibleRatio = 0;
        _reveal?.Kill();
        _reveal = CreateTween();
        _reveal.TweenProperty(_text, "visible_ratio", 1f, Math.Max(0.05f, _text.Text.Length / CharsPerSecond));

        _advance = new TaskCompletionSource();
        await using (token.NextLineToken.Register(() => _advance.TrySetResult()))
            await _advance.Task;
        _advance = null;
        _reveal?.Kill();
        _text.VisibleRatio = 1;
    }

    public async YarnTask<DialogueOption?> RunOptionsAsync(DialogueOption[] options, CancellationToken cancellationToken)
    {
        _hint.Text = "Choose";
        _choice = new TaskCompletionSource<DialogueOption?>();
        var number = 1;
        foreach (var option in options.Where(o => o.IsAvailable))
            _buttons.Add((AddOption(number++, option), option));
        var stuck = !HasOpenChoice;
        if (stuck)
        {
            // Nothing the player can pick. The vendored runner never finishes an option step
            // that every presenter answers with null, so fallthrough cannot work: show the
            // locked rows, then end the conversation on the next press instead of freezing.
            GD.PushError("DialogueBox: every choice is locked or hidden; ending the conversation.");
            _hint.Text = "Close";
            if (_buttons.Count == 0) _choice.TrySetResult(null);
        }
        _buttons.FirstOrDefault(b => !b.Button.Disabled).Button?.GrabFocus();

        DialogueOption? chosen;
        await using (cancellationToken.Register(() => _choice.TrySetResult(null)))
            chosen = await _choice.Task;
        _choice = null;
        foreach (var (button, _) in _buttons) button.QueueFree();
        _buttons.Clear();
        if (stuck && !cancellationToken.IsCancellationRequested) Conversation.End();
        return chosen;
    }

    // ---- Input ----

    // Clicks advance in _Input so a full-screen blocker (the close-up's) can't eat them;
    // while choices are open the buttons take the click through the GUI instead.
    public override void _Input(InputEvent e)
    {
        if (!Visible || !IsClick(e)) return;
        if (_choice != null && HasOpenChoice) Callable.From(ShowFocus).CallDeferred();
        else Handle(e);
    }

    // A click beside the choices hides the focused row's highlight; bring it back so the
    // player still sees which one E will pick.
    private void ShowFocus()
    {
        if (_choice != null && GetViewport().GuiGetFocusOwner() is Button focused && _buttons.Any(b => b.Button == focused))
            focused.GrabFocus();
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (Visible && !IsClick(e)) Handle(e);
    }

    private void Handle(InputEvent e)
    {
        if (_choice != null)
        {
            if (!HasOpenChoice)
            {
                if (IsAdvance(e)) { Accept(); _choice.TrySetResult(null); }
                return;
            }
            if (e.IsActionPressed("interact") && GetViewport().GuiGetFocusOwner() is Button { Disabled: false } focused)
            {
                Accept();
                focused.EmitSignal(BaseButton.SignalName.Pressed);
            }
            else if (e is InputEventKey { Pressed: true, Echo: false } key && key.Keycode is >= Key.Key1 and <= Key.Key9)
            {
                var index = (int)(key.Keycode - Key.Key1);
                if (index < _buttons.Count && !_buttons[index].Button.Disabled)
                {
                    Accept();
                    _buttons[index].Button.EmitSignal(BaseButton.SignalName.Pressed);
                }
            }
            return;
        }
        if (!IsAdvance(e) || _advance == null) return;
        Accept();
        if (_reveal != null && _reveal.IsRunning())
        {
            _reveal.Kill();
            _text.VisibleRatio = 1;
        }
        else _advance.TrySetResult();
    }

    private bool HasOpenChoice => _buttons.Any(b => !b.Button.Disabled);

    private static bool IsAdvance(InputEvent e) =>
        e.IsActionPressed("interact") || e.IsActionPressed("ui_accept") || IsClick(e);

    private static bool IsClick(InputEvent e) => e is InputEventMouseButton { Pressed: true, ButtonIndex: MouseButton.Left };

    private void Accept() => GetViewport().SetInputAsHandled();

    // ---- Layout ----

    private void ShowSpeaker(CharacterProfile? profile, string? name, Mood mood)
    {
        _portrait.Texture = profile?.PortraitFor(mood);
        _portrait.Visible = _portrait.Texture != null;
        _plate.Visible = name != null;
        _name.Text = profile?.DisplayName ?? name ?? "";
        var details = new List<string>();
        if (!string.IsNullOrEmpty(profile?.Subtitle)) details.Add(profile.Subtitle);
        if (profile is { HasBond: true })
        {
            var bond = Game.State.GetBond(profile.Id);
            details.Add("Bond " + new string('◆', bond) + new string('◇', GameState.MaxBond - bond));
        }
        _subtitle.Text = string.Join(" · ", details);
        // The plate takes the speaker's colour on top of the theme's NamePlate style.
        var plate = (StyleBoxFlat)_plate.GetThemeStylebox("panel", "NamePlate").Duplicate();
        if (profile != null) plate.BgColor = profile.Color;
        _plate.AddThemeStyleboxOverride("panel", plate);
        // Text starts right of the portrait when there is one.
        _text.OffsetLeft = _portrait.Visible ? 340 : 40;
        _plate.OffsetLeft = _portrait.Visible ? 300 : 24;
    }

    private Button AddOption(int number, DialogueOption option)
    {
        var needs = LineTags.Needs(option.Line.Metadata);
        var condition = needs == null ? null : Condition.Parse(needs);
        var locked = condition != null && !condition.IsMet(Game.State);

        var button = new Button
        {
            CustomMinimumSize = new Vector2(0, 50),
            Disabled = locked,
            FocusMode = locked ? FocusModeEnum.None : FocusModeEnum.All,
            MouseFilter = MouseFilterEnum.Stop,
            ThemeTypeVariation = "ChoiceButton",
        };

        var row = new HBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
        row.AddThemeConstantOverride("separation", 12);
        row.SetAnchorsPreset(LayoutPreset.FullRect);
        row.OffsetLeft = 14; row.OffsetRight = -16;
        button.AddChild(row);

        var badge = RowLabel(number.ToString(), "ChoiceNumber");
        row.AddChild(badge);
        var tagText = condition?.Describe() ?? "";
        if (tagText != "")
            row.AddChild(RowLabel(tagText.ToUpperInvariant(), locked ? "TagMuted" : "TagYellow"));
        var text = RowLabel(option.Line.TextWithoutCharacterName.Text, locked ? "ChoiceTextLocked" : "ChoiceText");
        text.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        text.TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis;
        row.AddChild(text);
        if (locked && condition is StatAtLeast stat)
            row.AddChild(RowLabel($"{stat.Stat} {Game.State.GetStat(stat.Stat)} of {stat.Level}", "FaintLabel"));

        // Dark text only while the white focus style is drawn: a mouse click keeps focus on the
        // button but hides its style, which left dark text on the dark row.
        button.Draw += () =>
        {
            var lit = button.HasFocus(ignoreHiddenFocus: true);
            text.ThemeTypeVariation = lit ? "ChoiceTextFocus" : locked ? "ChoiceTextLocked" : "ChoiceText";
            badge.ThemeTypeVariation = lit ? "ChoiceNumberFocus" : "ChoiceNumber";
        };
        button.MouseEntered += () => { if (!locked) button.GrabFocus(); };
        button.Pressed += () => _choice?.TrySetResult(option);
        _options.AddChild(button);
        return button;
    }

    private static Label RowLabel(string text, string variation) => new()
    {
        Text = text,
        ThemeTypeVariation = variation,
        VerticalAlignment = VerticalAlignment.Center,
        SizeFlagsVertical = SizeFlags.ShrinkCenter,
        MouseFilter = MouseFilterEnum.Ignore,
    };

    private void Build()
    {
        _options = new VBoxContainer { MouseFilter = MouseFilterEnum.Ignore, Alignment = BoxContainer.AlignmentMode.End };
        _options.AddThemeConstantOverride("separation", 10);
        _options.AnchorLeft = 0.45f; _options.AnchorRight = 1; _options.AnchorTop = 0; _options.AnchorBottom = 1;
        _options.OffsetLeft = 20; _options.OffsetRight = -48; _options.OffsetTop = 40; _options.OffsetBottom = -240;
        AddChild(_options);

        var box = new Panel { MouseFilter = MouseFilterEnum.Ignore, ThemeTypeVariation = "CreamBox" };
        box.AnchorLeft = 0; box.AnchorRight = 1; box.AnchorTop = 1; box.AnchorBottom = 1;
        box.OffsetLeft = 60; box.OffsetRight = -60; box.OffsetTop = -205; box.OffsetBottom = -36;
        AddChild(box);

        _text = new Label { AutowrapMode = TextServer.AutowrapMode.WordSmart, MouseFilter = MouseFilterEnum.Ignore, ThemeTypeVariation = "DialogueText" };
        _text.AnchorRight = 1; _text.AnchorBottom = 1;
        _text.OffsetLeft = 340; _text.OffsetTop = 44; _text.OffsetRight = -40; _text.OffsetBottom = -40;
        box.AddChild(_text);

        var hintRow = new HBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
        hintRow.AddThemeConstantOverride("separation", 8);
        hintRow.AnchorLeft = 1; hintRow.AnchorRight = 1; hintRow.AnchorTop = 1; hintRow.AnchorBottom = 1;
        hintRow.OffsetLeft = -140; hintRow.OffsetRight = -22; hintRow.OffsetTop = -44; hintRow.OffsetBottom = -14;
        hintRow.Alignment = BoxContainer.AlignmentMode.End;
        hintRow.AddChild(RowLabel("E", "KeyCapGrey"));
        _hint = RowLabel("Next", "MutedLabel");
        hintRow.AddChild(_hint);
        box.AddChild(hintRow);

        _plate = new PanelContainer { MouseFilter = MouseFilterEnum.Ignore, ThemeTypeVariation = "NamePlate" };
        _plate.AnchorTop = 1; _plate.AnchorBottom = 1;
        _plate.OffsetLeft = 300; _plate.OffsetTop = -232; _plate.OffsetBottom = -190;
        var plateRow = new HBoxContainer();
        plateRow.AddThemeConstantOverride("separation", 12);
        _name = new Label { VerticalAlignment = VerticalAlignment.Center, ThemeTypeVariation = "TitleLabel" };
        _subtitle = new Label { VerticalAlignment = VerticalAlignment.Center, ThemeTypeVariation = "SubtitleLabel" };
        plateRow.AddChild(_name);
        plateRow.AddChild(_subtitle);
        _plate.AddChild(plateRow);
        AddChild(_plate);

        _portrait = new TextureRect
        {
            ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize,
            StretchMode = TextureRect.StretchModeEnum.KeepAspectCovered,
            MouseFilter = MouseFilterEnum.Ignore,
            // Portraits are drawn at 4x for big screens; mipmaps keep the line art clean at 720p.
            TextureFilter = TextureFilterEnum.LinearWithMipmaps,
        };
        _portrait.AnchorTop = 1; _portrait.AnchorBottom = 1;
        _portrait.OffsetLeft = 80; _portrait.OffsetRight = 330; _portrait.OffsetTop = -560; _portrait.OffsetBottom = 0;
        AddChild(_portrait);
    }
}
