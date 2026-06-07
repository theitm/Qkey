# QKey

**QKey** là bộ gõ tiếng Việt nhẹ cho Windows, chạy local, ưu tiên Telex/VNI, macro và chuyển đổi nhanh.

> Trạng thái: MVP/preview. QKey dùng AutoHotkey v2 để có bản Windows nhẹ, dễ chạy và dễ đóng gói.

## Tính năng hiện có

- Gõ tiếng Việt Unicode bằng **Telex**.
- Gõ tiếng Việt Unicode bằng **VNI**.
- Bật/tắt nhanh bằng `Ctrl+Shift+V`.
- Chuyển Telex/VNI bằng `Ctrl+Shift+M`.
- Macro cơ bản trong `src/QKey.ahk`.
- Script kiểm thử engine bằng Python để xác minh quy tắc chuyển đổi.

## Cài đặt nhanh trên Windows

1. Cài [AutoHotkey v2](https://www.autohotkey.com/).
2. Tải repo này về máy Windows.
3. Double-click `src/QKey.ahk`.
4. Mở Notepad/Word/Chrome và gõ thử:
   - Telex: `tieengs Vieetj Nam` → `tiếng Việt Nam`
   - VNI: nhấn `Ctrl+Shift+M`, gõ `tie61ng Vie65t Nam` → `tiếng Việt Nam`

## Phím tắt

- `Ctrl+Shift+V`: bật/tắt QKey.
- `Ctrl+Shift+M`: chuyển Telex/VNI.
- `Ctrl+Shift+R`: reload script.

## Giới hạn MVP

- Chưa có GUI settings đầy đủ.
- Chưa hỗ trợ TCVN3/VNI Windows font legacy.
- Cơ chế inject dùng clipboard fallback của AutoHotkey, có thể cần tinh chỉnh thêm cho một số app bảo mật hoặc game.
- Chưa có installer `.exe`; có thể compile bằng Ahk2Exe trên Windows.

## Kiểm thử trên Linux/dev box

Repo có bản engine Python tương đương để test logic chuyển đổi:

```bash
python3 tests/test_engine.py
```

## Roadmap

- Settings UI nhỏ bằng AutoHotkey GUI.
- Import/export macro.
- Smart switch theo app/window.
- Build release `.exe` bằng GitHub Actions Windows runner.
- Tách engine sang C#/Rust nếu cần độ ổn định cao hơn.


