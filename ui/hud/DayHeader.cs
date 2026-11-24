using Godot;
using Pegasus.Core;

namespace Pegasus.UI;

/// <summary>Top-left day header from the HUD mockup: date block, slot pills, place.</summary>
public partial class DayHeader : Control
{
    private string _place = "", _spot = "";

    public string Place { get => _place; set { _place = value; if (IsNodeReady()) Rebuild(); } }
    /// <summary>Sub-place shown after the name, e.g. "Library".</summary>
    public string Spot { get => _spot; set { _spot = value; if (IsNodeReady()) Rebuild(); } }

    public override void _Ready()
    {
        Game.Changed += Rebuild;
        Rebuild();
    }

    public override void _ExitTree() => Game.Changed -= Rebuild;

    private void Rebuild()
    {
        var s = Game.State;
        var date = Clock.Date(s.Day);
        GetNode<Label>("%Date").Text = $"{date.Month}/{date.Day}";
        GetNode<Label>("%Weekday").Text = date.DayOfWeek.ToString()[..3].ToUpperInvariant();
        SetPill(GetNode<RichTextLabel>("%Afternoon"), "AFTERNOON", TimeSlot.Afternoon, s.Slot);
        SetPill(GetNode<RichTextLabel>("%Evening"), "EVENING", TimeSlot.Evening, s.Slot);
        GetNode<Label>("%Place").Text = _spot == "" ? _place : $"{_place} · {_spot}";
    }

    private static void SetPill(RichTextLabel pill, string text, TimeSlot slot, TimeSlot now)
    {
        var spent = now > slot;
        pill.Text = spent ? $"[s]{text}[/s]" : text;
        pill.ThemeTypeVariation = now == slot ? "PillNow" : spent ? "PillSpent" : "PillOpen";
    }
}
