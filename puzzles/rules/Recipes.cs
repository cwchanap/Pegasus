using System.Collections.Generic;
using System.Linq;

namespace Pegasus.Puzzles;

/// <summary>
/// What two items make together (pure, tested), in either order. Combining is for good:
/// the two go and the result stays, so it saves as items.
/// </summary>
public static class Recipes
{
    public static readonly IReadOnlyList<(string A, string B, string Result)> All = new[]
    {
        ("newspaper", "charcoal", "kindling"),
    };

    /// <summary>The item id <paramref name="a"/> and <paramref name="b"/> make, or null.</summary>
    public static string? Combine(string a, string b) =>
        All.FirstOrDefault(r => (r.A == a && r.B == b) || (r.A == b && r.B == a)).Result;
}
