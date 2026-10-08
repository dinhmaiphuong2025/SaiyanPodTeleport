using Microsoft.Xna.Framework;

namespace SaiyanPodTeleport;

public record TeleportDestination(
    string Id,
    string DisplayName,
    string Category, // "Nông trại", "Thung lũng", "Đảo & Sa mạc"
    string LocationName,
    int TileX,
    int TileY,
    string Description
);
