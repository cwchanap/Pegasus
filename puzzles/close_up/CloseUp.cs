using System.Collections.Generic;
using System.Linq;
using Godot;
using Pegasus.Core;
using Pegasus.Dialogue;
using Pegasus.UI;

namespace Pegasus.Puzzles;

/// <summary>
/// Autoload (puzzles/close_up/close_up.tscn): the one close-up layout from the mockup that
/// every puzzle opens in. Title and view dots on top, the puzzle's current view in the
/// middle with arrows to turn, his items along the bottom (pick one, then a hotspot to use
/// it there) and Warmth, paused, at night. It holds the input lock while open, so night
/// Warmth stops; Esc steps back and drops any partial progress. Mouse and keyboard both
/// work: arrows move focus, E, Space or Enter press, Esc leaves.
/// </summary>
public partial class CloseUp : CanvasLayer
{
    private static readonly Vector2 SlotSize = new(84, 84);
    private const int MinSlots = 6;

    private static CloseUp? _instance;

    private Puzzle? _puzzle;
    private int _view;
    private string? _using;
    private bool _closeAfter;
    private Control? _lastFocus;
    private readonly ButtonGroup _slotGroup = new() { AllowUnpress = true };

    public static bool IsOpen => _instance?._puzzle != null;

    public static void Open(PackedScene scene) => _instance?.OpenScene(scene);

    public override void _EnterTree() => _instance = this;

    public override void _Ready()
    {
        Visible = false;
        Conversation.Finished += OnConversationFinished;
        GetNode<Button>("%Prev").Pressed += () => Turn(-1);
        GetNode<Button>("%Next").Pressed += () => Turn(1);
    }

    public override void _ExitTree() => Conversation.Finished -= OnConversationFinished;

    private void OpenScene(PackedScene scene)
    {
        if (_puzzle != null || !InputLock.IsFree) return;
        InputLock.Acquire(nameof(CloseUp));
        _puzzle = scene.Instantiate<Puzzle>();
        _puzzle.Frame = this;
        GetNode<Control>("%Stage").AddChild(_puzzle);
        foreach (var spot in Hotspots())
        {
            spot.Pressed += () => OnHotspot(spot);
            spot.FocusEntered += () => ShowTag(spot);
            spot.FocusExited += HideTag;
        }
        _view = 0;
        _using = null;
        _closeAfter = false;
        _lastFocus = null;
        ShowView();
        BuildSlots();
        ShowWarmth();
        Visible = true;
        FocusFirst();
    }

    private void Close()
    {
        if (_puzzle == null) return;
        Visible = false;
        _puzzle.QueueFree();
        _puzzle = null;
        HideTag();
        GetNode<Control>("%Blocker").Visible = false;
        GetViewport().GuiReleaseFocus();
        Callable.From(() => InputLock.Release(nameof(CloseUp))).CallDeferred();
    }

    public override void _Input(InputEvent e)
    {
        if (_puzzle == null || Conversation.Active) return;
        if (e.IsActionPressed("pause") || e.IsActionPressed("ui_cancel"))
        {
            GetViewport().SetInputAsHandled();
            Close();
        }
        // E presses the focused button too; Space and Enter already do (ui_accept).
        else if (e.IsActionPressed("interact") && !e.IsActionPressed("ui_accept") && GetViewport().GuiGetFocusOwner() is BaseButton button)
        {
            GetViewport().SetInputAsHandled();
            if (button.ToggleMode) button.ButtonPressed = !button.ButtonPressed;
            else button.EmitSignal(BaseButton.SignalName.Pressed);
        }
    }

    private void OnHotspot(Hotspot spot)
    {
        if (_puzzle == null || Conversation.Active) return;
        _lastFocus = spot;
        _puzzle.Use(spot, _using);
    }

    /// <summary>Play a Yarn node over the close-up; optionally step back once it ends.</summary>
    internal void Say(string node, bool closeAfter = false)
    {
        _closeAfter |= closeAfter;
        if (!Conversation.Start(node))
        {
            if (_closeAfter) Close();
            return;
        }
        HideTag();
        GetNode<Control>("%Blocker").Visible = true;
        GetViewport().GuiReleaseFocus();
    }

    private void OnConversationFinished(string node)
    {
        if (_puzzle == null) return;
        GetNode<Control>("%Blocker").Visible = false;
        if (_closeAfter)
        {
            Close();
            return;
        }
        BuildSlots(); // what he saw may have given him something
        if (_lastFocus != null && IsInstanceValid(_lastFocus)) _lastFocus.GrabFocus();
        else FocusFirst();
    }

