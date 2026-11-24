using Godot;

namespace Pegasus.Day;

/// <summary>Placeholder city map drawn in code on the 1280x720 board, until the A6 map art lands.</summary>
public partial class MapDrawing : Control
{
    private static readonly Color Block = new("e6d6b5");
    private static readonly Color Road = new("fffdf8");
    private static readonly Color Water = new("a3d4e6");
    private static readonly Color Park = new("bcd8a4");

    public override void _Draw()
    {
        DrawColoredPolygon(new[] { new Vector2(-400, 646), new Vector2(1700, 331), new Vector2(1700, 401), new Vector2(-400, 716) }, Water);
        foreach (var r in new[] { new Rect2(60, 150, 230, 150), new Rect2(330, 131, 200, 119), new Rect2(570, 170, 230, 200), new Rect2(80, 340, 210, 120), new Rect2(330, 290, 200, 150) })
            DrawStyleBox(Rounded(Block, 10), r);
        DrawStyleBox(Rounded(Park, 36), new Rect2(600, 420, 230, 120));
        DrawRect(new Rect2(40, 310, 800, 20), Road);
        DrawRect(new Rect2(300, 110, 20, 420), Road);
        DrawRect(new Rect2(540, 110, 20, 420), Road);
    }

    private static StyleBoxFlat Rounded(Color color, int radius)
    {
        var box = new StyleBoxFlat { BgColor = color };
        box.SetCornerRadiusAll(radius);
        return box;
    }
}
