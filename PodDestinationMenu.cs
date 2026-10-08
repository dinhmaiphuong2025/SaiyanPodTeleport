using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using StardewValley;
using StardewValley.Menus;

namespace SaiyanPodTeleport;

public class PodDestinationMenu : IClickableMenu
{
    private readonly Action<TeleportDestination> OnSelectDestination;
    private readonly Action<string> OnSelectSkin;
    private readonly List<TeleportDestination> AllDestinations;
    private readonly RemoteTierInfo CurrentTier;
    private readonly int RemainingUses;

    // 4 Tabs: 3 tab điểm đến + 1 tab ngoại trang
    private readonly List<string> Tabs = new() { "Nông trại", "Thung lũng", "Đảo & Sa mạc", "Ngoại trang" };
    private int CurrentTabIndex = 0;

    private List<TeleportDestination> FilteredDestinations = new();
    private List<PodSkinInfo> AllSkins = PodSkinInfo.AllSkins;

    private float CurrentScrollY = 0f;
    private float TargetScrollY = 0f;
    private float MaxScrollY = 0f;

    private readonly List<ClickableComponent> TabButtons = new();
    private readonly ClickableTextureComponent CloseButton;
    private readonly ClickableComponent SetCurrentTileButton;

    private bool IsDragging = false;
    private int DragStartY = 0;
    private float DragStartScrollY = 0f;
    private const float TouchSlop = 14f;
    private bool HasScrolled = false;

    // Kích thước Pill chuẩn mobile rộng rãi không tràn text
    private const int RowHeightDestination = 84; 
    private const int RowHeightSkin = 76; // 76px đủ 3 dòng text: Tên, Buff, Mô tả

    public PodDestinationMenu(
        List<TeleportDestination> destinations,
        RemoteTierInfo tier,
        int remainingUses,
        Action<TeleportDestination> onSelect,
        Action<string> onSelectSkin)
        : base(
            (int)Utility.getTopLeftPositionForCenteringOnScreen(880, 680).X,
            (int)Utility.getTopLeftPositionForCenteringOnScreen(880, 680).Y,
            880,
            680,
            showUpperRightCloseButton: true)
    {
        this.AllDestinations = destinations;
        this.CurrentTier = tier;
        this.RemainingUses = remainingUses;
        this.OnSelectDestination = onSelect;
        this.OnSelectSkin = onSelectSkin;

        // Nút Close góc trên bên phải
        this.CloseButton = new ClickableTextureComponent(
            new Rectangle(this.xPositionOnScreen + this.width - 56, this.yPositionOnScreen + 16, 48, 48),
            Game1.mouseCursors,
            new Rectangle(337, 494, 12, 12),
            4f
        );

        // Nút Lưu Vị Trí Hiện Tại Làm Điểm Đáp
        int pinBtnW = 240;
        int pinBtnH = 38;
        this.SetCurrentTileButton = new ClickableComponent(
            new Rectangle(this.xPositionOnScreen + this.width - pinBtnW - 68, this.yPositionOnScreen + 20, pinBtnW, pinBtnH),
            "Luu vi tri dang dung"
        );

        // Khởi tạo các Tab ngang: 4 tab vừa vặn chiều rộng menu
        int tabW = 188;
        int tabH = 46;
        int tabSpacing = 10;
        int tabsStartX = this.xPositionOnScreen + 44;
        int tabsY = this.yPositionOnScreen + 74;

        for (int i = 0; i < this.Tabs.Count; i++)
        {
            this.TabButtons.Add(new ClickableComponent(
                new Rectangle(tabsStartX + i * (tabW + tabSpacing), tabsY, tabW, tabH),
                this.Tabs[i]
            ));
        }

        this.UpdateFilteredList();
    }

    private bool IsSkinTab => (this.CurrentTabIndex == 3);

