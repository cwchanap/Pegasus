using Godot;

namespace Pegasus.Day;

/// <summary>A place on the city map. One .tres per place under content/locations/.</summary>
[GlobalClass]
public partial class LocationData : Resource
{
    [Export] public string Id { get; set; } = "";
    [Export] public string DisplayName { get; set; } = "";
    /// <summary>Letter on the map marker.</summary>
    [Export] public string Letter { get; set; } = "";
    /// <summary>Marker position on the 1280x720 map.</summary>
    [Export] public Vector2 MapPosition { get; set; }
    [Export(PropertyHint.File, "*.tscn")] public string ScenePath { get; set; } = "";
    [Export(PropertyHint.MultilineText)] public string Blurb { get; set; } = "";
}
