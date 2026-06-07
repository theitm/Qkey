# Kiến trúc QKey

QKey được xây dựng mới cho Windows, không phụ thuộc code macOS/Swift. Hướng dài hạn là bộ gõ Windows tử tế với core engine test được, Windows shell mỏng, sau này có thể nâng lên TSF IME native.

## Thành phần hiện tại

- `src/QKey.Core`: engine gõ tiếng Việt và tiện ích không phụ thuộc UI.
- `src/QKey.Core.Tests`: test harness console, không cần NuGet, chạy được trên Linux/CI.
- `src/QKey.Windows`: tray app WinForms cho Windows, dùng low-level keyboard hook và text injection.
- `src/QKey.ahk`: prototype AutoHotkey v2 cũ, giữ làm fallback/tham khảo tạm thời.

## Chức năng tham khảo từ XKey

QKey tham khảo danh sách chức năng cần thiết từ XKey, nhưng code Windows được viết mới:

- Telex, VNI, Simple Telex.
- Unicode first; TCVN3/VNI Windows ở phase sau.
- Quick Telex, Quick Start/End consonants.
- Macro/text shortcuts.
- Convert tools: xóa dấu, đổi hoa/thường, sentence/title case, chuyển bảng mã.
- Smart switch theo app/window.
- Excluded apps/rules.
- Spell check và restore-if-wrong-spelling.
- Debug/log window.
- Import/export settings và macro backup.

## Luồng xử lý Windows shell

1. `QKey.Windows` chạy ở system tray.
2. `KeyboardHook` nhận phím qua `WH_KEYBOARD_LL`.
3. Shell cập nhật raw buffer của từ hiện tại.
4. `QKey.Core.VietnameseEngine` chuyển raw buffer sang Unicode tiếng Việt.
5. Nếu kết quả thay đổi, shell thay từ hiện tại bằng chuỗi đã chuyển.
6. Space/Enter/Tab/punctuation commit buffer; Space cũng thử expand macro.

## Hướng dài hạn

Low-level keyboard hook là bước thực dụng để có bản Windows dùng được sớm. Khi core ổn, QKey nên thêm TSF IME native để tương thích tốt hơn với Windows, Office, trình duyệt, Terminal và các app bảo mật.
