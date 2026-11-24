using Godot;
using Pegasus.World;

namespace Pegasus.UI;

/// <summary>Item slot buttons (icon over name) shared by the Tab menu and the close-up inventory bar.</summary>
public static class ItemSlots
{
    /// <summary>Item data for an owned id, or a stand-in named after the id if its .tres is missing.</summary>
    public static ItemData Data(string id) => Items.Get(id) ?? new ItemData { Id = id, DisplayName = id };

    public static Button Create(ItemData item, Vector2 size)
    {
        var slot = new Button { ThemeTypeVariation = "ItemSlot", CustomMinimumSize = size };
        var col = new VBoxContainer { Alignment = BoxContainer.AlignmentMode.Center, MouseFilter = Control.MouseFilterEnum.Ignore };
        col.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        col.AddThemeConstantOverride("separation", 8);
        Control icon = item.Icon != null
            ? new TextureRect { Texture = item.Icon, ExpandMode = TextureRect.ExpandModeEnum.IgnoreSize, StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered }
            : new ColorRect { Color = item.IconColor };
        icon.CustomMinimumSize = new Vector2(40, 40);
        icon.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
        icon.MouseFilter = Control.MouseFilterEnum.Ignore;
        col.AddChild(icon);
        col.AddChild(new Label { Text = item.DisplayName, HorizontalAlignment = HorizontalAlignment.Center, ThemeTypeVariation = "MenuMuted", MouseFilter = Control.MouseFilterEnum.Ignore });
        slot.AddChild(col);
        slot.MouseEntered += slot.GrabFocus;
        return slot;
    }

    public static Panel Empty(Vector2 size) => new() { ThemeTypeVariation = "ItemSlotEmpty", CustomMinimumSize = size };
}
