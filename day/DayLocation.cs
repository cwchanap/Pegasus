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
            foreach (var spot in FindChildren("*", "Area3D", owned: false).OfType<ActivitySpot>())
                spot.Interacted += () => { if (spot.Activity != null) Hud.Offer(spot.Activity); };
        }
        if (Exit != null) Exit.Interacted += OpenMap;
    }

    public static void OpenMap() => SceneRouter.Instance.GoTo(Scenes.CityMap);

    public override void _UnhandledInput(InputEvent e)
    {
        if (!e.IsActionPressed("map") || !InputLock.IsFree) return;
        GetViewport().SetInputAsHandled();
        OpenMap();
    }
}
