using System.Globalization;
using System.Text;

namespace QKey.Core;

public sealed class TextConverter
{
    public string RemoveDiacritics(string input)
    {
        var normalized = input.Normalize(NormalizationForm.FormD);
        var builder = new StringBuilder(capacity: normalized.Length);
        foreach (var ch in normalized)
        {
            var category = CharUnicodeInfo.GetUnicodeCategory(ch);
            if (category == UnicodeCategory.NonSpacingMark) continue;
            builder.Append(ch switch
            {
                'đ' => 'd',
                'Đ' => 'D',
                _ => ch
            });
        }
        return builder.ToString().Normalize(NormalizationForm.FormC);
    }

    public string ToSentenceCase(string input)
    {
        if (string.IsNullOrEmpty(input)) return input;
        return char.ToUpperInvariant(input[0]) + input[1..];
    }

    public string ToTitleCase(string input)
    {
        return CultureInfo.CurrentCulture.TextInfo.ToTitleCase(input.ToLowerInvariant());
    }
}
