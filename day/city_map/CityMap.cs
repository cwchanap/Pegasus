using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using Pegasus.Core;
using Pegasus.UI;

namespace Pegasus.Day;

/// <summary>
/// The city map from the mockup. Hover or arrow keys select a place, the panel says what
/// it is, Enter / E / click goes there. Moving is free. Esc returns to where he was.
/// </summary>
public partial class CityMap : Control
{
    private static readonly Color Paper = new("f2e6d0");
    private static readonly Color Block = new("e6d6b5");
    private static readonly Color Road = new("fffdf8");
    private static readonly Color Water = new("a3d4e6");
    private static readonly Color Park = new("bcd8a4");

    [Export] public Array<LocationData> Locations { get; set; } = new();

    private readonly System.Collections.Generic.Dictionary<Button, LocationData> _markers = new();
    private Label _panelTitle = null!, _panelBlurb = null!;
    private Button _go = null!;
    private LocationData? _selected;

    public override void _Ready()
    {
        SetAnchorsPreset(LayoutPreset.FullRect);
        var header = new DayHeader { Position = new Vector2(28, 28), Place = "City map" };
        AddChild(header);
        foreach (var location in Locations) AddMarker(location);
        BuildPanel();
        BuildTip();
        var start = _markers.FirstOrDefault(m => m.Value.Id == DayLocation.Here).Key ?? _markers.Keys.First();
        start.GrabFocus();
    }

    public override void _Draw()
    {
        DrawRect(new Rect2(Vector2.Zero, Size), Paper);
        DrawColoredPolygon(new[] { new Vector2(0, 586), new Vector2(918, 448), new Vector2(918, 512), new Vector2(0, 656) }, Water);
        foreach (var r in new[] { new Rect2(60, 150, 230, 150), new Rect2(330, 131, 200, 119), new Rect2(570, 170, 230, 200), new Rect2(80, 340, 210, 120), new Rect2(330, 290, 200, 150) })
            DrawStyleBox(Ui.Box(Block, 10), r);
        DrawStyleBox(Ui.Box(Park, 36), new Rect2(600, 420, 230, 120));
        DrawRect(new Rect2(40, 310, 800, 20), Road);
        DrawRect(new Rect2(300, 110, 20, 420), Road);
        DrawRect(new Rect2(540, 110, 20, 420), Road);
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (!InputLock.IsFree) return;
        if (e.IsActionPressed("pause") || e.IsActionPressed("map") || e.IsActionPressed("ui_cancel"))
        {
            GetViewport().SetInputAsHandled();
            var here = Locations.FirstOrDefault(l => l.Id == DayLocation.Here) ?? Locations.First();
            Travel(here);
        }
        else if (e.IsActionPressed("interact") && _selected != null)
        {
            GetViewport().SetInputAsHandled();
            Travel(_selected);
        }
    }

    private void Travel(LocationData location) => SceneRouter.Instance.GoTo(location.ScenePath);

    private void Select(LocationData location)
    {
        _selected = location;
        _panelTitle.Text = location.DisplayName;
        _panelBlurb.Text = location.Blurb;
        _go.Text = location.Id == DayLocation.Here ? "Go back" : "Go there";
        foreach (var (button, data) in _markers) Style(button, data == location);
    }

    private void AddMarker(LocationData location)
    {
        var button = new Button { Text = location.Letter, Size = new Vector2(52, 52), Position = location.MapPosition - new Vector2(26, 26) };
        button.AddThemeFontSizeOverride("font_size", 22);
        Style(button, false);
        button.FocusEntered += () => Select(location);
        button.MouseEntered += button.GrabFocus;
        button.Pressed += () => Travel(location);
        AddChild(button);
        _markers[button] = location;

        var name = Ui.Tag(location.DisplayName, Ui.Navy, Colors.White, 16);
        name.Position = location.MapPosition + new Vector2(-60, 32);
        name.CustomMinimumSize = new Vector2(120, 0);
        AddChild(name);
        if (location.Id == DayLocation.Here)
        {
            var here = Ui.Tag("YOU ARE HERE", Ui.Teal, Colors.White, 12);
            here.Position = location.MapPosition + new Vector2(-52, -60);
            AddChild(here);
        }
    }

    private static void Style(Button button, bool selected)
    {
        var bg = selected ? Ui.Orange : Ui.Navy;
        var box = Ui.Box(bg, 26, border: selected ? new Color(1, 0.35f, 0.24f, 0.45f) : Colors.White, borderWidth: selected ? 6 : 3);
        foreach (var state in new[] { "normal", "hover", "pressed", "focus" })
            button.AddThemeStyleboxOverride(state, box);
        button.AddThemeColorOverride("font_color", Colors.White);
        button.AddThemeColorOverride("font_focus_color", Colors.White);
        button.AddThemeColorOverride("font_hover_color", Colors.White);
    }

    private void BuildPanel()
    {
        var panel = new PanelContainer { Position = new Vector2(880, 150), CustomMinimumSize = new Vector2(370, 0) };
        panel.AddThemeStyleboxOverride("panel", Ui.Box(Ui.Cream, 14, border: Ui.Navy, borderWidth: 3));
        var col = new VBoxContainer();
        col.AddThemeConstantOverride("separation", 0);
        panel.AddChild(col);

        var head = new PanelContainer();
        head.AddThemeStyleboxOverride("panel", Ui.Box(Ui.Navy, 12, corners: (12, 12, 0, 0), padH: 22, padV: 14));
        var headCol = new VBoxContainer();
        headCol.AddChild(Ui.Text("SELECTED", 12, new Color(1, 1, 1, 0.6f)));
        _panelTitle = Ui.Text("", 24, Colors.White);
        headCol.AddChild(_panelTitle);
        head.AddChild(headCol);
        col.AddChild(head);

        var body = new MarginContainer();
        foreach (var side in new[] { "left", "right", "top", "bottom" }) body.AddThemeConstantOverride($"margin_{side}", 22);
        var bodyCol = new VBoxContainer();
        bodyCol.AddThemeConstantOverride("separation", 18);
        _panelBlurb = Ui.Text("", 16, Ui.Navy);
        _panelBlurb.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        _panelBlurb.CustomMinimumSize = new Vector2(320, 0);
        bodyCol.AddChild(_panelBlurb);
        _go = new Button { FocusMode = FocusModeEnum.None, CustomMinimumSize = new Vector2(0, 46) };
        _go.AddThemeFontSizeOverride("font_size", 19);
        foreach (var state in new[] { "normal", "hover", "pressed" })
            _go.AddThemeStyleboxOverride(state, Ui.Box(Ui.Orange, 10, border: Ui.Navy, borderWidth: 2));
        _go.Pressed += () => { if (_selected != null) Travel(_selected); };
        bodyCol.AddChild(_go);
        body.AddChild(bodyCol);
        col.AddChild(body);
        AddChild(panel);
    }

    private void BuildTip()
    {
        var tip = Ui.Tag("Moving between places is free. Activities spend a slot.", new Color("3a3b4d"), Colors.White, 15);
        tip.Position = new Vector2(450, 662);
        AddChild(tip);
    }
}