    private void UpdateFilteredList()
    {
        if (this.IsSkinTab)
        {
            // Trong tab Skin, dành 84px đầu tiên cho Ghim Ngoại Trang Đang Dùng
            // Danh sách cuộn bên dưới chỉ chứa 3 skin còn lại
            string currentEquippedSkin = ModEntry.Instance.Config.CurrentSkin;
            var otherSkins = this.AllSkins.Where(s => !s.Id.Equals(currentEquippedSkin, StringComparison.OrdinalIgnoreCase)).ToList();

            int contentHeight = otherSkins.Count * (RowHeightSkin + 10);
            int viewHeight = this.height - 250;
            this.MaxScrollY = Math.Max(0, contentHeight - viewHeight);
        }
        else
        {
            string selectedCategory = this.Tabs[this.CurrentTabIndex];
            this.FilteredDestinations = this.AllDestinations
                .Where(d => d.Category.Equals(selectedCategory, StringComparison.OrdinalIgnoreCase))
                .ToList();

            int contentHeight = this.FilteredDestinations.Count * (RowHeightDestination + 10);
            int viewHeight = this.height - 160;
            this.MaxScrollY = Math.Max(0, contentHeight - viewHeight);
        }

        this.CurrentScrollY = 0f;
        this.TargetScrollY = 0f;
    }

    public override void receiveLeftClick(int x, int y, bool playSound = true)
    {
        base.receiveLeftClick(x, y, playSound);

        if (this.CloseButton.containsPoint(x, y))
        {
            this.exitThisMenu();
            return;
        }

        // Chạm nút Ghim Tọa Độ Hiện Tại
        if (this.SetCurrentTileButton.containsPoint(x, y))
        {
            ModEntry.Instance.SetCustomLandingForCurrentLocation();
            this.exitThisMenu(playSound: false);
            return;
        }

        // Chuyển Tab
        for (int i = 0; i < this.TabButtons.Count; i++)
        {
            if (this.TabButtons[i].containsPoint(x, y))
            {
                if (this.CurrentTabIndex != i)
                {
                    this.CurrentTabIndex = i;
                    Game1.playSound("shwip");
                    this.UpdateFilteredList();
                }
                return;
            }
        }

        // Vùng danh sách
        Rectangle listBounds = this.IsSkinTab
            ? new Rectangle(this.xPositionOnScreen + 44, this.yPositionOnScreen + 224, this.width - 88, this.height - 250)
            : new Rectangle(this.xPositionOnScreen + 44, this.yPositionOnScreen + 130, this.width - 88, this.height - 160);

        if (listBounds.Contains(x, y))
        {
            this.IsDragging = true;
            this.DragStartY = y;
            this.DragStartScrollY = this.CurrentScrollY;
            this.HasScrolled = false;
        }
    }

    public override void leftClickHeld(int x, int y)
    {
        base.leftClickHeld(x, y);

        if (this.IsDragging)
        {
            float deltaY = this.DragStartY - y;
            if (Math.Abs(deltaY) > TouchSlop)
            {
                this.HasScrolled = true;
            }

            this.TargetScrollY = Math.Clamp(this.DragStartScrollY + deltaY, 0f, this.MaxScrollY);
        }
    }

    public override void releaseLeftClick(int x, int y)
    {
        base.releaseLeftClick(x, y);

        if (this.IsDragging)
        {
            if (!this.HasScrolled)
            {
                if (this.IsSkinTab)
                {
                    Rectangle listBounds = new Rectangle(this.xPositionOnScreen + 44, this.yPositionOnScreen + 224, this.width - 88, this.height - 250);
                    if (listBounds.Contains(x, y))
                    {
                        float relativeY = (y - listBounds.Y) + this.CurrentScrollY;
                        string currentEquippedSkin = ModEntry.Instance.Config.CurrentSkin;
                        var otherSkins = this.AllSkins.Where(s => !s.Id.Equals(currentEquippedSkin, StringComparison.OrdinalIgnoreCase)).ToList();

                        int clickedIndex = (int)(relativeY / (RowHeightSkin + 10));
                        if (clickedIndex >= 0 && clickedIndex < otherSkins.Count)
                        {
                            var skin = otherSkins[clickedIndex];
                            Game1.playSound("purchase");
                            this.OnSelectSkin?.Invoke(skin.Id);
                            this.UpdateFilteredList(); // Ghim skin mới lên đầu ngay
                        }
                    }
                }
                else
                {
                    Rectangle listBounds = new Rectangle(this.xPositionOnScreen + 44, this.yPositionOnScreen + 130, this.width - 88, this.height - 160);
                    if (listBounds.Contains(x, y))
                    {
                        float relativeY = (y - listBounds.Y) + this.CurrentScrollY;
                        int clickedIndex = (int)(relativeY / (RowHeightDestination + 10));
                        if (clickedIndex >= 0 && clickedIndex < this.FilteredDestinations.Count)
                        {
                            var dest = this.FilteredDestinations[clickedIndex];

                            if (!this.CurrentTier.CanBypassLockedAreas && !AreaUnlockHelper.IsDestinationUnlocked(dest, out string lockReason))
                            {
                                Game1.playSound("cancel");
                                Game1.addHUDMessage(new HUDMessage(lockReason, HUDMessage.error_type));
                                return;
                            }

                            Game1.playSound("powerup");
                            this.exitThisMenu(playSound: false);
                            this.OnSelectDestination?.Invoke(dest);
                        }
                    }
                }
            }
            this.IsDragging = false;
            this.HasScrolled = false;
        }
    }

