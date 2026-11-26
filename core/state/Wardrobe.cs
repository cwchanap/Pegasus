namespace Pegasus.Core;

public enum Outfit { Summer, Coat }

/// <summary>
/// What he wears: summer clothes by day, and at night the down coat once he owns it.
/// </summary>
public static class Wardrobe
{
    public const string CoatItem = "down_coat";

    public static Outfit Wears(GameState s) =>
        s.Slot == TimeSlot.Night && s.HasItem(CoatItem) ? Outfit.Coat : Outfit.Summer;
}
