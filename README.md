# QKey

<p align="center">
  <strong>Bộ gõ tiếng Việt nhẹ, riêng tư và kiểm thử được cho Windows.</strong>
</p>

<p align="center">
  <a href="https://github.com/theitm/Qkey/actions/workflows/windows-build.yml"><img alt="Windows Build" src="https://github.com/theitm/Qkey/actions/workflows/windows-build.yml/badge.svg"></a>
  <a href="https://github.com/theitm/Qkey/releases/latest"><img alt="Release" src="https://img.shields.io/github/v/release/theitm/Qkey?sort=semver"></a>
  <img alt=".NET" src="https://img.shields.io/badge/.NET-8.0-512BD4">
  <img alt="Platform" src="https://img.shields.io/badge/platform-Windows-0078D4">
</p>

**QKey** là bộ gõ tiếng Việt cho Windows, viết mới bằng C#/.NET 8. Dự án ưu tiên một core engine sạch, có test tự động, chạy local và đủ linh hoạt để nâng cấp lên Windows TSF IME native ở các phase sau.

> **Trạng thái:** early preview. QKey đã có core engine và Windows tray app dùng được ở mức thử nghiệm, nhưng chưa phải bản thay thế hoàn chỉnh cho bộ gõ hằng ngày.

## Điểm nổi bật

- **Riêng tư:** xử lý gõ tiếng Việt hoàn toàn local, không gửi nội dung gõ ra ngoài.
- **Nhẹ:** tray app Windows nhỏ gọn, không cần service nền phức tạp.
- **Có test:** core engine tách riêng và được kiểm thử bằng test harness chạy được trên Linux/macOS/Windows.
- **Dễ mở rộng:** kiến trúc `QKey.Core` + `QKey.Windows`, sẵn đường nâng cấp sang TSF IME native.
- **Có CI/CD:** GitHub Actions build app Windows và đính kèm artifact vào GitHub Release khi tạo tag `v*`.

## Tính năng hiện có

### Core engine

- Telex Unicode cơ bản.
- VNI Unicode cơ bản.
- Simple Telex 1/2.
- Quick Typing:
  - Quick Telex.
  - Quick Start Consonant.
  - Quick End Consonant.
- Macro manager.
- Text converter:
  - Xóa dấu.
  - Sentence case.
  - Title case.
- Settings JSON:
  - Kiểu gõ.
  - Bật/tắt QKey.
  - Quick Typing options.

### Windows tray app

- Low-level keyboard hook.
- Tray icon/menu để bật tắt và đổi kiểu gõ.
- Hotkey nhanh:
  - `Ctrl+Shift+V`: bật/tắt QKey.
  - `Ctrl+Shift+M`: xoay vòng Telex → VNI → Simple Telex 1 → Simple Telex 2.
- Lưu cấu hình tại `%AppData%\QKey\settings.json`.
- Macro expansion cơ bản khi nhấn `Space`.

## Tải bản Windows

Bản build tự động nằm trong GitHub Releases:

- **Latest release:** https://github.com/theitm/Qkey/releases/latest
- Artifact Windows: `QKey-win-x64.zip`

Cách chạy thử:

1. Tải `QKey-win-x64.zip` từ trang release.
2. Giải nén.
3. Chạy `QKey.exe`.
4. Dùng tray icon để đổi kiểu gõ hoặc bật/tắt Quick Typing.

> Windows có thể hiện cảnh báo SmartScreen vì app chưa ký code-signing certificate.

## Cấu hình

QKey lưu cấu hình người dùng tại:

```text
%AppData%\QKey\settings.json
```

Tray menu hiện hỗ trợ:

- Bật/tắt QKey.
- Chọn kiểu gõ:
  - Telex.
  - VNI.
  - Simple Telex 1.
  - Simple Telex 2.
- Bật/tắt Quick Typing:
  - Quick Telex.
  - Quick Start Consonant.
  - Quick End Consonant.
- Mở thư mục cấu hình.

Ví dụ settings:

```json
{
  "InputMethod": "Telex",
  "CodeTable": "Unicode",
  "Enabled": true,
  "QuickTelex": true,
  "QuickStartConsonant": false,
  "QuickEndConsonant": false,
  "SpellCheck": false,
  "RestoreIfWrongSpelling": true
}
```

## Build và test

Yêu cầu:

- .NET 8 SDK

Chạy test core:

```bash
dotnet build src/QKey.Core.Tests/QKey.Core.Tests.csproj
dotnet run --project src/QKey.Core.Tests/QKey.Core.Tests.csproj
```

Kết quả mong đợi:

```text
OK: QKey .NET core tests passed
```

Build Windows app:

```powershell
dotnet publish src/QKey.Windows/QKey.Windows.csproj -c Release -r win-x64 --self-contained true
```

File chạy nằm trong thư mục publish của project `QKey.Windows`.

## Cấu trúc repo

```text
src/QKey.Core/        Core engine, settings, macro, text converter
src/QKey.Core.Tests/  Console test harness cho core engine
src/QKey.Windows/     Windows tray app
src/QKey.ahk          Prototype/fallback tạm thời
python/               Prototype engine Python đời đầu
scripts/              Script build/test phụ trợ
docs/                 Kiến trúc, changelog, kế hoạch phát triển
```

Tài liệu thêm:

- `docs/ARCHITECTURE.md`: kiến trúc tổng quan.
- `docs/CHANGELOG.md`: lịch sử thay đổi.
- `docs/QKEY_WINDOWS_NATIVE_PLAN.md`: kế hoạch native Windows.

## Roadmap

### Gần

- Settings UI đầy đủ cho macro, hotkey và excluded apps.
- Smart switch theo app/window.
- Spell check và restore-if-wrong-spelling.
- Debug/log window để dễ kiểm tra hook và engine.

### Sau

- TCVN3/VNI Windows legacy code tables.
- Macro UI và import/export macro.
- Installer/signing cho Windows.
- TSF IME native khi core engine đủ ổn định.

## Ghi chú trạng thái

QKey đang ở giai đoạn preview. Nếu dùng để thử nghiệm, nên test trong các app đơn giản trước như Notepad/Notepad++ rồi mới thử trong trình duyệt, editor hoặc ứng dụng chat.
