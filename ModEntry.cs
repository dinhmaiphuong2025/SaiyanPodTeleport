using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using StardewValley.GameData.Objects;
using StardewValley.GameData.Shops;
using StardewValley.Menus;

namespace SaiyanPodTeleport;

public class ModEntry : Mod
{
    public static ModEntry Instance { get; private set; } = null!;
    public ModConfig Config { get; private set; } = new();

    private Texture2D PodTexture = null!;
    private Texture2D FlameTexture = null!;
    private Texture2D ShadowTexture = null!;

    // Cache các Texture của từng Skin và Lửa tương ứng
    private readonly Dictionary<string, Texture2D> SkinTextures = new();
    private readonly Dictionary<string, Texture2D> SkinFlameTextures = new();

    // Bộ 4 texture icon cho 4 bậc Remote
    private Texture2D RemoteBaseTexture = null!;
    private Texture2D RemotePlusTexture = null!;
    private Texture2D RemoteProTexture = null!;
    private Texture2D RemoteProMaxTexture = null!;

    private readonly PodAnimationState AnimState = new();
    private readonly List<TeleportDestination> Destinations = new();

    // Quản lý số lần sử dụng trong ngày: Day -> count
    private int CurrentDay = -1;
    private int DailyUsesCount = 0;

    public override void Entry(IModHelper helper)
    {
        Instance = this;
        this.Config = helper.ReadConfig<ModConfig>();

        this.LoadTextures();
        this.RegisterDestinations();

        // SMAPI Events
        helper.Events.Content.AssetRequested += this.OnAssetRequested;
        helper.Events.GameLoop.SaveLoaded += this.OnSaveLoaded;
        helper.Events.GameLoop.DayStarted += this.OnDayStarted;
        helper.Events.GameLoop.UpdateTicked += this.OnUpdateTicked;
        helper.Events.GameLoop.OneSecondUpdateTicked += this.OnOneSecondUpdateTicked;
        helper.Events.Display.RenderedWorld += this.OnRenderedWorld;
        helper.Events.Input.ButtonPressed += this.OnButtonPressed;
    }

    private void LoadTextures()
    {
        this.FlameTexture = this.Helper.ModContent.Load<Texture2D>("assets/thruster_flame.png");
        this.ShadowTexture = this.Helper.ModContent.Load<Texture2D>("assets/pod_shadow.png");

        // Load tất cả các bộ skin và lửa riêng biệt
        this.SkinTextures.Clear();
        this.SkinFlameTextures.Clear();
        foreach (var skin in PodSkinInfo.AllSkins)
        {
            try
            {
                this.SkinTextures[skin.Id] = this.Helper.ModContent.Load<Texture2D>(skin.AssetPath);
                this.SkinFlameTextures[skin.Id] = this.Helper.ModContent.Load<Texture2D>(skin.FlameAssetPath);
            }
            catch (Exception ex)
            {
                this.Monitor.Log($"Không thể tải skin/lửa {skin.Id}: {ex.Message}", LogLevel.Warn);
            }
        }

        // Texture tàu hiện tại theo Config
        this.UpdateCurrentSkinTexture();

        this.RemoteBaseTexture = this.Helper.ModContent.Load<Texture2D>("assets/remote_base.png");
        this.RemotePlusTexture = this.Helper.ModContent.Load<Texture2D>("assets/remote_plus.png");
        this.RemoteProTexture = this.Helper.ModContent.Load<Texture2D>("assets/remote_pro.png");
        this.RemoteProMaxTexture = this.Helper.ModContent.Load<Texture2D>("assets/remote_promax.png");
    }

    public void UpdateCurrentSkinTexture()
    {
        string skinId = this.Config.CurrentSkin;
        if (this.SkinTextures.TryGetValue(skinId, out var tex))
            this.PodTexture = tex;
        else if (this.SkinTextures.TryGetValue(PodSkinInfo.Classic.Id, out var classicTex))
            this.PodTexture = classicTex;

        if (this.SkinFlameTextures.TryGetValue(skinId, out var fTex))
            this.FlameTexture = fTex;
        else if (this.SkinFlameTextures.TryGetValue(PodSkinInfo.Classic.Id, out var classicFTex))
            this.FlameTexture = classicFTex;
    }

    public Texture2D GetSkinTexture(string skinId)
    {
        if (this.SkinTextures.TryGetValue(skinId, out var tex))
            return tex;
        return this.PodTexture;
    }

