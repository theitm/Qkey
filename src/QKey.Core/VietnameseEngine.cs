namespace QKey.Core;

public enum InputMethod
{
    Telex,
    Vni,
    SimpleTelex1,
    SimpleTelex2
}

public enum CodeTable
{
    Unicode,
    Tcvn3,
    VniWindows
}

public sealed record EngineOptions
{
    public InputMethod InputMethod { get; init; } = InputMethod.Telex;
    public CodeTable CodeTable { get; init; } = CodeTable.Unicode;
    public bool ModernToneStyle { get; init; } = true;
    public bool QuickTelex { get; init; } = true;
    public bool QuickStartConsonant { get; init; }
    public bool QuickEndConsonant { get; init; }
    public bool SpellCheck { get; init; }
    public bool RestoreIfWrongSpelling { get; init; } = true;
    public bool MacroEnabled { get; init; }
}

public sealed class VietnameseEngine
{
    private readonly EngineOptions _options;

    public VietnameseEngine(EngineOptions? options = null)
    {
        _options = options ?? new EngineOptions();
    }

    public string ConvertWord(string raw)
    {
        return _options.InputMethod switch
        {
            InputMethod.Vni => ConvertVni(raw),
            InputMethod.SimpleTelex1 => ConvertTelex(raw, _options, simple1: true),
            InputMethod.SimpleTelex2 => ConvertTelex(raw, _options, simple2: true),
            _ => ConvertTelex(raw, _options)
        };
    }

    private static string ConvertTelex(string raw, EngineOptions options, bool simple1 = false, bool simple2 = false)
    {
        var output = string.Empty;
        foreach (var ch in raw)
        {
            var low = char.ToLowerInvariant(ch);
            if (!(options.QuickStartConsonant && output.Length == 0 && low is 'f' or 'j') && TelexTone(low) is { } tone)
            {
                output = ApplyTone(output, tone);
            }
            else if (low == 'w' && !simple1 && !(options.QuickStartConsonant && output.Length == 0))
            {
                if (output.Length >= 2 && Normalize(output[^2]).Base == 'u' && Normalize(output[^1]).Base == 'o')
                {
                    output = output[..^2] + AccentChar(output[^2], 'ư') + AccentChar(output[^1], 'ơ');
                    continue;
                }
                var before = output;
                output = ReplaceLastVowel(output, "a", 'ă');
                if (output == before) output = ReplaceLastVowel(output, "o", 'ơ');
                if (output == before) output = ReplaceLastVowel(output, "u", 'ư');
                if (output == before) output += CaseLike(ch, 'ư');
            }
            else if (low == 'a' && output.EndsWith("a", StringComparison.OrdinalIgnoreCase))
            {
                output = output[..^1] + CaseLike(output[^1], 'â');
            }
            else if (low == 'e' && output.EndsWith("e", StringComparison.OrdinalIgnoreCase))
            {
                output = output[..^1] + CaseLike(output[^1], 'ê');
            }
            else if (low == 'o' && output.EndsWith("o", StringComparison.OrdinalIgnoreCase))
            {
                output = output[..^1] + CaseLike(output[^1], 'ô');
            }
            else if (low == 'd' && output.EndsWith("d", StringComparison.OrdinalIgnoreCase))
            {
                output = output[..^1] + CaseLike(output[^1], 'đ');
            }
            else
            {
                output += ch;
            }
        }
        return ApplyQuickTyping(output, options);
    }

    private static string ApplyQuickTyping(string word, EngineOptions options)
    {
        if (options.QuickTelex)
        {
            word = ReplacePrefix(word, "cc", "ch");
            word = ReplacePrefix(word, "gg", "gi");
            word = ReplacePrefix(word, "kk", "kh");
            word = ReplacePrefix(word, "nn", "ng");
            word = ReplacePrefix(word, "pp", "ph");
            word = ReplacePrefix(word, "qq", "qu");
            word = ReplacePrefix(word, "tt", "th");
        }

        if (options.QuickStartConsonant)
        {
            word = ReplacePrefix(word, "f", "ph");
            word = ReplacePrefix(word, "j", "gi");
            word = ReplacePrefix(word, "w", "qu");
        }

        if (options.QuickEndConsonant)
        {
            word = ReplaceSuffix(word, "g", "ng");
            word = ReplaceSuffix(word, "h", "nh");
            word = ReplaceSuffix(word, "k", "ch");
        }

        return word;
    }

    private static string ReplacePrefix(string word, string prefix, string replacement)
    {
        if (!word.StartsWith(prefix, StringComparison.OrdinalIgnoreCase)) return word;
        var actual = word[..prefix.Length];
        return MatchCasing(actual, replacement) + word[prefix.Length..];
    }

    private static string ReplaceSuffix(string word, string suffix, string replacement)
    {
        if (!word.EndsWith(suffix, StringComparison.OrdinalIgnoreCase)) return word;
        var actual = word[^suffix.Length..];
        return word[..^suffix.Length] + MatchCasing(actual, replacement);
    }

    private static string MatchCasing(string source, string value)
    {
        if (source.All(char.IsUpper)) return value.ToUpperInvariant();
        if (char.IsUpper(source[0])) return char.ToUpperInvariant(value[0]) + value[1..];
        return value;
    }

