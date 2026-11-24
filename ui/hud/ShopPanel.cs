using System.Linq;
using Godot;
using Pegasus.Core;
using Pegasus.Day;
using Pegasus.World;

namespace Pegasus.UI;

/// <summary>
/// A shop counter: one row per item with what it does and its price. Up/down picks,
/// E or click buys, Esc leaves. Browsing and buying are free of time.
/// </summary>
public partial class ShopPanel : Control
{
    private ShopData? _shop;

    public override void _Ready() => Visible = false;

    public void Open(ShopData shop)
    {
        if (Visible || !InputLock.IsFree) return;
        _shop = shop;
        GetNode<Label>("%Title").Text = shop.Title;
        Visible = true;
        InputLock.Acquire(nameof(ShopPanel));
        Refresh();
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (!Visible) return;
        if (e.IsActionPressed("pause") || e.IsActionPressed("ui_cancel"))
        {
            GetViewport().SetInputAsHandled();
            Close();
        }
        else if (e.IsActionPressed("interact") && GetViewport().GuiGetFocusOwner() is Button { Disabled: false } row)
        {
            GetViewport().SetInputAsHandled();
            row.EmitSignal(BaseButton.SignalName.Pressed);
        }
    }

    private void Buy(ItemData item)
    {
        if (!Game.State.TryBuy(item.Id, item.Price)) return;
        Toast.Show($"Bought: {item.DisplayName}");
        Refresh();
    }

    private void Refresh()
    {
        var s = Game.State;
        GetNode<Label>("%Money").Text = Format.Yen(s.Money);
        var rows = GetNode<VBoxContainer>("%Rows");
        foreach (var child in rows.GetChildren()) { rows.RemoveChild(child); child.QueueFree(); }
        foreach (var item in _shop!.Stock) rows.AddChild(Row(item, s));
        rows.GetChildren().OfType<Button>().FirstOrDefault(b => !b.Disabled)?.GrabFocus();
    }

    private Button Row(ItemData item, GameState s)
    {
        var owned = s.HasItem(item.Id);
        var button = new Button
        {
            ThemeTypeVariation = "ShopRow",
            CustomMinimumSize = new Vector2(0, 76),
            Disabled = owned || s.Money < item.Price,
        };
        button.Pressed += () => Buy(item);
        button.MouseEntered += () => { if (!button.Disabled) button.GrabFocus(); };

        var margin = new MarginContainer { MouseFilter = MouseFilterEnum.Ignore };
        margin.SetAnchorsPreset(LayoutPreset.FullRect);
        foreach (var side in new[] { "left", "right" }) margin.AddThemeConstantOverride($"margin_{side}", 16);
        button.AddChild(margin);
        var line = new HBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
        line.AddThemeConstantOverride("separation", 16);
        margin.AddChild(line);

        var text = new VBoxContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill, Alignment = BoxContainer.AlignmentMode.Center, MouseFilter = MouseFilterEnum.Ignore };
        text.AddThemeConstantOverride("separation", 2);
        var head = new HBoxContainer { MouseFilter = MouseFilterEnum.Ignore };
        head.AddThemeConstantOverride("separation", 10);
        head.AddChild(new Label { Text = item.DisplayName, ThemeTypeVariation = "DarkTitle" });
        if (item.Warmth > 0)
            head.AddChild(new Label { Text = $"WARMTH +{item.Warmth}", ThemeTypeVariation = "TagTeal", SizeFlagsVertical = SizeFlags.ShrinkCenter });
        text.AddChild(head);
        text.AddChild(new Label
        {
            Text = item.Description,
            ThemeTypeVariation = "MutedLabel",
            TextOverrunBehavior = TextServer.OverrunBehavior.TrimEllipsis,
            CustomMinimumSize = new Vector2(420, 0),
        });
        line.AddChild(text);

        var price = owned ? "Owned" : s.Money < item.Price ? $"Need {Format.Yen(item.Price)}" : Format.Yen(item.Price);
        line.AddChild(new Label
        {
            Text = price,
            ThemeTypeVariation = owned || s.Money < item.Price ? "MutedLabel" : "SpendLabel",
            VerticalAlignment = VerticalAlignment.Center,
            SizeFlagsVertical = SizeFlags.ShrinkCenter,
        });
        return button;
    }

    private void Close()
    {
        Visible = false;
        _shop = null;
        Callable.From(() => InputLock.Release(nameof(ShopPanel))).CallDeferred();
    }
}
