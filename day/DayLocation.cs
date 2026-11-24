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

    public override void _Ready()
    {
        if (Location != null) Here = Location.Id;
        if (Hud != null)
        {
            Hud.Header.Place = Location?.DisplayName ?? "";
            Hud.Header.Spot = Spot;
            var spots = FindChildren("*", "Area3D", owned: false).OfType<ActivitySpot>().ToList();
            foreach (var spot in spots)
                spot.Interacted += () => { if (spot.Activity != null) Hud.Offer(spot.Activity); };
            WarnIfMapDisagrees(spots);
        }
        if (Exit != null) Exit.Interacted += OpenMap;
    }

    /// <summary>The map lists LocationData.Activities; the scene offers its spots. Flag drift early.</summary>
    private void WarnIfMapDisagrees(List<ActivitySpot> spots)
    {
        var inScene = spots.Select(s => s.Activity?.Id).OfType<string>().ToHashSet();
        var onMap = (Location?.Activities ?? new()).Select(a => a.Id).ToHashSet();
        if (!inScene.SetEquals(onMap))
            GD.PushWarning($"{Name}: map lists [{string.Join(", ", onMap)}] but the scene offers [{string.Join(", ", inScene)}].");
    }

    public static void OpenMap() => SceneRouter.Instance.GoTo(Scenes.CityMap);

    public override void _UnhandledInput(InputEvent e)
    {
        if (!e.IsActionPressed("map") || !InputLock.IsFree) return;
        GetViewport().SetInputAsHandled();
        OpenMap();
    }
}