    public override void receiveScrollWheelAction(int direction)
    {
        base.receiveScrollWheelAction(direction);
        this.TargetScrollY = Math.Clamp(this.TargetScrollY - direction * 0.5f, 0f, this.MaxScrollY);
    }

    public override void update(GameTime time)
    {
        base.update(time);
        this.CurrentScrollY = MathHelper.Lerp(this.CurrentScrollY, this.TargetScrollY, 0.25f);
    }

    public override void draw(SpriteBatch b)
    {
        // Shaded screen dim
        b.Draw(Game1.fadeToBlackRect, new Rectangle(0, 0, Game1.uiViewport.Width, Game1.uiViewport.Height), Color.Black * 0.6f);

        // Native Dialogue Box
        Game1.drawDialogueBox(this.xPositionOnScreen, this.yPositionOnScreen, this.width, this.height, false, true);

        // Header Title
        string title = this.IsSkinTab 
            ? "TÙY CHỈNH NGOẠI TRANG TÀU SAIYAN POD" 
            : $"{this.CurrentTier.DisplayName.ToUpper()} - TỌA ĐỘ TIẾP ĐÁP";
        Vector2 titleSize = Game1.dialogueFont.MeasureString(title) * 0.65f;
        Vector2 titlePos = new Vector2(this.xPositionOnScreen + (this.width - titleSize.X) / 2, this.yPositionOnScreen + 16);
        Utility.drawTextWithShadow(b, title, Game1.dialogueFont, titlePos, Color.Gold, 0.65f);

        // Subtitle / Uses info
        string usesText = this.IsSkinTab
            ? "Chạm vào mẫu ngoại trang bên dưới để trang bị và nhận ngay Nội Tại Lõi"
            : (this.CurrentTier.DailyLimit == -1 
                ? "Năng lượng: VÔ HẠN (Unlimited)" 
                : $"Lượt gọi hôm nay: {this.RemainingUses} / {this.CurrentTier.DailyLimit} lần");
        Color usesColor = this.IsSkinTab ? Color.Cyan : ((this.RemainingUses > 0 || this.CurrentTier.DailyLimit == -1) ? Color.LightGreen : Color.Tomato);
        Vector2 usesSize = Game1.smallFont.MeasureString(usesText);
        Vector2 usesPos = new Vector2(this.xPositionOnScreen + (this.width - usesSize.X) / 2, this.yPositionOnScreen + 48);
        b.DrawString(Game1.smallFont, usesText, usesPos, usesColor);

        // Nút Ghim Tọa Độ Tiếp Đất
        bool isCurrentLocPinned = Game1.currentLocation != null && ModEntry.Instance.Config.CustomLandingTiles.ContainsKey(Game1.currentLocation.Name);
        Color pinBg = isCurrentLocPinned ? new Color(170, 240, 180) : new Color(245, 235, 215);
        Color pinText = isCurrentLocPinned ? new Color(0, 100, 40) : new Color(90, 50, 20);
        string pinLabel = isCurrentLocPinned 
            ? $"Vi tri da luu: ({ModEntry.Instance.Config.CustomLandingTiles[Game1.currentLocation!.Name]})" 
            : "Luu vi tri dang dung";

        IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), 
            this.SetCurrentTileButton.bounds.X, this.SetCurrentTileButton.bounds.Y, 
            this.SetCurrentTileButton.bounds.Width, this.SetCurrentTileButton.bounds.Height, 
            pinBg, 1f, false);

        Vector2 pinTextSize = Game1.smallFont.MeasureString(pinLabel) * 0.75f;
        Vector2 pinTextPos = new Vector2(
            this.SetCurrentTileButton.bounds.X + (this.SetCurrentTileButton.bounds.Width - pinTextSize.X) / 2,
            this.SetCurrentTileButton.bounds.Y + (this.SetCurrentTileButton.bounds.Height - pinTextSize.Y) / 2
        );
        b.DrawString(Game1.smallFont, pinLabel, pinTextPos, pinText, 0f, Vector2.Zero, 0.75f, SpriteEffects.None, 1f);

        // Close Button
        this.CloseButton.draw(b);

        // Tabs
        for (int i = 0; i < this.TabButtons.Count; i++)
        {
            var tab = this.TabButtons[i];
            bool isSelected = (i == this.CurrentTabIndex);
            bool isSkinButton = (i == 3);

            Color tabBg;
            if (isSkinButton)
            {
                // Màu đặc biệt cho Tab Ngoại Trang: Tím huyền bí / Tím Neon
                tabBg = isSelected ? new Color(230, 205, 255) : new Color(185, 155, 220);
            }
            else
            {
                tabBg = isSelected ? new Color(245, 230, 205) : new Color(175, 160, 140);
            }

            Color tabText = isSelected ? new Color(70, 20, 90) : new Color(50, 30, 60);

            IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), tab.bounds.X, tab.bounds.Y, tab.bounds.Width, tab.bounds.Height, tabBg, 1f, false);

            Vector2 tabTextSize = Game1.smallFont.MeasureString(tab.name);
            Vector2 textPos = new Vector2(tab.bounds.X + (tab.bounds.Width - tabTextSize.X) / 2, tab.bounds.Y + (tab.bounds.Height - tabTextSize.Y) / 2);
            b.DrawString(Game1.smallFont, tab.name, textPos, tabText);
        }

        if (this.IsSkinTab)
        {
            // ==========================================
            // 1. Ô GHIM NGOẠI TRANG ĐANG DÙNG (CỐ ĐỊNH Ở TRÊN CÙNG)
            // ==========================================
            string currentEquippedSkin = ModEntry.Instance.Config.CurrentSkin;
            var equippedSkin = PodSkinInfo.ById.GetValueOrDefault(currentEquippedSkin, PodSkinInfo.Classic);

            Rectangle pinnedBox = new Rectangle(this.xPositionOnScreen + 44, this.yPositionOnScreen + 130, this.width - 88, 80);
            // Khung viền màu vàng kim hoàng gia cho ô đang trang bị
            IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), pinnedBox.X, pinnedBox.Y, pinnedBox.Width, pinnedBox.Height, new Color(255, 245, 200), 1f, false);

            // Thumbnail tàu
            Texture2D eqTexture = ModEntry.Instance.GetSkinTexture(equippedSkin.Id);
            b.Draw(eqTexture, new Rectangle(pinnedBox.X + 16, pinnedBox.Y + 14, 52, 52), new Rectangle(0, 0, 64, 64), Color.White);

            // Tên Skin Đang Dùng
            Vector2 eqNamePos = new Vector2(pinnedBox.X + 78, pinnedBox.Y + 8);
            b.DrawString(Game1.dialogueFont, equippedSkin.DisplayName, eqNamePos, new Color(130, 70, 0), 0f, Vector2.Zero, 0.65f, SpriteEffects.None, 1f);

            // Nội tại Lõi (Font vừa vặn, không tràn lấn ra mép phải)
            Vector2 eqBuffPos = new Vector2(pinnedBox.X + 78, pinnedBox.Y + 32);
            b.DrawString(Game1.smallFont, $"Hieu ung: {equippedSkin.PassiveBuffDescription}", eqBuffPos, new Color(0, 120, 50), 0f, Vector2.Zero, 0.82f, SpriteEffects.None, 1f);

            // Mô tả (Thu gọn scale 0.75f)
            Vector2 eqDescPos = new Vector2(pinnedBox.X + 78, pinnedBox.Y + 52);
            b.DrawString(Game1.smallFont, equippedSkin.Description, eqDescPos, Color.DimGray, 0f, Vector2.Zero, 0.75f, SpriteEffects.None, 1f);

            // Huy hiệu [ ĐANG TRANG BỊ ]
            string badge = "[Dang su dung]";
            Vector2 badgeSize = Game1.smallFont.MeasureString(badge);
            Vector2 badgePos = new Vector2(pinnedBox.Right - badgeSize.X - 20, pinnedBox.Y + (pinnedBox.Height - badgeSize.Y) / 2);
            b.DrawString(Game1.smallFont, badge, badgePos, new Color(140, 60, 0));

            // ==========================================
            // 2. DANH SÁCH CUỘN CÁC NGOẠI TRANG KHÁC
            // ==========================================
            var otherSkins = this.AllSkins.Where(s => !s.Id.Equals(currentEquippedSkin, StringComparison.OrdinalIgnoreCase)).ToList();
            Rectangle listRect = new Rectangle(this.xPositionOnScreen + 44, this.yPositionOnScreen + 224, this.width - 88, this.height - 250);

            b.End();
            RasterizerState rState = new RasterizerState { ScissorTestEnable = true };
            Rectangle originalScissor = b.GraphicsDevice.ScissorRectangle;
            b.GraphicsDevice.ScissorRectangle = listRect;
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, rState);

            int startY = listRect.Y - (int)this.CurrentScrollY;

            for (int i = 0; i < otherSkins.Count; i++)
            {
                var skin = otherSkins[i];
                int itemY = startY + i * (RowHeightSkin + 10);

                if (itemY + RowHeightSkin < listRect.Y || itemY > listRect.Bottom)
                    continue;

                Rectangle rowBox = new Rectangle(listRect.X, itemY, listRect.Width, RowHeightSkin);

                // Khung viền pill da cam nhạt cho ngoại trang khác
                IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), rowBox.X, rowBox.Y, rowBox.Width, rowBox.Height, new Color(240, 238, 245), 1f, false);

                // Thumbnail tàu thu nhỏ
                Texture2D skinTexture = ModEntry.Instance.GetSkinTexture(skin.Id);
                Vector2 thumbPos = new Vector2(rowBox.X + 16, rowBox.Y + (RowHeightSkin - 48) / 2);
                b.Draw(skinTexture, new Rectangle((int)thumbPos.X, (int)thumbPos.Y, 48, 48), new Rectangle(0, 0, 64, 64), Color.White);

                // Tên Skin
                Vector2 namePos = new Vector2(rowBox.X + 76, rowBox.Y + 8);
                b.DrawString(Game1.dialogueFont, skin.DisplayName, namePos, Game1.textColor, 0f, Vector2.Zero, 0.62f, SpriteEffects.None, 1f);

                // Nội Tại Đặc Quyền của Skin
                Vector2 buffPos = new Vector2(rowBox.X + 76, rowBox.Y + 30);
                b.DrawString(Game1.smallFont, $"Hieu ung: {skin.PassiveBuffDescription}", buffPos, new Color(0, 130, 70), 0f, Vector2.Zero, 0.80f, SpriteEffects.None, 1f);

                // Mô tả Ngoại Trang
                Vector2 descPos = new Vector2(rowBox.X + 76, rowBox.Y + 50);
                b.DrawString(Game1.smallFont, skin.Description, descPos, Color.DimGray, 0f, Vector2.Zero, 0.74f, SpriteEffects.None, 1f);

                // Nút Trang Bị
                string selectBtn = "Trang bị";
                Vector2 btnSize = Game1.smallFont.MeasureString(selectBtn);
                Vector2 btnPos = new Vector2(rowBox.Right - btnSize.X - 24, rowBox.Y + (RowHeightSkin - btnSize.Y) / 2);
                b.DrawString(Game1.smallFont, selectBtn, btnPos, Color.DarkBlue);
            }

            b.End();
            b.GraphicsDevice.ScissorRectangle = originalScissor;
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
        }
        else
        {
            // === VẼ DANH SÁCH BẢN ĐỒ (Pill lớn 84px) ===
            Rectangle listRect = new Rectangle(this.xPositionOnScreen + 44, this.yPositionOnScreen + 130, this.width - 88, this.height - 160);

            b.End();
            RasterizerState rState = new RasterizerState { ScissorTestEnable = true };
            Rectangle originalScissor = b.GraphicsDevice.ScissorRectangle;
            b.GraphicsDevice.ScissorRectangle = listRect;
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, rState);

            int startY = listRect.Y - (int)this.CurrentScrollY;

            for (int i = 0; i < this.FilteredDestinations.Count; i++)
            {
                var dest = this.FilteredDestinations[i];
                int itemY = startY + i * (RowHeightDestination + 10);

                if (itemY + RowHeightDestination < listRect.Y || itemY > listRect.Bottom)
                    continue;

                Rectangle rowBox = new Rectangle(listRect.X, itemY, listRect.Width, RowHeightDestination);
                bool isUnlocked = this.CurrentTier.CanBypassLockedAreas || AreaUnlockHelper.IsDestinationUnlocked(dest, out _);

                Color boxColor = isUnlocked ? new Color(230, 235, 240) : new Color(180, 180, 185);
                IClickableMenu.drawTextureBox(b, Game1.menuTexture, new Rectangle(0, 256, 60, 60), rowBox.X, rowBox.Y, rowBox.Width, rowBox.Height, boxColor, 1f, false);

                // Bullet
                Vector2 bulletPos = new Vector2(rowBox.X + 20, rowBox.Y + (RowHeightDestination - 32) / 2);
                Color bulletColor = isUnlocked ? Color.Gold : Color.DarkGray;
                b.Draw(Game1.mouseCursors, new Rectangle((int)bulletPos.X, (int)bulletPos.Y, 32, 32), new Rectangle(194, 388, 16, 16), bulletColor);

                // Tên điểm đến
                Vector2 namePos = new Vector2(rowBox.X + 68, rowBox.Y + 12);
                Color nameColor = isUnlocked ? Game1.textColor : Color.DarkRed;
                string displayName = isUnlocked ? dest.DisplayName : $"{dest.DisplayName} (CHƯA MỞ KHÓA)";
                b.DrawString(Game1.dialogueFont, displayName, namePos, nameColor, 0f, Vector2.Zero, 0.75f, SpriteEffects.None, 1f);

                // Mô tả & Tọa độ tùy chỉnh nếu có
                string descText = dest.Description;
                if (ModEntry.Instance.Config.CustomLandingTiles.TryGetValue(dest.LocationName, out string? customPt))
                {
                    descText = $"[Toa do da luu: ({customPt})] - {dest.Description}";
                }
                Vector2 descPos = new Vector2(rowBox.X + 68, rowBox.Y + 48);
                b.DrawString(Game1.smallFont, descText, descPos, ModEntry.Instance.Config.CustomLandingTiles.ContainsKey(dest.LocationName) ? new Color(0, 110, 60) : Color.DimGray, 0f, Vector2.Zero, 0.82f, SpriteEffects.None, 1f);

                // Mũi tên
                Vector2 arrowPos = new Vector2(rowBox.Right - 46, rowBox.Y + (RowHeightDestination - 24) / 2);
                if (isUnlocked)
                {
                    b.Draw(Game1.mouseCursors, arrowPos, new Rectangle(365, 495, 12, 11), Color.White, 0f, Vector2.Zero, 2.5f, SpriteEffects.None, 1f);
                }
            }

            b.End();
            b.GraphicsDevice.ScissorRectangle = originalScissor;
            b.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
        }

        this.drawMouse(b);
    }
}