    private void RegisterDestinations()
    {
        this.Destinations.Clear();

        // 1. Nông trại
        this.Destinations.Add(new TeleportDestination("farm_house", "Trước Nhà Chính", "Nông trại", "Farm", 64, 18, "Sân trước nhà nông trại"));
        this.Destinations.Add(new TeleportDestination("farm_greenhouse", "Nhà Kính", "Nông trại", "Farm", 28, 17, "Trước cửa nhà kính nông trại"));

        // 2. Thung lũng Pelican
        this.Destinations.Add(new TeleportDestination("town_square", "Quảng Trường Pelican", "Thung lũng", "Town", 42, 57, "Trung tâm thị trấn Pelican"));
        this.Destinations.Add(new TeleportDestination("town_pierres", "Tiệm Tạp Hóa Pierre", "Thung lũng", "Town", 44, 59, "Cửa tiệm hạt giống Pierre"));
        this.Destinations.Add(new TeleportDestination("beach_dock", "Bãi Biển & Cửa Hàng Willy", "Thung lũng", "Beach", 20, 8, "Bãi cát khô ráo bãi biển phía nam"));
        this.Destinations.Add(new TeleportDestination("mountain_carpenter", "Xưởng Mộc Robin", "Thung lũng", "Mountain", 12, 28, "Sân trước xưởng mộc trên núi"));
        this.Destinations.Add(new TeleportDestination("mountain_mines", "Hiệp Hội Mạo Hiểm & Hang Mỏ", "Thung lũng", "Mountain", 74, 12, "Sân trước nhà Hiệp hội Mạo hiểm giả"));
        this.Destinations.Add(new TeleportDestination("forest_marnie", "Trang Trại Marnie", "Thung lũng", "Forest", 83, 21, "Sân rộng trước nhà trang trại Marnie"));
        this.Destinations.Add(new TeleportDestination("forest_wizard", "Tháp Pháp Sư", "Thung lũng", "Forest", 5, 35, "Góc tây nam rừng Cindersap"));

        // 3. Đảo & Sa mạc
        this.Destinations.Add(new TeleportDestination("desert_oasis", "Sa Mạc Calico", "Đảo & Sa mạc", "Desert", 35, 43, "Khu đất cát bằng phẳng sa mạc Calico"));
        this.Destinations.Add(new TeleportDestination("island_south", "Bến Tàu Đảo Ginger", "Đảo & Sa mạc", "IslandSouth", 14, 22, "Bãi cát đảo Ginger thoáng đãng"));
    }

    private void OnAssetRequested(object? sender, AssetRequestedEventArgs e)
    {
        // 1. Cung cấp Texture icon cho 4 bậc Remote
        if (e.NameWithoutLocale.IsEquivalentTo("Mods/dinhmaiphuong2025.SaiyanPod/RemoteBaseIcon"))
            e.LoadFrom(() => this.RemoteBaseTexture, AssetLoadPriority.Medium);
        else if (e.NameWithoutLocale.IsEquivalentTo("Mods/dinhmaiphuong2025.SaiyanPod/RemotePlusIcon"))
            e.LoadFrom(() => this.RemotePlusTexture, AssetLoadPriority.Medium);
        else if (e.NameWithoutLocale.IsEquivalentTo("Mods/dinhmaiphuong2025.SaiyanPod/RemoteProIcon"))
            e.LoadFrom(() => this.RemoteProTexture, AssetLoadPriority.Medium);
        else if (e.NameWithoutLocale.IsEquivalentTo("Mods/dinhmaiphuong2025.SaiyanPod/RemoteProMaxIcon") ||
                 e.NameWithoutLocale.IsEquivalentTo("Mods/dinhmaiphuong2025.SaiyanPod/RemoteIcon"))
            e.LoadFrom(() => this.RemoteProMaxTexture, AssetLoadPriority.Medium);

        // 2. Đăng ký cả 4 Vật phẩm vào Data/Objects
        if (e.NameWithoutLocale.IsEquivalentTo("Data/Objects"))
        {
            e.Edit(editor =>
            {
                var dict = editor.AsDictionary<string, ObjectData>().Data;

                dict[RemoteTierInfo.BaseInfo.ItemId] = new ObjectData
                {
                    Name = RemoteTierInfo.BaseInfo.DisplayName,
                    DisplayName = RemoteTierInfo.BaseInfo.DisplayName,
                    Description = RemoteTierInfo.BaseInfo.Description,
                    Type = "Basic",
                    Category = StardewValley.Object.metalResources,
                    Price = 1000,
                    Texture = "Mods/dinhmaiphuong2025.SaiyanPod/RemoteBaseIcon",
                    SpriteIndex = 0,
                    ContextTags = new List<string> { "saiyan_remote", "saiyan_remote_base" }
                };

                dict[RemoteTierInfo.PlusInfo.ItemId] = new ObjectData
                {
                    Name = RemoteTierInfo.PlusInfo.DisplayName,
                    DisplayName = RemoteTierInfo.PlusInfo.DisplayName,
                    Description = RemoteTierInfo.PlusInfo.Description,
                    Type = "Basic",
                    Category = StardewValley.Object.metalResources,
                    Price = 3000,
                    Texture = "Mods/dinhmaiphuong2025.SaiyanPod/RemotePlusIcon",
                    SpriteIndex = 0,
                    ContextTags = new List<string> { "saiyan_remote", "saiyan_remote_plus" }
                };

                dict[RemoteTierInfo.ProInfo.ItemId] = new ObjectData
                {
                    Name = RemoteTierInfo.ProInfo.DisplayName,
                    DisplayName = RemoteTierInfo.ProInfo.DisplayName,
                    Description = RemoteTierInfo.ProInfo.Description,
                    Type = "Basic",
                    Category = StardewValley.Object.metalResources,
                    Price = 10000,
                    Texture = "Mods/dinhmaiphuong2025.SaiyanPod/RemoteProIcon",
                    SpriteIndex = 0,
                    ContextTags = new List<string> { "saiyan_remote", "saiyan_remote_pro" }
                };

                dict[RemoteTierInfo.ProMaxInfo.ItemId] = new ObjectData
                {
                    Name = RemoteTierInfo.ProMaxInfo.DisplayName,
                    DisplayName = RemoteTierInfo.ProMaxInfo.DisplayName,
                    Description = RemoteTierInfo.ProMaxInfo.Description,
                    Type = "Basic",
                    Category = StardewValley.Object.metalResources,
                    Price = 20000,
                    Texture = "Mods/dinhmaiphuong2025.SaiyanPod/RemoteProMaxIcon",
                    SpriteIndex = 0,
                    ContextTags = new List<string> { "saiyan_remote", "saiyan_remote_promax" }
                };

                // Giữ ItemId cũ trỏ tới ProMax cho tương thích
                dict["dinhmaiphuong2025.SaiyanRemote"] = dict[RemoteTierInfo.ProMaxInfo.ItemId];
            });
        }
    }

