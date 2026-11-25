using System.Collections.Generic;
using System.Linq;
using Godot;
using Pegasus.Core;

namespace Pegasus.UI;

/// <summary>
/// The menu's notebook column (ui/notebook/notebook.tscn): one card for each clue flag he
/// has (set by Yarn's &lt;&lt;flag clue_*&gt;&gt;), from content/clues/{flag}.tres, grouped by place.
/// </summary>
public partial class Notebook : VBoxContainer
{
    private const string Folder = "res://content/clues/";
    private static readonly Dictionary<string, ClueData?> Cache = new();

    [Export] public PackedScene? Entry { get; set; }

    public void Rebuild()
    {
        var list = GetNode<VBoxContainer>("%Entries");
        foreach (var child in list.GetChildren()) { list.RemoveChild(child); child.QueueFree(); }
        var found = Game.State.Flags.Where(f => f.StartsWith(Pegasus.Dialogue.YarnBindings.CluePrefix)).Select(Get).OfType<ClueData>().OrderBy(c => c.Order).ToList();
        GetNode<CanvasItem>("%Empty").Visible = found.Count == 0;
        foreach (var clue in found)
        {
            var card = Entry!.Instantiate<Control>();
            var when = card.GetNode<Label>("%When");
            when.Text = clue.Title.ToUpperInvariant();
            when.ThemeTypeVariation = clue.ByDay ? "ClueWhenDay" : "ClueWhen";
            card.GetNode<Label>("%Text").Text = clue.Text;
            // Focusable so arrow keys can reach and scroll the column; focus is the mockup's selected card.
            card.FocusMode = Control.FocusModeEnum.All;
            card.FocusEntered += () => card.ThemeTypeVariation = "ClueCardOn";
            card.FocusExited += () => card.ThemeTypeVariation = "ClueCard";
            card.MouseEntered += card.GrabFocus;
            list.AddChild(card);
        }
    }

    public Control? FirstCard() => GetNode<VBoxContainer>("%Entries").GetChildren().OfType<Control>().FirstOrDefault();

    private static ClueData? Get(string flag)
    {
        if (Cache.TryGetValue(flag, out var cached)) return cached;
        var path = $"{Folder}{flag}.tres";
        var clue = ResourceLoader.Exists(path) ? GD.Load<ClueData>(path) : null;
        if (clue == null) GD.PushWarning($"Notebook: no entry for '{flag}' (expected {path}).");
        return Cache[flag] = clue;
    }
}
