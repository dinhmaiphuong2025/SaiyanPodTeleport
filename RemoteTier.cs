using System;
using System.Collections.Generic;

namespace SaiyanPodTeleport;

public enum RemoteTier
{
    Base,
    Plus,
    Pro,
    ProMax
}

public class RemoteTierInfo
{
    public RemoteTier Tier { get; }
    public string ItemId { get; }
    public string DisplayName { get; }
    public string Description { get; }
    public int DailyLimit { get; } // -1 = Unlimited
    public float DescentSpeed { get; }
    public float AscentSpeed { get; }
    public float BoardingDuration { get; }
    public float ExitDuration { get; }
    public bool CanBypassLockedAreas { get; }

    public RemoteTierInfo(
        RemoteTier tier,
        string itemId,
        string displayName,
        string description,
        int dailyLimit,
        float descentSpeed,
        float ascentSpeed,
        float boardingDuration,
        float exitDuration,
        bool canBypassLockedAreas)
    {
        this.Tier = tier;
        this.ItemId = itemId;
        this.DisplayName = displayName;
        this.Description = description;
        this.DailyLimit = dailyLimit;
        this.DescentSpeed = descentSpeed;
        this.AscentSpeed = ascentSpeed;
        this.BoardingDuration = boardingDuration;
        this.ExitDuration = exitDuration;
        this.CanBypassLockedAreas = canBypassLockedAreas;
    }

    public static readonly RemoteTierInfo BaseInfo = new(
        tier: RemoteTier.Base,
        itemId: "dinhmaiphuong2025.SaiyanRemote.Base",
        displayName: "Saiyan Remote Mk.I (Base)",
        description: "Bộ điều khiển tàu Saiyan bản tiêu chuẩn của Capsule Corp. Giới hạn 2 lần/ngày, động cơ đời cũ khởi động chậm rãi, chỉ đáp ở khu vực đã mở khóa.",
        dailyLimit: 2,
        descentSpeed: 6.5f,       // Rơi chậm rãi, đầm chắc
        ascentSpeed: 6.0f,        // Bay lên từ từ
        boardingDuration: 2.2f,   // Bước vào tàu mất 2.2s
        exitDuration: 2.0f,       // Bước ra mất 2.0s
        canBypassLockedAreas: false
    );

    public static readonly RemoteTierInfo PlusInfo = new(
        tier: RemoteTier.Plus,
        itemId: "dinhmaiphuong2025.SaiyanRemote.Plus",
        displayName: "Saiyan Remote Mk.II (Plus)",
        description: "Bản nâng cấp hợp kim Cobalt với pin mở rộng. Cho phép gọi tàu 5 lần/ngày, cải thiện tốc độ phản lực nhưng vẫn cần thời gian kiểm tra buồng lái.",
        dailyLimit: 5,
        descentSpeed: 10.5f,      // Nhanh hơn Base ~60%
        ascentSpeed: 9.5f,
        boardingDuration: 1.6f,
        exitDuration: 1.5f,
        canBypassLockedAreas: false
    );

    public static readonly RemoteTierInfo ProInfo = new(
        tier: RemoteTier.Pro,
        itemId: "dinhmaiphuong2025.SaiyanRemote.Pro",
        displayName: "Saiyan Remote Mk.III (Pro)",
        description: "Bản khung mạ vàng Titan cao cấp. Pin đôi dung lượng lớn gọi tàu 10 lần/ngày, tốc độ đáp khá nhanh và buồng lái phản ứng nhạy bén.",
        dailyLimit: 10,
        descentSpeed: 15.5f,      // Tiếp cận mốc ProMax
        ascentSpeed: 14.0f,
        boardingDuration: 1.1f,
        exitDuration: 1.0f,
        canBypassLockedAreas: false
    );

    public static readonly RemoteTierInfo ProMaxInfo = new(
        tier: RemoteTier.ProMax,
        itemId: "dinhmaiphuong2025.SaiyanRemote.ProMax",
        displayName: "Saiyan Remote Ultra (Pro Max)",
        description: "Công nghệ tối thượng từ tinh thể Iridium. Không giới hạn số lần sử dụng trong ngày, tốc độ siêu thanh và bỏ qua mọi rào cản khu vực!",
        dailyLimit: -1,
        descentSpeed: 24.0f,      // Siêu tốc: rơi như thiên thạch, chớp nhoáng
        ascentSpeed: 22.0f,       // Vụt thẳng lên bầu trời
        boardingDuration: 0.65f,  // Mở cửa bước vào siêu tốc
        exitDuration: 0.55f,      // Bước ra chớp mắt
        canBypassLockedAreas: true
    );

    public static readonly Dictionary<string, RemoteTierInfo> ByItemId = new()
    {
        { BaseInfo.ItemId, BaseInfo },
        { PlusInfo.ItemId, PlusInfo },
        { ProInfo.ItemId, ProInfo },
        { ProMaxInfo.ItemId, ProMaxInfo },
        // ItemId cũ để backward compatibility
        { "dinhmaiphuong2025.SaiyanRemote", ProMaxInfo }
    };
}
