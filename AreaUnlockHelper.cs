using System;
using System.Collections.Generic;
using StardewValley;

namespace SaiyanPodTeleport;

public static class AreaUnlockHelper
{
    public static bool IsDestinationUnlocked(TeleportDestination dest, out string reason)
    {
        reason = string.Empty;

        // 1. Kiểm tra Lễ hội / Event chặn tại thị trấn Pelican & Bãi biển
        if (dest.LocationName == "Town" || dest.LocationName == "Beach")
        {
            if (Utility.isFestivalDay(Game1.dayOfMonth, Game1.season))
            {
                reason = "Khu vực đang chuẩn bị Lễ hội lớn! Không gian đã bị phong tỏa.";
                return false;
            }
        }

        // 2. Kiểm tra Sa mạc Calico
        if (dest.LocationName == "Desert")
        {
            // Kiểm tra xe buýt đã sửa chưa (hoàn thành Vault cc hoặc Joja)
            bool busRepaired = Game1.MasterPlayer.mailReceived.Contains("ccVault") 
                            || Game1.MasterPlayer.mailReceived.Contains("JojaVault")
                            || Game1.MasterPlayer.hasOrWillReceiveMail("ccVault");
            if (!busRepaired)
            {
                reason = "Tuyến xe buýt tới Sa mạc Calico chưa được khôi phục!";
                return false;
            }
        }

        // 3. Kiểm tra Đảo Ginger
        if (dest.LocationName.StartsWith("Island"))
        {
            // Kiểm tra thuyền của Willy đã được sửa chữa chưa
            bool islandUnlocked = Game1.MasterPlayer.mailReceived.Contains("willyBoatFixed")
                               || Game1.MasterPlayer.hasOrWillReceiveMail("willyBoatFixed");
            if (!islandUnlocked)
            {
                reason = "Tuyến hải trình tới Quần đảo Ginger chưa được khám phá!";
                return false;
            }
        }

        // 4. Kiểm tra Nhà kính
        if (dest.Id == "farm_greenhouse")
        {
            bool greenhouseFixed = Game1.MasterPlayer.mailReceived.Contains("ccPantry") 
                                || Game1.MasterPlayer.mailReceived.Contains("JojaPantry")
                                || Game1.MasterPlayer.hasOrWillReceiveMail("ccPantry");
            if (!greenhouseFixed)
            {
                reason = "Nhà kính nông trại chưa được phục hồi!";
                return false;
            }
        }

        // 5. Kiểm tra Đường ray & Suối nước nóng
        if (dest.LocationName == "Railroad")
        {
            bool earthquakeHappened = Game1.Date.Year > 1 
                                   || Game1.Date.Season != Season.Spring 
                                   || Game1.Date.DayOfMonth >= 3
                                   || Game1.MasterPlayer.mailReceived.Contains("earthquake");
            if (!earthquakeHappened)
            {
                reason = "Lối lên núi và đường ray đang bị sạt lở đá chặn!";
                return false;
            }
        }

        // 6. Kiểm tra Rừng Bí Mật
        if (dest.LocationName == "Woods")
        {
            bool hasSteelAxe = System.Linq.Enumerable.Any(System.Linq.Enumerable.OfType<StardewValley.Tools.Axe>(Game1.player.Items), a => a.UpgradeLevel >= 2);
            bool beenThere = Game1.player.mailReceived.Contains("beenToWoods");
            if (!hasSteelAxe && !beenThere)
            {
                reason = "Lối vào Rừng Bí Mật đang bị cây gỗ cứng chặn lối! Cần mang theo Rìu Thép trở lên.";
                return false;
            }
        }

        return true;
    }
}