    private void OnDayStarted(object? sender, DayStartedEventArgs e)
    {
        // Reset số lượt dùng khi bắt đầu ngày mới
        this.CurrentDay = Game1.Date.TotalDays;
        this.DailyUsesCount = 0;
    }

    private void OnSaveLoaded(object? sender, SaveLoadedEventArgs e)
    {
        this.CurrentDay = Game1.Date.TotalDays;

        if (this.Config.AutoGrantItemOnLoad && Context.IsWorldReady)
        {
            // Nhét luôn cả 4 bản vào balo cho người chơi test thoải mái
            string[] allTiers = new[]
            {
                RemoteTierInfo.BaseInfo.ItemId,
                RemoteTierInfo.PlusInfo.ItemId,
                RemoteTierInfo.ProInfo.ItemId,
                RemoteTierInfo.ProMaxInfo.ItemId
            };

            foreach (var itemId in allTiers)
            {
                bool hasItem = Game1.player.Items.Any(it => it != null && (it.ItemId == itemId || it.QualifiedItemId == $"(O){itemId}"));
                if (!hasItem)
                {
                    var remoteItem = ItemRegistry.Create($"(O){itemId}");
                    if (Game1.player.addItemToInventoryBool(remoteItem))
                    {
                        var info = RemoteTierInfo.ByItemId[itemId];
                        Game1.addHUDMessage(new HUDMessage($"Đã nhận: {info.DisplayName}!", HUDMessage.newQuest_type));
                    }
                }
            }
        }
    }


    private void OnOneSecondUpdateTicked(object? sender, OneSecondUpdateTickedEventArgs e)
    {
        if (!Context.IsWorldReady || Game1.player == null)
            return;

        // Kiểm tra xem trong 12 ô đầu (Toolbar) có bất kỳ Saiyan Remote nào không
        bool hasRemoteInToolbar = false;
        for (int i = 0; i < Math.Min(12, Game1.player.Items.Count); i++)
        {
            var item = Game1.player.Items[i];
            if (item != null && RemoteTierInfo.ByItemId.ContainsKey(item.ItemId))
            {
                hasRemoteInToolbar = true;
                break;
            }
        }

        string buffId = "dinhmaiphuong2025.SaiyanPod.SkinBuff";

        if (!hasRemoteInToolbar)
        {
            // Nếu không để trên Toolbar thì gỡ Buff
            Game1.player.buffs.Remove(buffId);
            return;
        }

        // 1. KHÔNG CỘNG DỒN NHIỀU REMOTE: CHỈ LẤY 1 REMOTE CÓ CẤP ĐỘ CAO NHẤT TRÊN TOOLBAR
        // (Để dành cơ chế cộng dồn kích hoạt theo bộ set skin hoàn chỉnh cho các item sau này)
        RemoteTier highestTier = RemoteTier.Base;
        bool hasAnyRemote = false;

        for (int i = 0; i < Math.Min(12, Game1.player.Items.Count); i++)
        {
            var item = Game1.player.Items[i];
            if (item != null && RemoteTierInfo.ByItemId.TryGetValue(item.ItemId, out var tInfo))
            {
                hasAnyRemote = true;
                if (tInfo.Tier > highestTier)
                    highestTier = tInfo.Tier;
            }
        }

        float remoteSpeedBonus = hasAnyRemote ? highestTier switch
        {
            RemoteTier.ProMax => 3.0f,
            RemoteTier.Pro => 2.0f,
            RemoteTier.Plus => 1.0f,
            _ => 0.5f
        } : 0f;

        // 2. NỘI TẠI MẠNH MẼ ĐẶC QUYỀN RIÊNG DÀNH CHO TỪNG SKIN
        string skinId = this.Config.CurrentSkin;
        var skinInfo = PodSkinInfo.ById.GetValueOrDefault(skinId, PodSkinInfo.Classic);

        var effects = new StardewValley.Buffs.BuffEffects();

        if (remoteSpeedBonus > 0f)
        {
            effects.Speed.Value = remoteSpeedBonus;
        }

        switch (skinInfo.Id)
        {
            case "classic":
                // Thép Saiyan Cổ Điển: Thiết Giáp Chiến Binh (+5 Phòng thủ, +2 Kháng hiệu ứng)
                effects.Defense.Value = 5f;
                effects.Immunity.Value = 2f;
                break;

            case "capsule":
                // Capsule Corp: Lõi Lượng Tử Nano (Tự động hồi 2.5 Thể Lực và 1 Máu mỗi giây)
                if (Game1.player.Stamina < Game1.player.MaxStamina)
                {
                    Game1.player.Stamina = Math.Min(Game1.player.MaxStamina, Game1.player.Stamina + 2.5f);
                }
                if (Game1.player.health < Game1.player.maxHealth)
                {
                    Game1.player.health = Math.Min(Game1.player.maxHealth, Game1.player.health + 1);
                }
                break;

            case "obsidian":
                // Hắc Diện Tàng Hình: Chiến Thần Hư Không (+8 Sát Thương Tấn Công, +3 Phòng thủ)
                effects.Attack.Value = 8f;
                effects.Defense.Value = 3f;
                break;

            case "golden":
                // Hoàng Kim Đế Vương: Chân Mệnh Đế Vương (+2 Vận May Hoàng Kim, +128 Nam Châm Siêu Hút)
                effects.LuckLevel.Value = 2f;
                effects.MagneticRadius.Value = 128f;
                break;
        }

        string buffDesc = remoteSpeedBonus > 0 
            ? $"{skinInfo.PassiveBuffDescription} | Động cơ Remote: +{remoteSpeedBonus} Speed" 
            : skinInfo.PassiveBuffDescription;

        var activeBuff = new StardewValley.Buff(
            id: buffId,
            displayName: $"Lõi {skinInfo.DisplayName}",
            description: buffDesc,
            iconTexture: this.PodTexture,
            iconSheetIndex: 0,
            duration: 2500, // Làm mới mỗi giây
            effects: effects
        );

        Game1.player.applyBuff(activeBuff);
    }

