# Kiến trúc QKey

QKey được viết mới cho Windows, không phụ thuộc code macOS/Swift. MVP chọn hướng Windows-native nhẹ bằng AutoHotkey v2.

## Thành phần

- `src/QKey.ahk`: script chạy trên Windows, bắt phím, quản lý buffer, chuyển đổi Telex/VNI, inject text.
- `src/qkey_engine.py`: engine Python tương đương dùng cho test logic trên Linux/CI.
- `tests/test_engine.py`: test cases cho các quy tắc chính.

## Luồng xử lý

1. Người dùng gõ phím chữ/số.
2. QKey thêm ký tự vào buffer từ hiện tại.
3. Engine thử chuyển buffer sang tiếng Việt theo mode Telex/VNI.
4. Nếu kết quả đổi, QKey xóa độ dài buffer cũ và gửi chuỗi mới.
5. Space/Enter/Tab/punctuation commit buffer.

## Ưu tiên thiết kế

- Chạy local, không gửi dữ liệu ra ngoài.
- Dễ đọc/dễ chỉnh hơn là tối ưu tuyệt đối.
- MVP dùng Unicode dựng sẵn.
