using QKey.Core;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace QKey.Windows;

internal sealed class QKeyApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _notifyIcon;
    private readonly KeyboardHook _hook;
    private readonly MacroManager _macros = new();
    private readonly SettingsStore _settingsStore;
    private AppSettings _settings;
    private string _raw = string.Empty;
    private string _rendered = string.Empty;
    private bool _injecting;

    public QKeyApplicationContext()
    {
        _settingsStore = new SettingsStore(DefaultSettingsPath());
        _settings = _settingsStore.Load();

        _macros.Set("dc", "được");
        _macros.Set("vn", "Việt Nam");

        _notifyIcon = new NotifyIcon
        {
            Icon = System.Drawing.SystemIcons.Application,
            Visible = true,
            ContextMenuStrip = BuildMenu()
        };
        UpdateTrayText();

        _hook = new KeyboardHook();
        _hook.KeyPressed += OnKeyPressed;
        _hook.Start();
    }

    private static string DefaultSettingsPath()
    {
        var appData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        return Path.Combine(appData, "QKey", "settings.json");
    }

    private ContextMenuStrip BuildMenu()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("Bật/tắt QKey (Ctrl+Shift+V)", null, (_, _) => ToggleEnabled())
            .Checked = _settings.Enabled;
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add(BuildInputMethodMenu());
        menu.Items.Add(BuildQuickTypingMenu());
        menu.Items.Add(new ToolStripSeparator());
        menu.Items.Add("Mở thư mục cấu hình", null, (_, _) => OpenSettingsFolder());
        menu.Items.Add("Thoát", null, (_, _) => ExitThread());
        return menu;
    }

    private ToolStripMenuItem BuildInputMethodMenu()
    {
        var menu = new ToolStripMenuItem("Kiểu gõ");
        AddInputMethodItem(menu, "Telex", InputMethod.Telex);
        AddInputMethodItem(menu, "VNI", InputMethod.Vni);
        AddInputMethodItem(menu, "Simple Telex 1", InputMethod.SimpleTelex1);
        AddInputMethodItem(menu, "Simple Telex 2", InputMethod.SimpleTelex2);
        return menu;
    }

    private void AddInputMethodItem(ToolStripMenuItem menu, string text, InputMethod method)
    {
        var item = new ToolStripMenuItem(text)
        {
            Checked = _settings.InputMethod == method
        };
        item.Click += (_, _) => UpdateSettings(_settings with { InputMethod = method });
        menu.DropDownItems.Add(item);
    }

    private ToolStripMenuItem BuildQuickTypingMenu()
    {
        var menu = new ToolStripMenuItem("Quick Typing");
        menu.DropDownItems.Add(BuildBooleanItem("Quick Telex", _settings.QuickTelex,
            value => _settings with { QuickTelex = value }));
        menu.DropDownItems.Add(BuildBooleanItem("Quick Start Consonant", _settings.QuickStartConsonant,
            value => _settings with { QuickStartConsonant = value }));
        menu.DropDownItems.Add(BuildBooleanItem("Quick End Consonant", _settings.QuickEndConsonant,
            value => _settings with { QuickEndConsonant = value }));
        return menu;
    }

    private ToolStripMenuItem BuildBooleanItem(string text, bool current, Func<bool, AppSettings> update)
    {
        var item = new ToolStripMenuItem(text) { Checked = current };
        item.Click += (_, _) => UpdateSettings(update(!current));
        return item;
    }

    private void OnKeyPressed(object? sender, KeyPressedEventArgs e)
    {
        if (_injecting) return;

        if (e.Control && e.Shift && e.Key == Keys.V)
        {
            ToggleEnabled();
            e.Handled = true;
            return;
        }
        if (e.Control && e.Shift && e.Key == Keys.M)
        {
            ToggleMethod();
            e.Handled = true;
            return;
        }
        if (!_settings.Enabled) return;

        if (e.Key == Keys.Back)
        {
            if (_raw.Length > 0)
            {
                _raw = _raw[..^1];
                _rendered = CurrentEngine().ConvertWord(_raw);
            }
            return;
        }

        if (IsCommitKey(e.Key))
        {
            if (e.Key == Keys.Space && _macros.TryExpand(_raw, out var replacement))
            {
                e.Handled = true;
                ReplaceCurrentWord((replacement ?? string.Empty) + " ");
            }
            ResetBuffer();
            return;
        }

        var ch = KeyToChar(e.Key, e.Shift);
        if (ch is null) return;

        _raw += ch.Value;
        var converted = CurrentEngine().ConvertWord(_raw);
        if (_rendered.Length == 0) _rendered = ch.Value.ToString();

        if (converted != _rendered)
        {
            e.Handled = true;
            ReplaceCurrentWord(converted);
        }
        _rendered = converted;
    }

    private VietnameseEngine CurrentEngine() => new(_settings.ToEngineOptions());

    private void ReplaceCurrentWord(string text)
    {
        _injecting = true;
        try
        {
            SendKeys.SendWait(new string('\b', Math.Max(0, _rendered.Length + 1)));
            Clipboard.SetText(text);
            SendKeys.SendWait("^v");
        }
        finally
        {
            _injecting = false;
        }
    }

    private static bool IsCommitKey(Keys key) => key is Keys.Space or Keys.Enter or Keys.Tab or Keys.Escape
        or Keys.OemPeriod or Keys.Oemcomma or Keys.OemSemicolon or Keys.OemQuestion;

    private static char? KeyToChar(Keys key, bool shift)
    {
        if (key >= Keys.A && key <= Keys.Z)
        {
            var ch = (char)('a' + (key - Keys.A));
            return shift ? char.ToUpperInvariant(ch) : ch;
        }
        if (key >= Keys.D0 && key <= Keys.D9) return (char)('0' + (key - Keys.D0));
        if (key >= Keys.NumPad0 && key <= Keys.NumPad9) return (char)('0' + (key - Keys.NumPad0));
        return null;
    }

    private void ToggleEnabled()
    {
        UpdateSettings(_settings with { Enabled = !_settings.Enabled });
    }

    private void ToggleMethod()
    {
        var next = _settings.InputMethod switch
        {
            InputMethod.Telex => InputMethod.Vni,
            InputMethod.Vni => InputMethod.SimpleTelex1,
            InputMethod.SimpleTelex1 => InputMethod.SimpleTelex2,
            _ => InputMethod.Telex
        };
        UpdateSettings(_settings with { InputMethod = next });
    }

    private void UpdateSettings(AppSettings settings)
    {
        _settings = settings;
        _settingsStore.Save(_settings);
        ResetBuffer();
        _notifyIcon.ContextMenuStrip = BuildMenu();
        UpdateTrayText();
    }

    private void OpenSettingsFolder()
    {
        var settingsDirectory = Path.GetDirectoryName(DefaultSettingsPath());
        if (settingsDirectory is null) return;
        Directory.CreateDirectory(settingsDirectory);
        System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
        {
            FileName = settingsDirectory,
            UseShellExecute = true
        });
    }

    private void ResetBuffer()
    {
        _raw = string.Empty;
        _rendered = string.Empty;
    }

    private void UpdateTrayText()
    {
        _notifyIcon.Text = $"QKey {(_settings.Enabled ? "ON" : "OFF")} - {_settings.InputMethod}";
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _hook.Dispose();
            _notifyIcon.Dispose();
        }
        base.Dispose(disposing);
    }
}

