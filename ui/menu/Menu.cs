using System.Globalization;
using System.Linq;
using Godot;
using Pegasus.Core;
using Pegasus.Dialogue;
using Pegasus.World;

namespace Pegasus.UI;

/// <summary>
/// Autoload (ui/menu/menu.tscn). Tab opens one screen with three columns, per the menu
/// mockup: status, items and the notebook (ui/notebook). It opens only where the player is and only while
/// the input lock is free, so never over a conversation, a panel or a scene fade. While
/// open it holds the lock and pauses the tree, so nothing timed (night Warmth) runs.
/// </summary>
public partial class Menu : CanvasLayer
{
    private const int SlotsPerRow = 4;

    private bool _noted;

    public override void _Ready()
    {
        // A new clue says so once its conversation ends, so the toast doesn't cover the box.
        YarnBindings.ClueNoted += _ => _noted = true;
        Conversation.Finished += _ =>
        {
            if (!_noted) return;
            _noted = false;
            Toast.Show("Notebook updated");
        };
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (Visible || !e.IsActionPressed("menu") || !InputLock.IsFree || GetTree().GetFirstNodeInGroup("player") == null) return;
        GetViewport().SetInputAsHandled();
        Open();
    }

    // _Input, not _UnhandledInput: a focused item slot would otherwise eat Tab as "next focus".
    public override void _Input(InputEvent e)
    {
        if (!Visible || !(e.IsActionPressed("menu") || e.IsActionPressed("pause") || e.IsActionPressed("ui_cancel"))) return;
        GetViewport().SetInputAsHandled();
        Close();
    }

    private void Open()
    {
        InputLock.Acquire(nameof(Menu));
        GetTree().Paused = true;
        Rebuild();
        Visible = true;
        GetNode<GridContainer>("%Slots").GetChildren().OfType<Button>().FirstOrDefault()?.GrabFocus();
    }

    private void Close()
    {
        Visible = false;
        GetViewport().GuiReleaseFocus();
        GetTree().Paused = false;
        Callable.From(() => InputLock.Release(nameof(Menu))).CallDeferred();
    }

    private void Rebuild()
    {
        var s = Game.State;
        GetNode<Label>("%Title").Text = Clock.Describe(s);
        GetNode<Label>("%Date").Text = Clock.Date(s.Day).ToString("dddd, MMMM d", CultureInfo.InvariantCulture);
        GetNode<Label>("%Warmth").Text = Items.MaxWarmth().ToString();
        var why = Warmth.Describe(s, Items.WarmthOf);
        GetNode<Label>("%WarmthWhy").Text = why == "" ? "No winter gear yet" : why;
        BuildBonds(s);
        BuildItems(s);
        GetNode<Notebook>("%Notes").Rebuild();
    }

    private void BuildBonds(GameState s)
    {
        var list = Clear(GetNode<VBoxContainer>("%Bonds"));
        foreach (var who in Conversation.Instance.Characters.Where(c => c.HasBond))
        {
            var row = new HBoxContainer();
            var names = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
            names.AddThemeConstantOverride("separation", 0);
            names.AddChild(new Label { Text = who.DisplayName, ThemeTypeVariation = "BodyText" });
            names.AddChild(new Label { Text = who.Subtitle, ThemeTypeVariation = "MenuMuted" });
            row.AddChild(names);
            var bond = s.GetBond(who.Id);
            row.AddChild(new Label
            {
                Text = new string('◆', bond) + new string('◇', GameState.MaxBond - bond),
                ThemeTypeVariation = "BondPips",
                VerticalAlignment = VerticalAlignment.Center,
            });
            list.AddChild(row);
        }
    }

    private static readonly Vector2 SlotSize = new(90, 92);

    private void BuildItems(GameState s)
    {
        var grid = Clear(GetNode<GridContainer>("%Slots"));
        var owned = s.Items.Select(ItemSlots.Data).ToList();
        foreach (var item in owned)
        {
            var slot = ItemSlots.Create(item, SlotSize);
            slot.FocusEntered += () => ShowDetail(item);
            grid.AddChild(slot);
        }
        var slots = System.Math.Max(SlotsPerRow * 2, (owned.Count + SlotsPerRow - 1) / SlotsPerRow * SlotsPerRow);
        for (var i = owned.Count; i < slots; i++) grid.AddChild(ItemSlots.Empty(SlotSize));
        ShowDetail(owned.FirstOrDefault());
    }

    private void ShowDetail(ItemData? item)
    {
        GetNode<Label>("%DetailName").Text = item?.DisplayName ?? "Empty pockets";
        GetNode<Label>("%Worn").Visible = item is { Warmth: > 0 };
        GetNode<Label>("%Worn").Text = $"WORN AT NIGHT · WARMTH +{item?.Warmth}";
        GetNode<Label>("%DetailText").Text = item?.Description ?? "Things you buy or are given show up here.";
    }

    private static T Clear<T>(T container) where T : Node
    {
        foreach (var child in container.GetChildren()) { container.RemoveChild(child); child.QueueFree(); }
        return container;
    }
}
