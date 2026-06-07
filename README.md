# QKey

**QKey** là bộ gõ tiếng Việt cho Windows, viết mới bằng C#/.NET, ưu tiên core engine sạch, test được và có thể nâng lên Windows TSF IME native trong các phase sau.

> Trạng thái: early preview. Phiên bản hiện tại đã chuyển hướng khỏi AutoHotkey prototype sang nền tảng .NET tử tế hơn: `QKey.Core` + `QKey.Windows` tray app.

## Mục tiêu

- Bộ gõ Windows nhẹ, chạy local, không gửi dữ liệu ra ngoài.
- Telex/VNI ổn định trước, sau đó mở rộng Simple Telex, Quick Typing, Macro, Smart Switch.
- Core engine tách riêng để test tự động và tái dùng cho TSF IME native.
- UX đơn giản: tray icon, hotkey bật/tắt, settings UI ở phase sau.

## Tính năng hiện có

- Core engine C#/.NET 8:
  - Telex Unicode cơ bản.
  - VNI Unicode cơ bản.
  - Simple Telex 1/2.
  - Quick Typing: Quick Telex, Quick Start Consonant, Quick End Consonant.
  - Macro manager.
  - Text converter: xóa dấu, sentence case, title case.
- Windows tray app skeleton:
  - Low-level keyboard hook.
  - Bật/tắt bằng `Ctrl+Shift+V`.
  - Chuyển Telex/VNI/Simple Telex bằng `Ctrl+Shift+M`.
  - Tray menu cấu hình kiểu gõ và Quick Typing.
  - Lưu settings JSON tại `%AppData%\QKey\settings.json`.
  - Macro expansion khi nhấn Space.
- Test harness chạy được không cần NuGet.
- AutoHotkey prototype vẫn còn ở `src/QKey.ahk` như fallback tạm thời, không phải hướng chính.

## Cấu trúc repo

```text
src/QKey.Core/        Core engine, macro, converter
src/QKey.Core.Tests/  Console test harness
src/QKey.Windows/     Windows tray app
src/QKey.ahk          Legacy prototype/fallback
docs/                 Kiến trúc, changelog, kế hoạch
```

## Build/test core

Trên Linux/macOS/Windows có .NET 8 SDK:

```bash
dotnet build src/QKey.Core.Tests/QKey.Core.Tests.csproj
dotnet run --project src/QKey.Core.Tests/QKey.Core.Tests.csproj
```

Kết quả mong đợi:

```text
OK: QKey .NET core tests passed
```

## Build Windows app

Trên Windows có .NET 8 SDK:

```powershell
dotnet publish src/QKey.Windows/QKey.Windows.csproj -c Release -r win-x64 --self-contained true
```

Chạy file `QKey.exe` trong thư mục publish.

Repo cũng có GitHub Actions `Windows Build` để build tự động trên `windows-latest`. Mỗi tag `v*` sẽ publish artifact `QKey-win-x64.zip` vào GitHub Release.

## Phím tắt Windows app

- `Ctrl+Shift+V`: bật/tắt QKey.
- `Ctrl+Shift+M`: xoay vòng Telex/VNI/Simple Telex 1/Simple Telex 2.

## Cấu hình

QKey lưu cấu hình tại:

```text
%AppData%\QKey\settings.json
```

Tray menu hiện hỗ trợ bật/tắt:

- Kiểu gõ: Telex, VNI, Simple Telex 1, Simple Telex 2.
- Quick Telex.
- Quick Start Consonant.
- Quick End Consonant.

## Roadmap

- GitHub Actions build Windows `.exe` release artifact.
- Settings UI đầy đủ cho macro, hotkey, excluded apps.
- Spell check và restore-if-wrong-spelling.
- Smart switch theo app/window.
- Debug/log window.
- TCVN3/VNI Windows legacy code tables.
- TSF IME native khi core ổn định.
