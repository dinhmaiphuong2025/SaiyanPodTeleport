namespace SaiyanPodTeleport;

public class ModConfig
{
    public bool AutoGrantItemOnLoad { get; set; } = true;
    public int ShopPrice { get; set; } = 500;
    public bool ScreenShakeEnabled { get; set; } = true;
    public string CurrentSkin { get; set; } = "default";

    // Cấu hình thời lượng bay và hạ cánh (để mở rộng tàu rẻ / tàu đắt)
    public float DescentSpeed { get; set; } = 18f; // Tốc độ rơi tự do (tàu rẻ chậm & kịch tính hơn: 18f vs tàu đắt: 45f)
    public float AscentSpeed { get; set; } = 16f;  // Tốc độ cất cánh
    public float BoardingDuration { get; set; } = 1.2f; // Thời gian bước vào buồng lái
    public float ExitDuration { get; set; } = 1.1f;     // Thời gian bước ra buồng lái

    // Tọa độ tiếp đất tùy chỉnh do người chơi tự đặt cho từng map: "LocationName:X,Y"
    public System.Collections.Generic.Dictionary<string, string> CustomLandingTiles { get; set; } = new();

    // Lịch hoạt động của Thương Nhân Thời Không (ChronoMerchant)
    public int MerchantOpenTime { get; set; } = 600;   // 6:00 sáng xuất hiện
    public int MerchantCloseTime { get; set; } = 2000; // 20:00 (8:00 tối) chui vào vết nứt không gian rời đi
}
