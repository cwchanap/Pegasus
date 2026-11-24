using Godot;
using Pegasus.Core;

namespace Pegasus.UI;

/// <summary>Top-left day header from the HUD mockup: date block, slot pills, place.</summary>
public partial class DayHeader : Control
{
    private static readonly Color Navy = Ui.Navy;
    private static readonly Color Orange = Ui.Orange;

    private Label _date = null!, _weekday = null!, _place = null!;
    private RichTextLabel _afternoon = null!, _evening = null!;
    private string _placeName = "", _spot = "";

    public string Place { get => _placeName; set { _placeName = value; if (IsNodeReady()) Rebuild(); } }
    public string Spot { get => _spot; set { _spot = value; if (IsNodeReady()) Rebuild(); } }

    public override void _Ready()
    {
        Build();
        Game.Changed += Rebuild;
        Rebuild();
    }

    public override void _ExitTree() => Game.Changed -= Rebuild;

    private void Rebuild()
    {
        var s = Game.State;
        var date = Clock.Date(s.Day);
        _date.Text = $"{date.Month}/{date.Day}";
        _weekday.Text = date.DayOfWeek.ToString()[..3].ToUpperInvariant();
        SetPill(_afternoon, "AFTERNOON", s.Slot == TimeSlot.Afternoon, s.Slot > TimeSlot.Afternoon);
        SetPill(_evening, "EVENING", s.Slot == TimeSlot.Evening, s.Slot > TimeSlot.Evening);
        _place.Text = _spot == "" ? _placeName : $"{_placeName} · {_spot}";
    }

    private static void SetPill(RichTextLabel pill, string text, bool now, bool spent)
    {
        pill.Text = spent ? $"[s]{text}[/s]" : text;
        pill.AddThemeColorOverride("default_color", now ? Navy : new Color(1, 1, 1, spent ? 0.45f : 0.75f));
        pill.AddThemeStyleboxOverride("normal", Ui.Box(now ? Colors.White : Colors.Transparent, 16,
            border: new Color(1, 1, 1, now ? 1f : 0.35f), borderWidth: 2, padH: 14, padV: 4));
    }

    private void Build()
    {
        CustomMinimumSize = new Vector2(360, 80);
        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", 0);
        row.SetAnchorsPreset(LayoutPreset.FullRect);
        AddChild(row);

        var dateBox = new PanelContainer { CustomMinimumSize = new Vector2(104, 80) };
        dateBox.AddThemeStyleboxOverride("panel", Ui.Box(Orange, 12, corners: (12, 0, 0, 12), padH: 16, padV: 6));
        var dateCol = new VBoxContainer { Alignment = BoxContainer.AlignmentMode.Center };
        dateCol.AddThemeConstantOverride("separation", -4);
        _date = Ui.Text("", 34, Colors.White);
        _weekday = Ui.Text("", 14, Colors.White);
        dateCol.AddChild(_date);
        dateCol.AddChild(_weekday);
        dateBox.AddChild(dateCol);
        row.AddChild(dateBox);

        var info = new PanelContainer { SizeFlagsHorizontal = SizeFlags.ExpandFill };
        info.AddThemeStyleboxOverride("panel", Ui.Box(Navy, 12, corners: (0, 12, 12, 0), padH: 18, padV: 8));
        var col = new VBoxContainer();
        col.AddThemeConstantOverride("separation", 6);
        var pills = new HBoxContainer();
        pills.AddThemeConstantOverride("separation", 8);
        _afternoon = Pill();
        _evening = Pill();
        pills.AddChild(_afternoon);
        pills.AddChild(_evening);
        col.AddChild(pills);
        _place = Ui.Text("", 17, Colors.White);
        col.AddChild(_place);
        info.AddChild(col);
        row.AddChild(info);
    }

    private static RichTextLabel Pill()
    {
        var pill = new RichTextLabel { BbcodeEnabled = true, FitContent = true, AutowrapMode = TextServer.AutowrapMode.Off, ScrollActive = false };
        pill.AddThemeFontSizeOverride("normal_font_size", 13);
        return pill;
    }
}
