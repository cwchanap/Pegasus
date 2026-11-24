using System.Globalization;
using System.Linq;
using Godot;
using Pegasus.Core;
using Pegasus.Dialogue;
using Pegasus.World;

namespace Pegasus.UI;

/// <summary>
/// Autoload (ui/menu/menu.tscn). Tab opens one screen with three columns, per the menu
/// mockup: status, items and notebook. It opens only where the player is and only while
/// the input lock is free, so never over a conversation, a panel or a scene fade. While
/// open it holds the lock and pauses the tree, so nothing timed (night Warmth) runs.
/// </summary>
public partial class Menu : CanvasLayer
{
    private const int SlotsPerRow = 4;

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

    private void BuildItems(GameState s)
    {
        var grid = Clear(GetNode<GridContainer>("%Slots"));
        var owned = s.Items.Select(id => Items.Get(id) ?? new ItemData { Id = id, DisplayName = id }).ToList();
        foreach (var item in owned) grid.AddChild(Slot(item));
        var slots = System.Math.Max(SlotsPerRow * 2, (owned.Count + SlotsPerRow - 1) / SlotsPerRow * SlotsPerRow);
        for (var i = owned.Count; i < slots; i++)
            grid.AddChild(new Panel { ThemeTypeVariation = "ItemSlotEmpty", CustomMinimumSize = SlotSize });
        ShowDetail(owned.FirstOrDefault());
    }

    private static readonly Vector2 SlotSize = new(90, 92);

    private Button Slot(ItemData item)
    {
        var slot = new Button { ThemeTypeVariation = "ItemSlot", CustomMinimumSize = SlotSize };
        var col = new VBoxContainer { Alignment = BoxContainer.AlignmentMode.Center, MouseFilter = Control.MouseFilterEnum.Ignore };
        col.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        col.AddThemeConstantOverride("separation", 8);
        Control icon = item.Icon != null
            ? new TextureRect { Texture = item.Icon, ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize, StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered }
            : new ColorRect { Color = item.IconColor };
        icon.CustomMinimumSize = new Vector2(44, 44);
        icon.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
        icon.MouseFilter = Control.MouseFilterEnum.Ignore;
        col.AddChild(icon);
        col.AddChild(new Label { Text = item.DisplayName, HorizontalAlignment = HorizontalAlignment.Center, ThemeTypeVariation = "MenuMuted", MouseFilter = Control.MouseFilterEnum.Ignore });
        slot.AddChild(col);
        slot.FocusEntered += () => ShowDetail(item);
        slot.MouseEntered += slot.GrabFocus;
        return slot;
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
