using System.Collections.Generic;

namespace SaiyanPodTeleport;

public class PodSkinInfo
{
    public string Id { get; }
    public string DisplayName { get; }
    public string Description { get; }
    public string PassiveBuffDescription { get; }
    public string AssetPath { get; }
    public string FlameAssetPath { get; }

    public PodSkinInfo(
        string id,
        string displayName,
        string description,
        string passiveBuffDescription,
        string assetPath,
        string flameAssetPath)
    {
        this.Id = id;
        this.DisplayName = displayName;
        this.Description = description;
        this.PassiveBuffDescription = passiveBuffDescription;
        this.AssetPath = assetPath;
        this.FlameAssetPath = flameAssetPath;
    }

    public static readonly PodSkinInfo Classic = new(
        "classic",
        "Thép Saiyan Cổ Điển",
        "Tàu nguyên bản của chiến binh Saiyan viễn chinh.",
        "Thiết Giáp Chiến Binh: +5 Phòng thủ, +2 Kháng hiệu ứng",
        "assets/skins/pod_classic.png",
        "assets/skins/flame_classic.png"
    );

    public static readonly PodSkinInfo Capsule = new(
        "capsule",
        "Capsule Corp Special",
        "Công nghệ cánh Plasma Nano tân tiến từ tập đoàn Capsule.",
        "Lõi Lượng Tử: Tự động hồi phục Thể Lực & Máu liên tục",
        "assets/skins/pod_capsule.png",
        "assets/skins/flame_capsule.png"
    );

    public static readonly PodSkinInfo Obsidian = new(
        "obsidian",
        "Hắc Diện Tàng Hình",
        "Vỏ hợp kim hắc diện thạch vát góc hấp thụ sóng radar không gian.",
        "Chiến Thần Hư Không: +8 Sát Thương Tấn Công, +3 Phòng Thủ",
        "assets/skins/pod_obsidian.png",
        "assets/skins/flame_obsidian.png"
    );

    public static readonly PodSkinInfo Golden = new(
        "golden",
        "Hoàng Kim Đế Vương",
        "Phi thuyền mạ vàng nguyên chất dành riêng cho Hoàng tộc Saiyan.",
        "Chân Mệnh Đế Vương: +2 Vận May Hoàng Kim, Hút Vật Phẩm Siêu Xa",
        "assets/skins/pod_golden.png",
        "assets/skins/flame_golden.png"
    );

    public static readonly List<PodSkinInfo> AllSkins = new()
    {
        Classic,
        Capsule,
        Obsidian,
        Golden
    };

    public static readonly Dictionary<string, PodSkinInfo> ById = new()
    {
        ["classic"] = Classic,
        ["capsule"] = Capsule,
        ["obsidian"] = Obsidian,
        ["golden"] = Golden
    };
}
