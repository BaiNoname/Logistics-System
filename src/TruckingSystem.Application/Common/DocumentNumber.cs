using System.Globalization;

namespace TruckingSystem.Application.Common;

public static class DocumentNumber
{
    public static string Next(string prefix, IEnumerable<string> existing)
    {
        var maximum = existing.Where(code => code.StartsWith(prefix, StringComparison.Ordinal))
            .Select(code => int.TryParse(code.AsSpan(prefix.Length), NumberStyles.None,
                CultureInfo.InvariantCulture, out var value) ? value : 0).DefaultIfEmpty().Max();
        return prefix + checked(maximum + 1).ToString("D3", CultureInfo.InvariantCulture);
    }
}