    private void OnButtonPressed(object? sender, ButtonPressedEventArgs e)
    {
        if (!Context.IsPlayerFree)
            return;

        // Bấm sử dụng khi đang cầm một trong 4 loại Saiyan Remote
        if (e.Button.IsUseToolButton() || e.Button.IsActionButton())
        {
            var activeItem = Game1.player.CurrentItem;
            if (activeItem != null)
            {
                string rawId = activeItem.ItemId;
                if (RemoteTierInfo.ByItemId.TryGetValue(rawId, out var tierInfo))
                {
                    this.Helper.Input.Suppress(e.Button);
                    this.TryOpenPodMenu(tierInfo);
                    return;
                }
            }
        }

        // Tương tác trực tiếp với Thương Nhân Thời Không (ChronoMerchant)
        if (e.Button.IsActionButton() || e.Button.IsUseToolButton() || e.Button == SButton.MouseLeft || e.Button == SButton.MouseRight)
        {
            if (Context.IsWorldReady && Game1.currentLocation != null && Game1.activeClickableMenu == null)
            {
                var merchant = Game1.currentLocation.getCharacterFromName("ChronoMerchant");
                if (merchant != null)
                {
                    Vector2 grabTile = Game1.player.GetGrabTile();
                    float dist = Vector2.Distance(Game1.player.Tile, merchant.Tile);
                    if (dist <= 3.2f || grabTile == merchant.Tile)
                    {
                        this.Helper.Input.Suppress(e.Button);
                        this.Monitor.Log($"Tương tác với ChronoMerchant (khoảng cách: {dist:F1}), mở ChronoShop...", LogLevel.Info);
                        Utility.TryOpenShopMenu("ChronoShop", "ChronoMerchant");
                        return;
                    }
                }
            }
        }
    }

    private void TryOpenPodMenu(RemoteTierInfo tier)
    {
        if (this.AnimState.Phase != PodSequencePhase.None)
            return;

        // Kiểm tra xem đã sang ngày mới chưa
        if (this.CurrentDay != Game1.Date.TotalDays)
        {
            this.CurrentDay = Game1.Date.TotalDays;
            this.DailyUsesCount = 0;
        }

        // Kiểm tra giới hạn số lần sử dụng trong ngày
        if (tier.DailyLimit != -1 && this.DailyUsesCount >= tier.DailyLimit)
        {
            Game1.playSound("cancel");
            Game1.addHUDMessage(new HUDMessage($"{tier.DisplayName} đã cạn năng lượng hôm nay ({this.DailyUsesCount}/{tier.DailyLimit} lần)! Cần sạc lại vào ngày mai.", HUDMessage.error_type));
            return;
        }

        int remainingUses = (tier.DailyLimit == -1) ? -1 : (tier.DailyLimit - this.DailyUsesCount);

        Game1.playSound("bigSelect");
        Game1.activeClickableMenu = new PodDestinationMenu(
            this.Destinations,
            tier,
            remainingUses,
            dest =>
            {
                // Tăng số lần sử dụng trong ngày
                if (tier.DailyLimit != -1)
                    this.DailyUsesCount++;

                this.StartPodSequence(dest, tier);
            },
            newSkinId =>
            {
                // Đổi skin ngoại trang và lưu lại
                this.Config.CurrentSkin = newSkinId;
                this.Helper.WriteConfig(this.Config);
                this.UpdateCurrentSkinTexture();

                if (PodSkinInfo.ById.TryGetValue(newSkinId, out var skinInfo))
                {
                    Game1.addHUDMessage(new HUDMessage($"Đã trang bị ngoại trang: {skinInfo.DisplayName}!", HUDMessage.newQuest_type));
                }
            }
        );
    }