    private IEnumerable<Hotspot> Hotspots(Node? under = null) =>
        (under ?? _puzzle!).FindChildren("*", "", true, false).OfType<Hotspot>();

    private void ShowView()
    {
        var views = _puzzle!.Views;
        for (var i = 0; i < views.Count; i++) views[i].Visible = i == _view;
        var title = $"{_puzzle.Title} · {ViewName(views[_view])}".ToUpperInvariant();
        GetNode<Label>("%Title").Text = title;
        GetNode<Label>("%Dots").Text = views.Count > 1 ? string.Join(" ", views.Select((_, i) => i == _view ? "●" : "○")) : "";
        var prev = GetNode<Button>("%Prev");
        var next = GetNode<Button>("%Next");
        prev.Visible = next.Visible = views.Count > 1;
        if (views.Count < 2) return;
        prev.Text = "‹\n" + ViewName(views[(_view + views.Count - 1) % views.Count]).ToUpperInvariant().Replace(" ", "\n");
        next.Text = "›\n" + ViewName(views[(_view + 1) % views.Count]).ToUpperInvariant().Replace(" ", "\n");
    }

    private static string ViewName(Node view) => view.Name.ToString().Capitalize();

    private void Turn(int step)
    {
        var count = _puzzle!.Views.Count;
        _view = (_view + step + count) % count;
        ShowView();
        LinkFocus();
        FocusFirst();
    }

    private void FocusFirst()
    {
        var first = Hotspots(_puzzle!.Views[_view]).FirstOrDefault(h => h.IsVisibleInTree());
        if (first != null) first.GrabFocus();
    }

    private void ShowTag(Hotspot spot)
    {
        var tag = GetNode<Label>("%Tag");
        tag.Visible = spot.Label != "";
        if (!tag.Visible) return;
        tag.Text = spot.Label;
        tag.ResetSize();
        var rect = spot.GetGlobalRect();
        var stage = GetNode<Control>("%Stage").GetGlobalRect();
        var x = rect.End.X + 12;
        if (x + tag.Size.X > stage.End.X) x = rect.Position.X - 12 - tag.Size.X;
        tag.Position = new Vector2(x, rect.GetCenter().Y - tag.Size.Y / 2);
    }

    private void HideTag() => GetNode<Label>("%Tag").Visible = false;

    private void BuildSlots()
    {
        var row = GetNode<HBoxContainer>("%Slots");
        foreach (var child in row.GetChildren())
        {
            // Out of the group first, or pressing the new slot below unpresses this one and drops _using.
            if (child is Button slot) slot.ButtonGroup = null;
            row.RemoveChild(child);
            child.QueueFree();
        }
        if (_using != null && !Game.State.HasItem(_using)) _using = null;
        foreach (var id in Game.State.Items)
        {
            var slot = ItemSlots.Create(ItemSlots.Data(id), SlotSize);
            slot.ToggleMode = true;
            slot.ButtonGroup = _slotGroup;
            slot.ButtonPressed = id == _using;
            slot.Toggled += on =>
            {
                if (on) _using = id;
                else if (_using == id) _using = null;
                ShowUsing();
            };
            row.AddChild(slot);
        }
        for (var i = Game.State.Items.Count; i < MinSlots; i++) row.AddChild(ItemSlots.Empty(SlotSize));
        ShowUsing();
        LinkFocus();
    }

    /// <summary>Down from any hotspot reaches the items, up from the items returns to the view.</summary>
    private void LinkFocus()
    {
        var slots = GetNode<HBoxContainer>("%Slots").GetChildren().OfType<Button>().ToList();
        var spots = Hotspots(_puzzle!.Views[_view]).ToList();
        if (slots.Count == 0 || spots.Count == 0) return;
        foreach (var spot in spots) spot.FocusNeighborBottom = slots[0].GetPath();
        foreach (var slot in slots) slot.FocusNeighborTop = spots[0].GetPath();
    }

    private void ShowUsing()
    {
        GetNode<Label>("%UsingName").Text = _using == null ? "Nothing" : ItemSlots.Data(_using).DisplayName;
        GetNode<Label>("%UsingHint").Text = _using == null ? "Pick an item, then a hotspot." : "Click a hotspot to use it there.";
    }

    private void ShowWarmth()
    {
        var hud = NightHud.Current;
        GetNode<Control>("%WarmthBox").Visible = hud != null;
        if (hud == null) return;
        var bar = GetNode<ProgressBar>("%Warmth");
        bar.Value = hud.Share;
        bar.ThemeTypeVariation = NightHud.BarStyle(hud.Share);
    }
}
