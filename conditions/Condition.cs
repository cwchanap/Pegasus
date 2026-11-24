using System;
using System.Linq;
using Pegasus.Core;

namespace Pegasus.Conditions;

/// <summary>
/// A yes/no question about the game state. Every gate asks one: a puzzle lock, a
/// dialogue choice, a shop item. Write them as text with <see cref="Parse"/>, e.g.
/// <c>"guts &gt;= 2 | item:flashlight"</c>.
/// </summary>
public abstract record Condition
{
    public abstract bool IsMet(GameState state);

    /// <summary>Short human text for locked choices, e.g. "Guts 2".</summary>
    public abstract string Describe();

    public static Condition Parse(string text) => ConditionParser.Parse(text);

    /// <summary>Empty or null text means "always".</summary>
    public static bool Check(string? text, GameState state) =>
        string.IsNullOrWhiteSpace(text) || Parse(text).IsMet(state);
}

public sealed record Always : Condition
{
    public override bool IsMet(GameState state) => true;
    public override string Describe() => "";
}

public sealed record StatAtLeast(Stat Stat, int Level) : Condition
{
    public override bool IsMet(GameState state) => state.GetStat(Stat) >= Level;
    public override string Describe() => $"{Stat} {Level}";
}

public sealed record MoneyAtLeast(int Amount) : Condition
{
    public override bool IsMet(GameState state) => state.Money >= Amount;
    public override string Describe() => $"¥{Amount}";
}

public sealed record BondAtLeast(string Character, int Level) : Condition
{
    public override bool IsMet(GameState state) => state.GetBond(Character) >= Level;
    public override string Describe() => $"Bond with {Character} {Level}";
}

public sealed record HasItem(string Item) : Condition
{
    public override bool IsMet(GameState state) => state.HasItem(Item);
    public override string Describe() => Item;
}

public sealed record FlagSet(string Flag) : Condition
{
    public override bool IsMet(GameState state) => state.HasFlag(Flag);
    public override string Describe() => Flag;
}

public sealed record Not(Condition Inner) : Condition
{
    public override bool IsMet(GameState state) => !Inner.IsMet(state);
    public override string Describe() => $"not {Inner.Describe()}";
}

public sealed record AllOf(Condition[] Parts) : Condition
{
    public override bool IsMet(GameState state) => Parts.All(p => p.IsMet(state));
    public override string Describe() => string.Join(" and ", Parts.Select(p => p.Describe()));
}

public sealed record AnyOf(Condition[] Parts) : Condition
{
    public override bool IsMet(GameState state) => Parts.Any(p => p.IsMet(state));
    public override string Describe() => string.Join(" or ", Parts.Select(p => p.Describe()));
}

public sealed class ConditionSyntaxException(string message) : Exception(message);
