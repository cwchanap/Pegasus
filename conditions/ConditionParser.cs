using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Pegasus.Core;

namespace Pegasus.Conditions;

/// <summary>
/// Grammar (whitespace ignored, names case-insensitive):
///   expr   := and ('|' and)*
///   and    := unary ('&amp;' unary)*
///   unary  := '!' unary | '(' expr ')' | atom
///   atom   := knowledge|guts|fitness|money '&gt;=' N
///           | bond:ID '&gt;=' N
///           | item:ID | flag:ID
/// </summary>
internal static partial class ConditionParser
{
    [GeneratedRegex(@"\s*(\(|\)|\||&|!|>=|[A-Za-z_][A-Za-z0-9_]*(?::[A-Za-z0-9_]*)?|\d+)")]
    private static partial Regex TokenRegex();

    public static Condition Parse(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return new Always();
        var tokens = Tokenize(text);
        var pos = 0;
        var result = ParseOr(tokens, ref pos);
        if (pos != tokens.Count) throw new ConditionSyntaxException($"Unexpected '{tokens[pos]}' in \"{text}\"");
        return result;
    }

    private static List<string> Tokenize(string text)
    {
        var tokens = new List<string>();
        var i = 0;
        while (i < text.Length)
        {
            if (char.IsWhiteSpace(text[i])) { i++; continue; }
            var m = TokenRegex().Match(text, i);
            if (!m.Success || m.Index != i) throw new ConditionSyntaxException($"Bad character at {i} in \"{text}\"");
            tokens.Add(m.Groups[1].Value);
            i += m.Length;
        }
        return tokens;
    }

    private static Condition ParseOr(List<string> t, ref int pos)
    {
        var parts = new List<Condition> { ParseAnd(t, ref pos) };
        while (Peek(t, pos) == "|") { pos++; parts.Add(ParseAnd(t, ref pos)); }
        return parts.Count == 1 ? parts[0] : new AnyOf(parts.ToArray());
    }

    private static Condition ParseAnd(List<string> t, ref int pos)
    {
        var parts = new List<Condition> { ParseUnary(t, ref pos) };
        while (Peek(t, pos) == "&") { pos++; parts.Add(ParseUnary(t, ref pos)); }
        return parts.Count == 1 ? parts[0] : new AllOf(parts.ToArray());
    }

    private static Condition ParseUnary(List<string> t, ref int pos)
    {
        var token = Next(t, ref pos);
        if (token == "!") return new Not(ParseUnary(t, ref pos));
        if (token == "(")
        {
            var inner = ParseOr(t, ref pos);
            Expect(t, ref pos, ")");
            return inner;
        }
        return ParseAtom(token, t, ref pos);
    }

    private static Condition ParseAtom(string token, List<string> t, ref int pos)
    {
        var colon = token.IndexOf(':');
        var kind = (colon < 0 ? token : token[..colon]).ToLowerInvariant();
        var id = colon < 0 ? "" : token[(colon + 1)..];
        var needsId = kind is "item" or "flag" or "bond";
        if (needsId && id == "") throw new ConditionSyntaxException($"'{kind}' needs an id, e.g. {kind}:name");
        if (!needsId && colon >= 0) throw new ConditionSyntaxException($"'{kind}' takes no id: '{token}'");
        switch (kind)
        {
            case "item": return new HasItem(id);
            case "flag": return new FlagSet(id);
            case "bond": return new BondAtLeast(id, ParseAtLeast(t, ref pos));
            case "money": return new MoneyAtLeast(ParseAtLeast(t, ref pos));
        }
        if (colon < 0 && !char.IsDigit(kind[0]) && Enum.TryParse<Stat>(kind, ignoreCase: true, out var stat))
            return new StatAtLeast(stat, ParseAtLeast(t, ref pos));
        throw new ConditionSyntaxException($"Unknown condition '{token}'");
    }

    private static int ParseAtLeast(List<string> t, ref int pos)
    {
        Expect(t, ref pos, ">=");
        var n = Next(t, ref pos);
        return int.TryParse(n, out var value) ? value : throw new ConditionSyntaxException($"Expected a number, got '{n}'");
    }

    private static string? Peek(List<string> t, int pos) => pos < t.Count ? t[pos] : null;

    private static string Next(List<string> t, ref int pos) =>
        pos < t.Count ? t[pos++] : throw new ConditionSyntaxException("Unexpected end of condition");

    private static void Expect(List<string> t, ref int pos, string expected)
    {
        var got = Next(t, ref pos);
        if (got != expected) throw new ConditionSyntaxException($"Expected '{expected}', got '{got}'");
    }
}