    private static string ConvertVni(string raw)
    {
        var output = string.Empty;
        foreach (var ch in raw)
        {
            if (VniTone(ch) is { } tone)
            {
                output = ApplyTone(output, tone);
            }
            else if (ch == '6')
            {
                var before = output;
                output = ReplaceLastVowel(output, "a", 'â');
                if (output == before) output = ReplaceLastVowel(output, "e", 'ê');
                if (output == before) output = ReplaceLastVowel(output, "o", 'ô');
                if (output == before) output = ApplyTone(output, '.');
            }
            else if (ch == '7')
            {
                var before = output;
                output = ReplaceLastVowel(output, "o", 'ơ');
                if (output == before) output = ReplaceLastVowel(output, "u", 'ư');
                if (output == before) output += ch;
            }
            else if (ch == '8')
            {
                var before = output;
                output = ReplaceLastVowel(output, "a", 'ă');
                if (output == before) output += ch;
            }
            else if (ch == '9' && output.EndsWith("d", StringComparison.OrdinalIgnoreCase))
            {
                output = output[..^1] + CaseLike(output[^1], 'đ');
            }
            else
            {
                output += ch;
            }
        }
        return output;
    }

    private static char? TelexTone(char ch) => ch switch
    {
        'f' => '`', 's' => '\'', 'r' => '?', 'x' => '~', 'j' => '.', _ => null
    };

    private static char? VniTone(char ch) => ch switch
    {
        '1' => '\'', '2' => '`', '3' => '?', '4' => '~', '5' => '.', _ => null
    };

    private static string ApplyTone(string word, char toneMark)
    {
        var pos = TonePosition(word);
        if (pos < 0) return word + toneMark;
        var tone = toneMark switch { '`' => 1, '\'' => 2, '?' => 3, '~' => 4, '.' => 5, _ => 0 };
        return word[..pos] + AccentChar(word[pos], null, tone) + word[(pos + 1)..];
    }

    private static int TonePosition(string word)
    {
        var positions = Enumerable.Range(0, word.Length).Where(i => IsVowel(word[i])).ToList();
        if (positions.Count == 0) return -1;
        foreach (var i in positions)
        {
            if ("êơôâăưÊƠÔÂĂƯ".Contains(word[i])) return i;
        }
        if (positions.Count >= 2)
        {
            var last = positions[^1];
            if ("iyu".Contains(char.ToLowerInvariant(word[last]))) return positions[^2];
            return positions.Count >= 3 ? positions[1] : positions[0];
        }
        return positions[0];
    }

    private static string ReplaceLastVowel(string word, string candidates, char target)
    {
        for (var i = word.Length - 1; i >= 0; i--)
        {
            var info = Normalize(word[i]);
            if (candidates.Contains(info.Base))
            {
                return word[..i] + AccentChar(word[i], target, info.Tone) + word[(i + 1)..];
            }
        }
        return word;
    }

    private static bool IsVowel(char ch) => "aăâeêioôơuưy".Contains(Normalize(ch).Base);

    private static char AccentChar(char source, char? newBase = null, int? tone = null)
    {
        var info = Normalize(source);
        var chars = AccentTable(newBase ?? info.Base);
        if (chars is null) return source;
        var outChar = chars[tone ?? info.Tone];
        return CaseLike(source, outChar);
    }

    private static (char Base, int Tone) Normalize(char ch)
    {
        var lower = char.ToLowerInvariant(ch);
        foreach (var baseChar in new[] { 'a', 'ă', 'â', 'e', 'ê', 'i', 'o', 'ô', 'ơ', 'u', 'ư', 'y' })
        {
            var table = AccentTable(baseChar)!;
            for (var i = 0; i < table.Length; i++)
            {
                if (lower == table[i]) return (baseChar, i);
            }
        }
        return (lower, 0);
    }

    private static char CaseLike(char source, char value)
    {
        return char.IsUpper(source) ? char.ToUpperInvariant(value) : value;
    }

    private static char[]? AccentTable(char baseChar) => baseChar switch
    {
        'a' => ['a', 'à', 'á', 'ả', 'ã', 'ạ'],
        'ă' => ['ă', 'ằ', 'ắ', 'ẳ', 'ẵ', 'ặ'],
        'â' => ['â', 'ầ', 'ấ', 'ẩ', 'ẫ', 'ậ'],
        'e' => ['e', 'è', 'é', 'ẻ', 'ẽ', 'ẹ'],
        'ê' => ['ê', 'ề', 'ế', 'ể', 'ễ', 'ệ'],
        'i' => ['i', 'ì', 'í', 'ỉ', 'ĩ', 'ị'],
        'o' => ['o', 'ò', 'ó', 'ỏ', 'õ', 'ọ'],
        'ô' => ['ô', 'ồ', 'ố', 'ổ', 'ỗ', 'ộ'],
        'ơ' => ['ơ', 'ờ', 'ớ', 'ở', 'ỡ', 'ợ'],
        'u' => ['u', 'ù', 'ú', 'ủ', 'ũ', 'ụ'],
        'ư' => ['ư', 'ừ', 'ứ', 'ử', 'ữ', 'ự'],
        'y' => ['y', 'ỳ', 'ý', 'ỷ', 'ỹ', 'ỵ'],
        _ => null
    };
}
