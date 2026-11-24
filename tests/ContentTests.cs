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
    public void Yarn_nodes_that_are_played_exist()
    {
        var nodes = YarnNodes();
        var played = Find(@"^(?:YarnNode|LockedYarnNode) = ""([^""]+)""", ".tscn", ".tres")
            .Concat(Find(@"^YarnScene = ""([^""]+)""", ".tscn", ".tres").Select(m => (m.File, Value: m.Value + "1")))
            .Concat(Find(@"Conversation\.Start\(""([^""]+)""", ".cs"))
            .Concat(Find(@"(?:Say|Solve)\(""([^""]+)""", ".cs").Where(m => m.File.StartsWith("puzzles")))
            .Concat(Find(@"Node = ""([^""]+)"";", ".cs"))
            .ToList();
        Assert.NotEmpty(played);
        Assert.Empty(played.Where(p => !nodes.Contains(p.Value)).Select(p => $"{p.File}: no Yarn node '{p.Value}'"));
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
