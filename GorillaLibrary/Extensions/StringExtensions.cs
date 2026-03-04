using GorillaLibrary.Utilities;
using System.Collections.Generic;
using System.Globalization;

namespace GorillaLibrary.Extensions;

public static class StringExtensions
{
    private static readonly CultureInfo cultureInfo = CultureInfo.InvariantCulture;

    private static readonly TextInfo _textInfo = cultureInfo.TextInfo;

    private static readonly Dictionary<string, string> _nameCache = [];

    public static string SanitizeName(this string name)
    {
        name ??= string.Empty;

        if (_nameCache.TryGetValue(name, out string sanitizedName))
            return sanitizedName;

        sanitizedName = RigUtility.LocalRig.NormalizeName(true, name);
        _nameCache.TryAdd(name, sanitizedName);

        return sanitizedName;
    }

    public static string ToTitleCase(this string original, bool forceLower = true) => _textInfo.ToTitleCase(forceLower ? original.ToLower() : original);

    public static string LimitLength(this string str, int maxLength) => str.Length > maxLength ? str[..maxLength] : str;
}
