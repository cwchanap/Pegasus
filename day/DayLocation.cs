using System.Collections.Generic;
using System.Linq;
using Godot;
using Pegasus.Core;
using Pegasus.UI;
using Pegasus.World;

namespace Pegasus.Day;

/// <summary>
/// Root script for a daytime place. Remembers where the player is for the map,
/// opens the map on M, feeds the HUD and hands its activity spots to the HUD panel.
/// </summary>
public partial class DayLocation : Node3D
{
    /// <summary>Where the player is in the daytime city. Runtime only, never saved.</summary>
    public static string Here { get; private set; } = "apartment";

    [Export] public LocationData? Location { get; set; }
    /// <summary>Sub-place shown after the name, e.g. "Library".</summary>
    [Export] public string Spot { get; set; } = "";
    [Export] public DayHud? Hud { get; set; }
    /// <summary>Door or path out to the city map.</summary>
    [Export] public Interactable? Exit { get; set; }
    [Export] public Music Music { get; set; } = Music.Day;

    public override void _Ready()
    {
        if (Location != null) Here = Location.Id;
        AudioService.PlayMusic(Music);
        if (Hud != null)
        {
            Hud.Header.Place = Location?.DisplayName ?? "";
            Hud.Header.Spot = Spot;
            var areas = FindChildren("*", "Area3D", owned: false);
            var spots = areas.OfType<ActivitySpot>().ToList();
            var shops = areas.OfType<ShopSpot>().ToList();
            foreach (var spot in spots)
                spot.Interacted += () => { if (spot.Activity != null) Hud.Offer(spot.Activity); };
            foreach (var spot in shops)
                spot.Interacted += () => { if (spot.Shop != null) Hud.OpenShop(spot.Shop); };
            WarnIfMapDisagrees(
                spots.Select(s => s.Activity?.Id).Concat(shops.Select(s => s.Shop?.Id)),
                (Location?.Activities.Select(a => a.Id) ?? []).Concat(Location?.Shops.Select(s => s.Id) ?? []));
        }
        if (Exit != null) Exit.Interacted += () =>
        {
            AudioService.PlaySfx(Sfx.Door);
            OpenMap();
        };
    }

    /// <summary>The map lists the place's activities and shops; the scene offers its spots. Flag drift early.</summary>
    private void WarnIfMapDisagrees(IEnumerable<string?> inScene, IEnumerable<string> onMap)
    {
        var scene = inScene.OfType<string>().ToHashSet();
        var map = onMap.ToHashSet();
        if (!scene.SetEquals(map))
            GD.PushWarning($"{Name}: map lists [{string.Join(", ", map)}] but the scene offers [{string.Join(", ", scene)}].");
    }

    public static void OpenMap() => SceneRouter.Instance.GoTo(Scenes.CityMap);

    public override void _UnhandledInput(InputEvent e)
    {
        if (!e.IsActionPressed("map") || !InputLock.IsFree) return;
        GetViewport().SetInputAsHandled();
        OpenMap();
    }
}
