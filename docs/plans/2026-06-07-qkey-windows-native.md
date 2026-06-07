# QKey Windows Native Implementation Plan

> **For Hermes:** Use TDD for core behavior; Windows shell is build-verified where Linux cannot execute WinForms.

**Goal:** Chuyển QKey từ AutoHotkey prototype sang nền tảng Windows tử tế bằng .NET 8, có core engine test được và Windows tray app dùng keyboard hook.

**Architecture:** Core engine nằm trong `QKey.Core` để test độc lập và có thể tái dùng cho TSF IME sau này. `QKey.Windows` là Windows tray app mỏng: low-level keyboard hook, tray menu, hotkey, macro expansion và text injection. Phase sau có thể thay hook bằng TSF IME native mà vẫn giữ core.

**Tech Stack:** C#/.NET 8, WinForms tray app, Windows low-level keyboard hook, console test harness không phụ thuộc NuGet.

---

## XKey features to reference, not copy

- Telex/VNI/Simple Telex input modes.
- Unicode first; later TCVN3 and VNI Windows code tables.
- Quick Telex, Quick Start/End consonants.
- Macro/text shortcuts.
- Convert tools: remove diacritics, sentence/title/upper/lower case, code-table conversion.
- Smart switch by app/window.
- Excluded apps/rules.
- Spell check and restore-if-wrong-spelling.
- Debug/log window.
- Import/export settings and macro backup.

## Phase 1 tasks completed in this change

1. Add .NET solution and `QKey.Core` project.
2. Add failing test harness for Telex conversion, then implement engine.
3. Add tests for VNI, macro, and text converter, then implement them.
4. Add `QKey.Windows` WinForms tray app skeleton with keyboard hook.
5. Update README/architecture/changelog to describe .NET path and keep AutoHotkey as legacy prototype only.
6. Verify core on Linux with `dotnet build` and `dotnet run`.

## Next phases

### Phase 2: Windows CI and release artifact
- Add GitHub Actions `windows-latest` build.
- `dotnet publish src/QKey.Windows -c Release -r win-x64 --self-contained true`.
- Upload `.zip` artifact.

### Phase 3: Settings UI
- WinForms settings dialog for input method, hotkeys, macro list, excluded apps.
- JSON config under `%AppData%/QKey/settings.json`.

### Phase 4: Real IME path
- Prototype TSF COM host or C++/C# interop wrapper.
- Keep `QKey.Core` unchanged.
