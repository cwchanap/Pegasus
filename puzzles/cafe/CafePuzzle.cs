using System.Linq;
using Godot;
using Pegasus.Core;

namespace Pegasus.Puzzles;

/// <summary>
/// Kissa Mori, the room-escape (rules in <see cref="GlyphLock"/> and <see cref="Recipes"/>).
/// The door freezes shut behind him; the way out is the tablet's rule, "only a fire wakes
/// the sleeper". The stove's lock opens on FIRE, WAKE, SLEEP; newspaper and charcoal make
/// kindling; the shrine's matches light it. Each step that lasts is a flag or an item, so
/// a night that ends early keeps them; only the dials reset. Esc means giving in to the
/// cold: he is asked once, and sitting down ends the night.
/// </summary>
public partial class CafePuzzle : Puzzle
{
    private const string LockOpen = "cafe_stove_open";
    private const string Loaded = "cafe_stove_loaded";

    private readonly GlyphLock _lock = new();
    private Hotspot[] _dials = System.Array.Empty<Hotspot>();
    private Hotspot _firebox = null!;

    public override void _Ready()
    {
        _dials = Enumerable.Range(1, GlyphLock.DialCount).Select(i => GetNode<Hotspot>($"Stove/Dial{i}")).ToArray();
        _firebox = GetNode<Hotspot>("Stove/Firebox");
        Refresh();
    }

    public override void Opened()
    {
        if (!Solved) Say("Cafe_Enter");
    }

    public override void StepBack()
    {
        if (Solved) base.StepBack();
        else Say("Cafe_GiveIn");
    }

    public override void Use(Hotspot spot, string? item)
    {
        var dial = System.Array.IndexOf(_dials, spot);
        if (Solved || (dial < 0 && spot != _firebox) || (dial >= 0 && item != null))
            base.Use(spot, item);
        else if (dial >= 0)
            TurnDial(dial);
        else
            UseFirebox(item);
    }

    private void TurnDial(int dial)
    {
        if (Has(LockOpen))
        {
            Say("Cafe_DialsSet");
            return;
        }
        _lock.Turn(dial);
        if (_lock.Open)
        {
            Game.State.SetFlag(LockOpen);
            Say("Cafe_LockOpens");
        }
        Refresh();
    }

    private void UseFirebox(string? item)
    {
        if (!Has(LockOpen))
        {
            Say("Cafe_StoveLocked");
            return;
        }
        if (Has(Loaded))
        {
            if (item == "matchbox")
            {
                Game.State.SetFlag(SolvedFlag);
                Refresh();
                Solve("Cafe_Fire");
            }
            else Say(item == null ? "Cafe_StoveReady" : "CloseUp_Nothing");
            return;
        }
        if (item == "kindling")
        {
            Game.State.TakeItem("kindling");
            Game.State.SetFlag(Loaded);
            Refresh();
        }
        Say(item switch
        {
            null => "Cafe_StoveEmpty",
            "kindling" => "Cafe_Loaded",
            "charcoal" => "Cafe_CharcoalAlone",
            "newspaper" => "Cafe_PaperAlone",
            "matchbox" => "Cafe_NothingToBurn",
            _ => "CloseUp_Nothing",
        });
    }

    private static bool Has(string flag) => Game.State.HasFlag(flag);

    private void Refresh()
    {
        var open = Has(LockOpen);
        for (var d = 0; d < _dials.Length; d++)
        {
            var shown = open ? GlyphLock.Answer[d] : _lock.Dials[d];
            foreach (var glyph in System.Enum.GetValues<Glyph>())
                _dials[d].GetNode<CanvasItem>(glyph.ToString()).Visible = glyph == shown;
        }
        // The wall's art shows the door shut; these overlays sit over it in drawing order.
        GetNode<CanvasItem>("Stove/Open").Visible = open;
        GetNode<CanvasItem>("Stove/Kindling").Visible = Has(Loaded) && !Solved;
        GetNode<CanvasItem>("Stove/Lit").Visible = Solved;
    }
}