    public void SetCustomLandingForCurrentLocation()
    {
        if (Game1.currentLocation == null || Game1.player == null)
            return;

        string locName = Game1.currentLocation.Name;
        int tileX = (int)Game1.player.Tile.X;
        int tileY = (int)Game1.player.Tile.Y;

        // Lưu theo tên LocationName chuẩn
        this.Config.CustomLandingTiles[locName] = $"{tileX},{tileY}";

        // Đồng thời nếu đang ở map có các điểm đến cụ thể (như Farm, Town, Mountain...) ghim luôn theo ID điểm đến gần nhất hoặc theo tên
        this.Helper.WriteConfig(this.Config);

        Game1.playSound("coin");
        Game1.addHUDMessage(new HUDMessage($"Da luu diem dap tai {locName}: ({tileX}, {tileY})", HUDMessage.newQuest_type));
    }

    public void ResetCustomLandingForCurrentLocation()
    {
        if (Game1.currentLocation == null)
            return;

        string locName = Game1.currentLocation.Name;
        if (this.Config.CustomLandingTiles.Remove(locName))
        {
            this.Helper.WriteConfig(this.Config);
            Game1.playSound("trashcan");
            Game1.addHUDMessage(new HUDMessage($"Da khoi phuc diem dap mac dinh cho {locName}.", HUDMessage.error_type));
        }
    }

    private void StartPodSequence(TeleportDestination destination, RemoteTierInfo tier)
    {
        this.AnimState.ActiveTier = tier;
        this.AnimState.PendingDestination = destination;
        this.AnimState.Phase = PodSequencePhase.Boarding_Fall;
        this.AnimState.PhaseTimer = 0f;

        // Vị trí tàu đáp xuống phía sau Farmer (Y - 72px)
        Vector2 farmerTile = Game1.player.Tile;
        this.AnimState.TargetTilePosition = new Vector2(farmerTile.X * 64f + 32f, farmerTile.Y * 64f - 72f);

        // Bắt đầu từ độ cao 1200px bên trên
        this.AnimState.PodCurrentWorldPos = new Vector2(this.AnimState.TargetTilePosition.X, this.AnimState.TargetTilePosition.Y - 1200f);
        this.AnimState.HatchFrame = 0;
        this.AnimState.ShowFlame = true;

        this.AnimState.FarmerInsidePod = false;
        this.AnimState.FarmerOriginalPos = Game1.player.Position;
        this.AnimState.FarmerStepOffset = Vector2.Zero;
        this.AnimState.FarmerScale = 1f;

        // Ẩn UI & Khóa camera
        Game1.displayHUD = false;
        Game1.viewportFreeze = true;

        Game1.player.freezePause = 99999;
        Game1.player.CanMove = false;
        Game1.player.faceDirection(0); // Quay mặt lên trên vào tàu
        Game1.playSound("furnace");
    }

