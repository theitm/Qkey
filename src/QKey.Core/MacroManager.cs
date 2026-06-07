namespace QKey.Core;

public sealed class MacroManager
{
    private readonly Dictionary<string, string> _macros = new(StringComparer.OrdinalIgnoreCase);

    public void Set(string shortcut, string replacement)
    {
        if (string.IsNullOrWhiteSpace(shortcut)) throw new ArgumentException("Shortcut is required", nameof(shortcut));
        _macros[shortcut.Trim()] = replacement;
    }

    public bool Remove(string shortcut) => _macros.Remove(shortcut);

    public bool TryExpand(string shortcut, out string? replacement)
    {
        return _macros.TryGetValue(shortcut.Trim(), out replacement);
    }

    public IReadOnlyDictionary<string, string> Snapshot() => _macros;
}
