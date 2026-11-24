using Godot;
using Pegasus.Day;

namespace Pegasus.UI;

/// <summary>
/// The daytime HUD, instanced once in every day place: header, stats, key hints,
/// the interact prompt, the activity confirm panel and the shop panel.
/// </summary>
public partial class DayHud : CanvasLayer
{
    public DayHeader Header => GetNode<DayHeader>("Header");

    public void Offer(ActivityData activity) => GetNode<ActivityPanel>("ActivityPanel").Offer(activity);

    public void OpenShop(ShopData shop) => GetNode<ShopPanel>("ShopPanel").Open(shop);
}
