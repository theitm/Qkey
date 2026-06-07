using QKey.Core;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace QKey.Windows;

internal sealed class QKeyApplicationContext : ApplicationContext
{
    private readonly NotifyIcon _notifyIcon;
    private readonly KeyboardHook _hook;
    private readonly VietnameseEngine _telex = new(new EngineOptions { InputMethod = InputMethod.Telex });
    private readonly VietnameseEngine _vni = new(new EngineOptions { InputMethod = InputMethod.Vni });
    private readonly MacroManager _macros = new();
    private bool _enabled = true;
    private InputMethod _method = InputMethod.Telex;
    private string _raw = string.Empty;
    private string _rendered = string.Empty;
    private bool _injecting;

    public QKeyApplicationContext()
    {
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

    private ContextMenuStrip BuildMenu()
    {
        var menu = new ContextMenuStrip();
        menu.Items.Add("Bật/tắt QKey (Ctrl+Shift+V)", null, (_, _) => ToggleEnabled());
        menu.Items.Add("Đổi Telex/VNI (Ctrl+Shift+M)", null, (_, _) => ToggleMethod());
        menu.Items.Add("Thoát", null, (_, _) => ExitThread());
        return menu;
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
        if (!_enabled) return;

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

    private VietnameseEngine CurrentEngine() => _method == InputMethod.Vni ? _vni : _telex;

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
        _enabled = !_enabled;
        ResetBuffer();
        UpdateTrayText();
    }

    private void ToggleMethod()
    {
        _method = _method == InputMethod.Telex ? InputMethod.Vni : InputMethod.Telex;
        ResetBuffer();
        UpdateTrayText();
    }

    private void ResetBuffer()
    {
        _raw = string.Empty;
        _rendered = string.Empty;
    }

    private void UpdateTrayText()
    {
        _notifyIcon.Text = $"QKey {(_enabled ? "ON" : "OFF")} - {_method}";
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