internal sealed class KeyPressedEventArgs : EventArgs
{
    public required Keys Key { get; init; }
    public required bool Shift { get; init; }
    public required bool Control { get; init; }
    public bool Handled { get; set; }
}

internal sealed class KeyboardHook : IDisposable
{
    private const int WH_KEYBOARD_LL = 13;
    private const int WM_KEYDOWN = 0x0100;
    private const int WM_SYSKEYDOWN = 0x0104;
    private readonly LowLevelKeyboardProc _proc;
    private IntPtr _hookId;

    public event EventHandler<KeyPressedEventArgs>? KeyPressed;

    public KeyboardHook()
    {
        _proc = HookCallback;
    }

    public void Start()
    {
        _hookId = SetWindowsHookEx(WH_KEYBOARD_LL, _proc, GetModuleHandle(null), 0);
    }

    private IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
    {
        if (nCode >= 0 && (wParam == WM_KEYDOWN || wParam == WM_SYSKEYDOWN))
        {
            var vkCode = Marshal.ReadInt32(lParam);
            var args = new KeyPressedEventArgs
            {
                Key = (Keys)vkCode,
                Shift = (GetKeyState((int)Keys.ShiftKey) & 0x8000) != 0,
                Control = (GetKeyState((int)Keys.ControlKey) & 0x8000) != 0
            };
            KeyPressed?.Invoke(this, args);
            if (args.Handled) return 1;
        }
        return CallNextHookEx(_hookId, nCode, wParam, lParam);
    }

    public void Dispose()
    {
        if (_hookId != IntPtr.Zero) UnhookWindowsHookEx(_hookId);
    }

    private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr SetWindowsHookEx(int idHook, LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern bool UnhookWindowsHookEx(IntPtr hhk);

    [DllImport("user32.dll", SetLastError = true)]
    private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode, IntPtr wParam, IntPtr lParam);

    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern IntPtr GetModuleHandle(string? lpModuleName);

    [DllImport("user32.dll")]
    private static extern short GetKeyState(int nVirtKey);
}