    private void OnUpdateTicked(object? sender, UpdateTickedEventArgs e)
    {
        if (!Context.IsWorldReady || this.AnimState.Phase == PodSequencePhase.None)
            return;

        var tier = this.AnimState.ActiveTier;

        // Khóa di chuyển người chơi, ẩn HUD và giữ camera
        Game1.player.freezePause = 1000;
        Game1.player.CanMove = false;
        Game1.displayHUD = false;
        Game1.viewportFreeze = true;

        float dt = 0.0166f;
        this.AnimState.PhaseTimer += dt;

        // Flicker ngọn lửa phản lực
        this.AnimState.FlameAnimTimer += dt;
        if (this.AnimState.FlameAnimTimer >= 0.05f)
        {
            this.AnimState.FlameAnimTimer = 0f;
            this.AnimState.FlameFrame = (this.AnimState.FlameFrame + 1) % 3;
        }

        switch (this.AnimState.Phase)
        {
            case PodSequencePhase.Boarding_Fall:
                // Tốc độ hạ cánh lấy theo từng bậc Remote Tier
                float fallSpeed = tier.DescentSpeed;
                float newY = this.AnimState.PodCurrentWorldPos.Y + fallSpeed;
                if (newY >= this.AnimState.TargetTilePosition.Y)
                {
                    this.AnimState.PodCurrentWorldPos = this.AnimState.TargetTilePosition;
                    this.AnimState.Phase = PodSequencePhase.Boarding_Impact;
                    this.AnimState.PhaseTimer = 0f;
                    this.AnimState.ShowFlame = false;

                    if (this.Config.ScreenShakeEnabled)
                        Game1.flashAlpha = 0.35f;

                    Game1.playSound("explosion");
                    this.SpawnImpactDust(this.AnimState.TargetTilePosition);
                }
                else
                {
                    this.AnimState.PodCurrentWorldPos = new Vector2(this.AnimState.TargetTilePosition.X, newY);
                }
                break;

            case PodSequencePhase.Boarding_Impact:
                float impactWait = (tier.Tier == RemoteTier.Base) ? 0.9f : (tier.Tier == RemoteTier.Plus ? 0.65f : 0.4f);
                if (this.AnimState.PhaseTimer >= impactWait)
                {
                    this.AnimState.Phase = PodSequencePhase.Boarding_Opening;
                    this.AnimState.PhaseTimer = 0f;
                    Game1.playSound("steam");
                }
                break;

            case PodSequencePhase.Boarding_Opening:
                float hatchSpeed = (tier.Tier == RemoteTier.Base) ? 0.32f : (tier.Tier == RemoteTier.Plus ? 0.22f : 0.12f);
                int openFrame = (int)(this.AnimState.PhaseTimer / hatchSpeed);
                if (openFrame >= 3)
                {
                    this.AnimState.HatchFrame = 3;
                    this.AnimState.Phase = PodSequencePhase.Boarding_Enter;
                    this.AnimState.PhaseTimer = 0f;
                }
                else
                {
                    this.AnimState.HatchFrame = Math.Clamp(openFrame, 0, 3);
                }
                break;

            case PodSequencePhase.Boarding_Enter:
                Game1.player.faceDirection(0);
                float totalBoardTime = tier.BoardingDuration;
                float enterProgress = Math.Clamp(this.AnimState.PhaseTimer / totalBoardTime, 0f, 1f);

                Game1.player.Position = this.AnimState.FarmerOriginalPos + new Vector2(0f, -enterProgress * 64f);

                if (enterProgress >= 0.85f)
                {
                    this.AnimState.FarmerInsidePod = true;
                    Game1.player.Position = new Vector2(-99999f, -99999f);
                }

                if (this.AnimState.PhaseTimer >= totalBoardTime)
                {
                    this.AnimState.HatchFrame = 0;
                    Game1.playSound("doorClose");
                    this.AnimState.Phase = PodSequencePhase.Boarding_Ascent;
                    this.AnimState.PhaseTimer = 0f;
                    this.AnimState.ShowFlame = true;
                    Game1.playSound("flame_ring");
                }
                break;

            case PodSequencePhase.Boarding_Ascent:
                Game1.player.Position = new Vector2(-99999f, -99999f);

                float launchSpeed = tier.AscentSpeed + this.AnimState.PhaseTimer * 25f;
                this.AnimState.PodCurrentWorldPos = new Vector2(this.AnimState.PodCurrentWorldPos.X, this.AnimState.PodCurrentWorldPos.Y - launchSpeed);

                if (this.AnimState.PhaseTimer >= 0.85f)
                {
                    this.AnimState.Phase = PodSequencePhase.Fade_Transition;
                    this.AnimState.PhaseTimer = 0f;
                    Game1.globalFadeToBlack(() =>
                    {
                        this.ExecuteMapWarp();
                    }, 0.025f);
                }
                break;

            case PodSequencePhase.Fade_Transition:
                Game1.player.Position = new Vector2(-99999f, -99999f);
                break;

            case PodSequencePhase.Arrival_Fall:
                Game1.player.Position = new Vector2(-99999f, -99999f);

                float arrFallSpeed = tier.DescentSpeed + 4f;
                float arrNewY = this.AnimState.PodCurrentWorldPos.Y + arrFallSpeed;
                if (arrNewY >= this.AnimState.TargetTilePosition.Y)
                {
                    this.AnimState.PodCurrentWorldPos = this.AnimState.TargetTilePosition;
                    this.AnimState.Phase = PodSequencePhase.Arrival_Impact;
                    this.AnimState.PhaseTimer = 0f;
                    this.AnimState.ShowFlame = false;

                    if (this.Config.ScreenShakeEnabled)
                        Game1.flashAlpha = 0.35f;

                    Game1.playSound("explosion");
                    this.SpawnImpactDust(this.AnimState.TargetTilePosition);
                }
                else
                {
                    this.AnimState.PodCurrentWorldPos = new Vector2(this.AnimState.TargetTilePosition.X, arrNewY);
                }
                break;

            case PodSequencePhase.Arrival_Impact:
                Game1.player.Position = new Vector2(-99999f, -99999f);

                float arrImpactWait = (tier.Tier == RemoteTier.Base) ? 0.9f : (tier.Tier == RemoteTier.Plus ? 0.65f : 0.4f);
                if (this.AnimState.PhaseTimer >= arrImpactWait)
                {
                    this.AnimState.Phase = PodSequencePhase.Arrival_Opening;
                    this.AnimState.PhaseTimer = 0f;
                    Game1.playSound("steam"); // Xả khí nén
                }
                break;

            case PodSequencePhase.Arrival_Opening:
                Game1.player.Position = new Vector2(-99999f, -99999f);

                float arrHatchSpeed = (tier.Tier == RemoteTier.Base) ? 0.32f : (tier.Tier == RemoteTier.Plus ? 0.22f : 0.12f);
                int arrOpenFrame = (int)(this.AnimState.PhaseTimer / arrHatchSpeed);
                if (arrOpenFrame >= 3)
                {
                    this.AnimState.HatchFrame = 3;
                    this.AnimState.Phase = PodSequencePhase.Arrival_Exit;
                    this.AnimState.PhaseTimer = 0f;
                }
                else
                {
                    this.AnimState.HatchFrame = Math.Clamp(arrOpenFrame, 0, 3);
                }
                break;

            case PodSequencePhase.Arrival_Exit:
                float totalExitTime = tier.ExitDuration;
                float exitProgress = Math.Clamp(this.AnimState.PhaseTimer / totalExitTime, 0f, 1f);

                this.AnimState.FarmerInsidePod = false;
                Vector2 exitOrigin = new Vector2(this.AnimState.TargetTilePosition.X - 32f, this.AnimState.TargetTilePosition.Y);
                Game1.player.Position = exitOrigin + new Vector2(0f, exitProgress * 64f);
                Game1.player.faceDirection(2);

                if (this.AnimState.PhaseTimer >= totalExitTime)
                {
                    this.AnimState.HatchFrame = 0;
                    Game1.playSound("doorClose");
                    this.AnimState.Phase = PodSequencePhase.Departure_Ascent;
                    this.AnimState.PhaseTimer = 0f;
                    this.AnimState.ShowFlame = true;
                    Game1.playSound("flame_ring");
                }
                break;

            case PodSequencePhase.Departure_Ascent:
                float depSpeed = tier.AscentSpeed + this.AnimState.PhaseTimer * 25f;
                this.AnimState.PodCurrentWorldPos = new Vector2(this.AnimState.PodCurrentWorldPos.X, this.AnimState.PodCurrentWorldPos.Y - depSpeed);

                if (this.AnimState.PhaseTimer >= 0.9f)
                {
                    this.AnimState.Phase = PodSequencePhase.None;
                    this.AnimState.ShowFlame = false;
                    this.AnimState.FarmerInsidePod = false;

                    Game1.displayHUD = true;
                    Game1.viewportFreeze = false;

                    Game1.player.freezePause = 0;
                    Game1.player.CanMove = true;
                    Game1.playSound("money");
                }
                break;
        }
    }

