using System.Linq;
using Godot;
using Godot.Collections;
using Pegasus.Core;
using Pegasus.UI;

namespace Pegasus.Day;

/// <summary>
/// The city map from the mockup. Hover or arrow keys select a place, the panel says what
/// it is and what can be done there, E / Enter / click goes there. Moving is free.
/// Esc or M returns to where he was. The map sits on a 1280x720 board centred in the window.
/// </summary>
public partial class CityMap : Control
{
    [Export] public Array<LocationData> Locations { get; set; } = new();

    private readonly System.Collections.Generic.Dictionary<Button, LocationData> _markers = new();
    private LocationData? _selected;

    private Control Board => GetNode<Control>("Board");

    [Export] public Music Music { get; set; } = Music.Day;

    public override void _Ready()
    {
        AudioService.PlayMusic(Music);
        GetNode<DayHeader>("Header").Place = "City map";
        foreach (var location in Locations) AddMarker(location);
        GetNode<Button>("%Go").Pressed += () => { if (_selected != null) Travel(_selected); };
        var start = _markers.FirstOrDefault(m => m.Value.Id == DayLocation.Here).Key ?? _markers.Keys.First();
        start.GrabFocus();
    }

    public override void _UnhandledInput(InputEvent e)
    {
        if (!InputLock.IsFree) return;
        if (e.IsActionPressed("pause") || e.IsActionPressed("map") || e.IsActionPressed("ui_cancel"))
        {
            GetViewport().SetInputAsHandled();
            Travel(Locations.FirstOrDefault(l => l.Id == DayLocation.Here) ?? Locations.First());
        }
        else if (e.IsActionPressed("interact") && _selected != null)
        {
            GetViewport().SetInputAsHandled();
            Travel(_selected);
        }
    }

    private static void Travel(LocationData location) => SceneRouter.Instance.GoTo(location.ScenePath);

    private void Select(LocationData location)
    {
        _selected = location;
        GetNode<Label>("%Title").Text = location.DisplayName;
        GetNode<Label>("%Blurb").Text = location.Blurb;
        GetNode<Label>("%GoText").Text = location.Id == DayLocation.Here ? "Go back" : "Go there";
        ListActivities(location);
        foreach (var (button, data) in _markers)
            button.ThemeTypeVariation = data == location ? "MapMarkerSelected" : "MapMarker";
    }

    private void ListActivities(LocationData location)
    {
        var list = GetNode<VBoxContainer>("%Activities");
        foreach (var child in list.GetChildren()) child.QueueFree();
        list.Visible = location.Activities.Count + location.Shops.Count > 0;
        foreach (var activity in location.Activities)
        {
            var person = activity.With;
            list.AddChild(person == null
                ? Row("1 SLOT", "TagOrange", activity.Title, activity.Describe(Game.State))
                : Row("HERE TODAY", "TagPurple", person.DisplayName,
                    $"Hang out · spends a slot · Bond {Game.State.GetBond(person.Id)} of {GameState.MaxBond}"));
        }
        foreach (var shop in location.Shops)
            list.AddChild(Row("FREE", "TagTeal", shop.Title,
                string.Join(" · ", shop.Stock.Select(i => $"{i.DisplayName} {Format.Yen(i.Price)}"))));
    }

    private static VBoxContainer Row(string tag, string tagStyle, string title, string detail)
    {
        var row = new VBoxContainer();
        row.AddThemeConstantOverride("separation", 2);
        var head = new HBoxContainer();
        head.AddThemeConstantOverride("separation", 10);
        head.AddChild(new Label { Text = tag, ThemeTypeVariation = tagStyle, SizeFlagsVertical = SizeFlags.ShrinkCenter });
        head.AddChild(new Label { Text = title, ThemeTypeVariation = "DarkTitle" });
        row.AddChild(head);
        row.AddChild(new Label { Text = detail, ThemeTypeVariation = "MutedLabel" });
        return row;
    }

    private void AddMarker(LocationData location)
    {
        var button = new Button
        {
            Text = location.Letter,
            ThemeTypeVariation = "MapMarker",
            Size = new Vector2(52, 52),
            Position = location.MapPosition - new Vector2(26, 26),
        };
        button.FocusEntered += () => Select(location);
        button.MouseEntered += button.GrabFocus;
        button.Pressed += () => Travel(location);
        Board.AddChild(button);
        _markers[button] = location;

        var name = new Label
        {
            Text = location.DisplayName,
            ThemeTypeVariation = "TagNavy",
            HorizontalAlignment = HorizontalAlignment.Center,
            Position = location.MapPosition + new Vector2(-70, 32),
            CustomMinimumSize = new Vector2(140, 0),
        };
        Board.AddChild(name);
        if (location.Id == DayLocation.Here)
        {
            var here = new Label
            {
                Text = "YOU ARE HERE",
                ThemeTypeVariation = "TagTeal",
                HorizontalAlignment = HorizontalAlignment.Center,
                Position = location.MapPosition + new Vector2(-60, -62),
                CustomMinimumSize = new Vector2(120, 0),
            };
            Board.AddChild(here);
        }
    }
}
