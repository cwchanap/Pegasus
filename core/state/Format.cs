using System.Globalization;

namespace Pegasus.Core;

/// <summary>Shared text formats for numbers shown to the player.</summary>
public static class Format
{
    public static string Yen(int amount) => "¥" + amount.ToString("N0", CultureInfo.InvariantCulture);
}
