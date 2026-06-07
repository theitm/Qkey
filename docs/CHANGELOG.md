# Changelog

Dự án tuân theo tinh thần [Keep a Changelog](https://keepachangelog.com/) và Semantic Versioning.

## [0.2.0] - 2026-06-07

### Added
- Thêm Quick Typing trong core engine: Quick Telex, Quick Start Consonant, Quick End Consonant.
- Thêm Simple Telex 1/2 test coverage trong core engine.
- Thêm GitHub Actions `Windows Build` để build `.exe` trên Windows runner và upload release artifact.
- Thêm solution .NET 8 cho hướng Windows tử tế.
- Thêm `QKey.Core` với engine Telex/VNI Unicode cơ bản.
- Thêm `MacroManager` và `TextConverter`.
- Thêm `QKey.Core.Tests` console test harness không phụ thuộc NuGet.
- Thêm `QKey.Windows` WinForms tray app skeleton với low-level keyboard hook.
- Thêm kế hoạch triển khai Windows native trong `docs/plans/`.

### Changed
- Cập nhật README/architecture: AutoHotkey chỉ còn là prototype/fallback, không phải hướng chính.

## [0.1.0] - 2026-06-07

### Added
- Khởi tạo QKey Windows MVP bằng AutoHotkey v2.
- Hỗ trợ Telex Unicode cơ bản.
- Hỗ trợ VNI Unicode cơ bản.
- Hotkey bật/tắt và chuyển mode.
- Engine Python tương đương để kiểm thử trên Linux.
- Tài liệu kiến trúc và README tiếng Việt.
