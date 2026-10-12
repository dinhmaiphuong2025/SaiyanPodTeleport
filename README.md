# Saiyan Pod Teleport (v1.0.0)

**Tác giả:** dinhmaiphuong2025  
**Yêu cầu:** Stardew Valley 1.6+ | SMAPI 4.0+  
**Nền tảng hỗ trợ:** Android (Cinderbox / SMAPI Android) & PC (Windows / Linux / macOS)  

---

## 1. Giới Thiệu (Overview)

**Saiyan Pod Teleport** mang công nghệ du hành không gian từ Capsule Corp vào thung lũng Stardew Valley! Thay vì dịch chuyển tức thời nhàm chán, mod mang đến trải nghiệm **hoạt cảnh điện ảnh đỉnh cao**:
- Tàu Saiyan lao từ quỹ đạo xuống mặt đất với vệt lửa phản lực bốc cháy.
- Va chạm mặt đất tạo làn khói bụi rung chuyển màn hình.
- Nắp buồng lái tự động nâng lên, nhân vật bước vào trong.
- Động cơ kích hoạt đẩy con tàu vút thẳng lên không trung, xé rách tầng khí quyển trước khi đáp xuống điểm đến.

Mod được tối ưu đặc biệt cho **màn hình cảm ứng điện thoại (Android Cinderbox)**: giao diện chia tab ngang, nút bấm lớn, không che khuất thanh công cụ, cơ chế chống chạm nhầm khi cuộn vuốt.

---

## 2. Bốn Phiên Bản Điều Khiển (Remote Tiers)

Mod cung cấp 4 cấp độ điều khiển từ cơ bản đến tối thượng, phù hợp với tiến trình chơi (RPG Progression):

| Cấp Độ | Tên Thiết Bị | Giới Hạn/Ngày | Tốc Độ Rơi/Bay | Thời Gian Ra/Vào | Vượt Rào Cản Khu Vực | Giá Mua (Shop) |
| :--- | :--- | :---: | :---: | :---: | :---: | :---: |
| **Mk.I (Base)** | Saiyan Remote Mk.I | 2 lần/ngày | Đầm chắc (6.5 / 6.0) | 2.2s / 2.0s | Không (chỉ khu vực đã mở) | 5,000g |
| **Mk.II (Plus)** | Saiyan Remote Mk.II | 5 lần/ngày | Nhanh +60% (10.5 / 9.5) | 1.6s / 1.5s | Không | 25,000g |
| **Mk.III (Pro)** | Saiyan Remote Mk.III | 10 lần/ngày | Tốc độ cao (15.5 / 14.0) | 1.1s / 1.0s | Không | 75,000g |
| **Ultra (Pro Max)** | Saiyan Remote Ultra | **Vô hạn** | Siêu thanh (24.0 / 22.0) | 0.65s / 0.55s | **Có (Vượt mọi rào cản)** | 200,000g |

> *Ghi chú:* Khi sở hữu nhiều remote trong túi, game sẽ tự động kích hoạt hiệu ứng của bản cấp cao nhất bạn đang mang.

---

## 3. Hệ Thống Skin Buồng Lái (Skins & Custom Thrusters)

Tàu Saiyan hỗ trợ 4 bộ ngoại trang độc quyền với hoạt ảnh ngọn lửa phản lực riêng biệt:
1. **Classic White:** Vỏ trắng viền xanh nguyên bản của người Saiyan, lửa cam truyền thống.
2. **Capsule Blue:** Hợp kim Capsule Corp xanh lam bóng bẩy, lửa phản lực Cyan tương lai.
3. **Golden Emperor:** Mạ vàng hoàng gia bóng loáng, lửa Plasma vàng kim rực rỡ.
4. **Obsidian Shadow:** Hợp kim bóng đêm tàng hình, lửa tím bóng tối huyền bí.

*Cách đổi skin:* Mở menu điều khiển -> chuyển sang tab **"Skin"** -> chạm vào mẫu tàu bạn muốn áp dụng.

---

## 4. Tính Năng Ghim Tọa Độ Tiếp Đất (Custom Landing Tiles)

Người chơi có thể tự do ghim tọa độ hạ cánh chính xác cho từng bản đồ (ví dụ: ngay trước cửa chuồng bò, cạnh hòm thư, hoặc trước lều câu cá):
- Trong menu chọn điểm đến, mỗi địa điểm có nút **"Ghim Tọa Độ Hiện Tại"**.
- Khi đứng ở bất kỳ ô đất mong muốn, bấm nút Ghim, lần sau tàu sẽ đáp chính xác ngay ô đất đó!

---

## 5. Hướng Dẫn Cài Đặt (Installation)

1. Cài đặt **SMAPI** bản mới nhất (4.0+).
2. Tải bản release tương ứng với nhu cầu của bạn (xem mục 7 bên dưới).
3. Giải nén thư mục `SaiyanPodTeleport` vào thư mục `Mods`:
   - **PC:** `Stardew Valley/Mods/`
   - **Android (Cinderbox):** `/storage/emulated/0/StardewValley/desktop/Mods/`
4. Khởi động game thông qua SMAPI.

---

## 6. Cấu Hình (config.json)

Sau lần chạy đầu tiên, file `config.json` sẽ được tạo:
```json
{
  "AutoGrantItemOnLoad": true,
  "GrantedRemoteTier": "Base",
  "ScreenShakeEnabled": true,
  "CurrentSkin": "default",
  "MerchantOpenTime": 600,
  "MerchantCloseTime": 2000,
  "CustomLandingTiles": {}
}
```
- `AutoGrantItemOnLoad`: `true` để tự động nhận remote vào túi đồ khi tải save; `false` nếu muốn tự kiếm tiền mua từ thương nhân.
- `GrantedRemoteTier`: Chọn cấp độ tự cấp (`"Base"`, `"Plus"`, `"Pro"`, `"ProMax"`, `"All"`, `"None"`).
- `ScreenShakeEnabled`: Bật/tắt hiệu ứng rung màn hình khi tàu đáp.

---

## 7. Các Gói Phát Hành (Release Packages)

- **Gói 1: SaiyanPodTeleport_v1.0.0_Base.zip** — Mặc định cấp bản Mk.I (Base).
- **Gói 2: SaiyanPodTeleport_v1.0.0_Plus.zip** — Mặc định cấp bản Mk.II (Plus).
- **Gói 3: SaiyanPodTeleport_v1.0.0_Pro.zip** — Mặc định cấp bản Mk.III (Pro).
- **Gói 4: SaiyanPodTeleport_v1.0.0_ProMax.zip** — Mặc định cấp bản Ultra (Pro Max).
- **Gói All-in-One: SaiyanPodTeleport_v1.0.0_AllInOne.zip** — Cấp đủ cả 4 bản remote để thử nghiệm.

*Khuyến nghị kết hợp:* Cài đặt thêm mod bổ trợ **`[CP] Chrono Merchant`** để trải nghiệm gặp gỡ Thương Nhân Thời Không bán các dòng remote này trong rừng!