    private void ExecuteMapWarp()
    {
        var dest = this.AnimState.PendingDestination;
        if (dest == null)
            return;

        int targetX = dest.TileX;
        int targetY = dest.TileY;

        // Ưu tiên kiểm tra tọa độ ghim theo Id điểm đến trước, sau đó tới LocationName
        if (this.Config.CustomLandingTiles.TryGetValue(dest.Id, out string? customCoords) ||
            this.Config.CustomLandingTiles.TryGetValue(dest.LocationName, out customCoords))
        {
            if (!string.IsNullOrEmpty(customCoords))
            {
                string[] parts = customCoords.Split(',');
                if (parts.Length == 2 && int.TryParse(parts[0], out int cx) && int.TryParse(parts[1], out int cy))
                {
                    targetX = cx;
                    targetY = cy;
                }
            }
        }

        // Warp farmer tới đúng tọa độ đã ghim
        Game1.warpFarmer(dest.LocationName, targetX, targetY, 2);
        Game1.player.Position = new Vector2(-99999f, -99999f);

        // Đặt mục tiêu tiếp đất của Saiyan Pod chính xác tại tọa độ đã ghim
        this.AnimState.TargetTilePosition = new Vector2(targetX * 64f + 32f, targetY * 64f + 8f);
        this.AnimState.PodCurrentWorldPos = new Vector2(this.AnimState.TargetTilePosition.X, this.AnimState.TargetTilePosition.Y - 1200f);
        this.AnimState.HatchFrame = 0;
        this.AnimState.ShowFlame = true;
        this.AnimState.Phase = PodSequencePhase.Arrival_Fall;
        this.AnimState.PhaseTimer = 0f;

        Game1.displayHUD = false;
        Game1.viewportFreeze = true;

        Game1.globalFadeToClear();
    }

    private void SpawnImpactDust(Vector2 worldPos)
    {
        string skinId = this.Config.CurrentSkin;

        // 1. Khói bụi va chạm cơ bản
        for (int i = 0; i < 14; i++)
        {
            float angle = (float)(Game1.random.NextDouble() * Math.PI * 2);
            float speed = (float)(Game1.random.NextDouble() * 3.5f + 1.5f);
            Vector2 motion = new Vector2((float)Math.Cos(angle) * speed, (float)Math.Sin(angle) * speed);

            Color dustColor = skinId switch
            {
                "capsule" => Color.Cyan * 0.85f,
                "obsidian" => new Color(170, 70, 255) * 0.85f,
                "golden" => Color.Gold * 0.9f,
                _ => Color.White * 0.8f
            };

            Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(
                @"LooseSprites\Cursors",
                new Rectangle(464, 179, 16, 16),
                worldPos + new Vector2(Game1.random.Next(-24, 24), Game1.random.Next(-8, 8)),
                false,
                0f,
                dustColor)
            {
                scale = 3.5f,
                motion = motion,
                interval = 60f,
                animationLength = 4,
                alphaFade = 0.04f,
                layerDepth = 0.999f
            });
        }

