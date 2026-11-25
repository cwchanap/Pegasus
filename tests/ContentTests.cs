using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Pegasus.Conditions;
using Xunit;

namespace Pegasus.Tests;

/// <summary>
/// Checks the content files against each other, as text: every condition parses and names
/// real items, every item given exists, every Yarn node a scene or script plays exists, and
/// every "Name:" line is a known character. These typos otherwise only show up in play.
/// </summary>
public class ContentTests
{
    private static readonly string Root = FindRoot();
    private static readonly string[] Skipped = { "addons", ".godot", "tests", "bin", "obj" };

    private static string FindRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir != null && !File.Exists(Path.Combine(dir.FullName, "project.godot"))) dir = dir.Parent;
        return dir?.FullName ?? throw new InvalidOperationException("No project.godot above the test output.");
    }

    private static IEnumerable<(string File, string Text)> Files(params string[] extensions) =>
        Directory.EnumerateFiles(Root, "*", SearchOption.AllDirectories)
            .Where(f => extensions.Contains(Path.GetExtension(f)))
            .Select(f => Path.GetRelativePath(Root, f))
            .Where(f => !f.Split(Path.DirectorySeparatorChar).Any(Skipped.Contains))
            .Select(f => (f, File.ReadAllText(Path.Combine(Root, f))));

    private static List<(string File, string Value)> Find(string pattern, params string[] extensions) =>
        Files(extensions)
            .SelectMany(f => Regex.Matches(f.Text, pattern, RegexOptions.Multiline).Select(m => (f.File, m.Groups[1].Value)))
            .ToList();

    private static HashSet<string> Ids(string folder) =>
        Directory.EnumerateFiles(Path.Combine(Root, folder), "*.tres").Select(f => Path.GetFileNameWithoutExtension(f)).ToHashSet();

    private static HashSet<string> YarnNodes() => Find(@"^title:\s*(\S+)", ".yarn").Select(m => m.Value).ToHashSet();

    [Fact]
    public void Conditions_parse_and_name_real_items()
    {
        var items = Ids("content/items");
        var conditions = Find(@"met\(""([^""]*)""\)", ".yarn")
            .Concat(Find(@"#needs:(\S+)", ".yarn"))
            .Concat(Find(@"^(?:Requires|Condition|OpenWhen) = ""([^""]*)""", ".tscn", ".tres"))
            .ToList();
        var bad = new List<string>();
        foreach (var (file, text) in conditions)
        {
            try { Condition.Parse(text); }
            catch (ConditionSyntaxException e) { bad.Add($"{file}: {e.Message}"); continue; }
            bad.AddRange(Regex.Matches(text, @"item:(\w+)").Select(m => m.Groups[1].Value)
                .Where(id => !items.Contains(id)).Select(id => $"{file}: no item '{id}' for \"{text}\""));
        }
        Assert.NotEmpty(conditions);
        Assert.Empty(bad);
    }

    [Fact]
    public void Flags_in_conditions_are_set_somewhere()
    {
        // A typo such as "flag:lanterns_lti" parses fine but leaves a gate shut forever.
        var set = Find(@"^SolvedFlag = ""(\w+)""", ".tscn")
            .Concat(Find(@"<<flag\s+(\w+)\s*>>", ".yarn"))
            .Concat(Find(@"SetFlag\(""(\w+)""", ".cs"))
            .Select(m => m.Value).ToHashSet();
        // A constant counts only when it is what SetFlag gets (Flags.Ended, a puzzle's LockOpen), not an item id.
        var setNames = Find(@"SetFlag\((?:\w+\.)?([A-Z]\w*)\)", ".cs").Select(m => m.Value).ToHashSet();
        set.UnionWith(Find(@"const string (\w+ = ""\w+"")", ".cs").Select(m => m.Value.Split(" = "))
            .Where(p => setNames.Contains(p[0])).Select(p => p[1].Trim('"')));
        var used = Find(@"met\(""([^""]*)""\)", ".yarn")
            .Concat(Find(@"#needs:(\S+)", ".yarn"))
            .Concat(Find(@"^(?:Requires|Condition|OpenWhen) = ""([^""]*)""", ".tscn", ".tres"))
            .SelectMany(c => Regex.Matches(c.Value, @"flag:(\w+)").Select(m => (c.File, Flag: m.Groups[1].Value)))
            .ToList();
        Assert.NotEmpty(used);
        Assert.Empty(used.Where(u => !set.Contains(u.Flag)).Select(u => $"{u.File}: nothing sets flag '{u.Flag}'"));
    }

    [Fact]
    public void The_tablet_opens_with_either_key()
    {
        var tablet = Find(@"^Requires = ""([^""]*)""", ".tscn").Where(m => m.File.Contains("tablet")).ToList();
        // Every sign must be gated: one without Requires would be readable from the start.
        Assert.Equal(Find(@"^\[node name=""(Sign\w+)"" type=""Button""", ".tscn").Count(m => m.File.Contains("tablet")), tablet.Count);
        var conditions = tablet.Select(m => m.Value).Distinct().ToList();
        Assert.NotEmpty(conditions);
        var scholar = new Pegasus.Core.GameState();
        scholar.AddStat(Pegasus.Core.Stat.Knowledge, 2);
        var friend = new Pegasus.Core.GameState();
        friend.GiveItem("aoi_notes");
        foreach (var c in conditions)
        {
            Assert.True(Condition.Check(c, scholar), $"Knowledge 2 should read \"{c}\"");
            Assert.True(Condition.Check(c, friend), $"Aoi's notes should read \"{c}\"");
            Assert.False(Condition.Check(c, new Pegasus.Core.GameState()), $"\"{c}\" should stay locked at the start");
        }
    }

    [Fact]
    public void Items_named_in_scripts_exist()
    {
        var items = Ids("content/items");
        var named = Find(@"<<(?:give|take)\s+(\w+)\s*>>", ".yarn")
            .Concat(Find(@"(?:HasItem|Items\.Get)\(""(\w+)""\)", ".cs"))
            .ToList();
        Assert.NotEmpty(named);
        Assert.Empty(named.Where(n => !items.Contains(n.Value)).Select(n => $"{n.File}: no item '{n.Value}'"));
    }

    [Fact]
    public void Recipes_use_real_items_and_have_a_line()
    {
        var items = Ids("content/items");
        var nodes = YarnNodes();
        var bad = Pegasus.Puzzles.Recipes.All
            .SelectMany(r => new[] { r.A, r.B, r.Result }.Where(id => !items.Contains(id)).Select(id => $"no item '{id}'")
                .Concat(nodes.Contains($"Made_{r.Result}") ? Array.Empty<string>() : new[] { $"no Yarn node 'Made_{r.Result}'" }))
            .ToList();
        Assert.Empty(bad);
    }

    [Fact]
    public void Yarn_nodes_that_are_played_exist()
    {
        var nodes = YarnNodes();
        var played = Find(@"^(?:YarnNode|LockedYarnNode) = ""([^""]+)""", ".tscn", ".tres")
            .Concat(Find(@"^YarnScene = ""([^""]+)""", ".tscn", ".tres").SelectMany(m => new[] { "1", "2", "3", "Again" }.Select(n => (m.File, Value: m.Value + n))))
            .Concat(Find(@"Conversation\.Start\(""([^""]+)""", ".cs"))
            .Concat(Find(@"(?:Say|Solve)\(""([^""]+)""", ".cs").Where(m => m.File.StartsWith("puzzles")))
            .Concat(Find(@"Node = ""([^""]+)"";", ".cs"))
            .Concat(Find(@"<<jump\s+(\w+)\s*>>", ".yarn"))
            .ToList();
        Assert.NotEmpty(played);
        Assert.Empty(played.Where(p => !nodes.Contains(p.Value)).Select(p => $"{p.File}: no Yarn node '{p.Value}'"));
    }

    [Fact]
    public void Every_requirement_has_a_locked_line()
    {
        // Without one, a locked spot does nothing but log an error.
        var bad = Files(".tscn", ".tres")
            .SelectMany(f => Regex.Split(f.Text, @"^(?=\[node )", RegexOptions.Multiline).Select(node => (f.File, Node: node)))
            .Where(n => Regex.IsMatch(n.Node, @"^Requires = ""[^""]+""", RegexOptions.Multiline)
                        && !Regex.IsMatch(n.Node, @"^LockedYarnNode = ""[^""]+""", RegexOptions.Multiline))
            .Select(n => $"{n.File}: {n.Node.Split('\n')[0]} has Requires but no LockedYarnNode")
            .ToList();
        Assert.Empty(bad);
    }

    [Fact]
    public void Every_clue_has_a_notebook_entry_and_every_entry_a_clue()
    {
        var set = Find(@"<<flag\s+(clue_\w+)\s*>>", ".yarn").Concat(Find(@"SetFlag\(""(clue_\w+)""\s*\)", ".cs"))
            .Select(m => m.Value).ToHashSet();
        var entries = Ids("content/clues");
        Assert.NotEmpty(set);
        Assert.Empty(set.Except(entries).Select(f => $"no content/clues/{f}.tres"));
        Assert.Empty(entries.Except(set).Select(f => $"content/clues/{f}.tres: nothing sets {f}"));
    }

    [Fact]
    public void Speakers_are_known_characters()
    {
        // Yarn reads any "Text:" at the start of a line as a speaker, so a stray colon in
        // narration silently becomes a name plate.
        var names = Find(@"^DisplayName = ""([^""]+)""", ".tres")
            .Where(m => m.File.StartsWith(Path.Combine("content", "characters")))
            .Select(m => m.Value).ToHashSet();
        var bad = new List<string>();
        foreach (var (file, text) in Files(".yarn"))
        {
            foreach (Match body in Regex.Matches(text, @"^---\s*$(.*?)^===", RegexOptions.Multiline | RegexOptions.Singleline))
            foreach (Match line in Regex.Matches(body.Groups[1].Value, @"^\s*(?:->\s*)?([^:<>#\r\n]+?):", RegexOptions.Multiline))
                if (!names.Contains(line.Groups[1].Value.Trim())) bad.Add($"{file}: \"{line.Value.Trim()}\" reads as a speaker");
        }
        Assert.NotEmpty(names);
        Assert.Empty(bad);
    }
}
