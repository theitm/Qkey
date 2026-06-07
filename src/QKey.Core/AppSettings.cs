using System.Text.Json;

namespace QKey.Core;

public sealed record AppSettings
{
    public InputMethod InputMethod { get; init; } = InputMethod.Telex;
    public CodeTable CodeTable { get; init; } = CodeTable.Unicode;
    public bool Enabled { get; init; } = true;
    public bool QuickTelex { get; init; } = true;
    public bool QuickStartConsonant { get; init; }
    public bool QuickEndConsonant { get; init; }
    public bool SpellCheck { get; init; }
    public bool RestoreIfWrongSpelling { get; init; } = true;

    public EngineOptions ToEngineOptions() => new()
    {
        InputMethod = InputMethod,
        CodeTable = CodeTable,
        QuickTelex = QuickTelex,
        QuickStartConsonant = QuickStartConsonant,
        QuickEndConsonant = QuickEndConsonant,
        SpellCheck = SpellCheck,
        RestoreIfWrongSpelling = RestoreIfWrongSpelling
    };
}

public sealed class SettingsStore
{
    private readonly string _path;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        WriteIndented = true,
        PropertyNameCaseInsensitive = true
    };

    public SettingsStore(string path)
    {
        _path = path;
    }

    public AppSettings Load()
    {
        try
        {
            if (!File.Exists(_path)) return new AppSettings();
            var json = File.ReadAllText(_path);
            if (string.IsNullOrWhiteSpace(json)) return new AppSettings();
            return JsonSerializer.Deserialize<AppSettings>(json, JsonOptions) ?? new AppSettings();
        }
        catch (IOException)
        {
            return new AppSettings();
        }
        catch (UnauthorizedAccessException)
        {
            return new AppSettings();
        }
        catch (JsonException)
        {
            return new AppSettings();
        }
    }

    public void Save(AppSettings settings)
    {
        var directory = Path.GetDirectoryName(_path);
        if (!string.IsNullOrEmpty(directory)) Directory.CreateDirectory(directory);
        File.WriteAllText(_path, JsonSerializer.Serialize(settings, JsonOptions));
    }
}