        // 2. Hiệu ứng hạt tiếp đất đặc quyền theo từng Skin (Nano, Tinh Không, Sấm Sét Hư Không)
        if (skinId == "capsule")
        {
            // Hiệu ứng hạt Nano / Lưới điện từ Cyan phát tán trên mặt đất
            for (int j = 0; j < 10; j++)
            {
                Vector2 sparkOffset = new Vector2(Game1.random.Next(-48, 48), Game1.random.Next(-20, 20));
                Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(
                    362,
                    70f,
                    6,
                    1,
                    worldPos + sparkOffset,
                    flicker: false,
                    flipped: false)
                {
                    scale = 1.4f,
                    color = Color.Cyan * 0.9f,
                    layerDepth = 0.9995f
                });
            }
        }
        else if (skinId == "obsidian")
        {
            // Sương mù hư không & Lôi điện bóng tối tím bùng nổ
            for (int k = 0; k < 12; k++)
            {
                float rot = (float)(Game1.random.NextDouble() * Math.PI * 2);
                Vector2 sparkOffset = new Vector2((float)Math.Cos(rot) * Game1.random.Next(10, 50), (float)Math.Sin(rot) * Game1.random.Next(5, 25));
                Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(
                    @"LooseSprites\Cursors",
                    new Rectangle(366, 409, 16, 16),
                    worldPos + sparkOffset,
                    false,
                    0f,
                    new Color(210, 80, 255) * 0.9f)
                {
                    scale = 2.4f,
                    interval = 70f,
                    animationLength = 5,
                    alphaFade = 0.035f,
                    layerDepth = 0.9995f
                });
            }
        }
        else if (skinId == "golden")
        {
            // Sóng xung kích hào quang vàng rực & Hạt bụi sao hoàng kim
            for (int g = 0; g < 16; g++)
            {
                float speed = (float)(Game1.random.NextDouble() * 4.0f + 2.0f);
                float angle = (float)(Game1.random.NextDouble() * Math.PI * 2);
                Vector2 gMotion = new Vector2((float)Math.Cos(angle) * speed, (float)Math.Sin(angle) * speed * 0.6f);

                Game1.currentLocation.temporarySprites.Add(new TemporaryAnimatedSprite(
                    10,
                    worldPos + new Vector2(Game1.random.Next(-16, 16), Game1.random.Next(-8, 8)),
                    Color.Gold * 0.95f)
                {
                    scale = 1.6f,
                    motion = gMotion,
                    interval = 60f,
                    alphaFade = 0.03f,
                    layerDepth = 0.9998f
                });
            }
        }
    }

    private void OnRenderedWorld(object? sender, RenderedWorldEventArgs e)
    {
        if (this.AnimState.Phase == PodSequencePhase.None || this.AnimState.Phase == PodSequencePhase.Fade_Transition)
            return;

        SpriteBatch b = e.SpriteBatch;
        Vector2 screenPos = Game1.GlobalToLocal(Game1.viewport, this.AnimState.PodCurrentWorldPos);

        float scale = 3.2f;
        int frameW = 64;
        int frameH = 64;
        Rectangle sourceRect = new Rectangle(this.AnimState.HatchFrame * frameW, 0, frameW, frameH);
        Vector2 origin = new Vector2(frameW / 2f, frameH / 2f);

        // 1. Bóng đổ chuyên dụng mượt mà không vỡ hạt pixel
        Texture2D shadowTex = this.ShadowTexture ?? Game1.shadowTexture;
        if (shadowTex != null)
        {
            Vector2 shadowWorldPos = new Vector2(this.AnimState.TargetTilePosition.X, this.AnimState.TargetTilePosition.Y + 68f);
            Vector2 shadowScreenPos = Game1.GlobalToLocal(Game1.viewport, shadowWorldPos);

            float altitude = Math.Max(0f, this.AnimState.TargetTilePosition.Y - this.AnimState.PodCurrentWorldPos.Y);
            float shadowFactor = Math.Clamp(1f - (altitude / 1000f), 0.2f, 1f);
            
            float shadowScale = (shadowTex == this.ShadowTexture ? (scale * 0.45f) : (scale * 4.2f)) * shadowFactor;
            float shadowAlpha = 0.80f * shadowFactor;

            Vector2 shadowOrigin = new Vector2(shadowTex.Bounds.Width / 2f, shadowTex.Bounds.Height / 2f);

            b.Draw(
                shadowTex,
                shadowScreenPos,
                shadowTex.Bounds,
                Color.White * shadowAlpha,
                0f,
                shadowOrigin,
                shadowScale,
                SpriteEffects.None,
                0.0001f
            );
        }

        // 2. Thân tàu
        b.Draw(
            this.PodTexture,
            screenPos,
            sourceRect,
            Color.White,
            0f,
            origin,
            scale,
            SpriteEffects.None,
            0.85f
        );

        // 3. Lửa phản lực
        if (this.AnimState.ShowFlame)
        {
            int flameW = 32;
            int flameH = 48;
            Rectangle flameSource = new Rectangle(this.AnimState.FlameFrame * flameW, 0, flameW, flameH);
            Vector2 flameOrigin = new Vector2(flameW / 2f, 0f);

            bool isFalling = (this.AnimState.Phase == PodSequencePhase.Boarding_Fall || this.AnimState.Phase == PodSequencePhase.Arrival_Fall);
            float flameRotation = isFalling ? (float)Math.PI : 0f;
            Vector2 flameOffset = isFalling ? new Vector2(0f, -22f * scale) : new Vector2(0f, 24f * scale);

            b.Draw(
                this.FlameTexture,
                screenPos + flameOffset,
                flameSource,
                Color.White,
                flameRotation,
                flameOrigin,
                scale * 1.1f,
                SpriteEffects.None,
                0.86f
            );
        }

        // 4. Farmer đứng trước cửa buồng lái
        if (!this.AnimState.FarmerInsidePod && Game1.player != null && Game1.player.Position.X > -1000f)
        {
            Game1.player.draw(b);
        }
    }
}
