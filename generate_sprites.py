from PIL import Image, ImageDraw
import math

# Create metallic Saiyan Pod sprite sheet
# Frame size: 64x64 for Pod (4 frames: Closed, Opening 1, Opening 2, Open)
# Remote size: 16x16
# Flame effect: 32x48 (3 frames)

pod_w, pod_h = 64, 64
sheet = Image.new("RGBA", (pod_w * 4, pod_h), (0, 0, 0, 0))

def draw_metallic_sphere(draw, cx, cy, r, base_color, shadow_color, dark_rim, highlight_color):
    for y in range(cy - r, cy + r + 1):
        for x in range(cx - r, cx + r + 1):
            dx = x - cx
            dy = y - cy
            dist_sq = dx * dx + dy * dy
            if dist_sq <= r * r:
                nx = dx / r
                ny = dy / r
                nz = math.sqrt(max(0.0, 1.0 - nx*nx - ny*ny))
                # Light coming from top-left (-0.45, -0.6, 0.65)
                lx, ly, lz = -0.45, -0.6, 0.65
                dot = nx * lx + ny * ly + nz * lz
                
                # Outer rim darken
                edge_factor = math.sqrt(dist_sq) / r
                
                if edge_factor > 0.92:
                    c = dark_rim
                elif dot > 0.65:
                    c = highlight_color
                elif dot > 0.15:
                    c = base_color
                else:
                    c = shadow_color
                draw.point((x, y), fill=c)

# Metallic Cool Steel-Gray Palette (hơi xám hơn, chuẩn kim loại tàu vũ trụ)
c_base_metal = (185, 192, 202, 255)      # Cool metallic steel-gray
c_shadow_metal = (115, 122, 138, 255)    # Slate metallic shadow
c_dark_groove = (50, 54, 65, 255)        # Deep mechanical seams
c_rim_metal = (80, 85, 98, 255)          # Outer metallic edge
c_highlight = (235, 242, 250, 255)       # Specular metal sheen

c_glass_red = (215, 30, 25, 255)
c_glass_glow = (255, 100, 90, 255)
c_glass_dark = (110, 12, 12, 255)
c_interior = (28, 25, 35, 255)
c_interior_seat = (165, 35, 30, 255)

for frame in range(4):
    f_img = Image.new("RGBA", (pod_w, pod_h), (0, 0, 0, 0))
    d = ImageDraw.Draw(f_img)
    cx, cy, r = 32, 34, 25
    
    # Outer Metallic Sphere
    draw_metallic_sphere(d, cx, cy, r, c_base_metal, c_shadow_metal, c_rim_metal, c_highlight)
    
    # Mechanical panel seams / lines
    d.arc([cx - r + 2, cy - r + 2, cx + r - 2, cy + r - 2], start=190, end=350, fill=c_dark_groove, width=1)
    d.line([cx - 20, cy, cx - 12, cy + 16], fill=c_dark_groove, width=1)
    d.line([cx + 20, cy, cx + 12, cy + 16], fill=c_dark_groove, width=1)
    d.line([cx - 22, cy - 6, cx + 22, cy - 6], fill=c_dark_groove, width=1)
    # Rivet dots on panel joints
    d.point((cx - 16, cy - 1), fill=(225, 230, 240, 255))
    d.point((cx + 16, cy - 1), fill=(225, 230, 240, 255))
    d.point((cx - 10, cy + 14), fill=(225, 230, 240, 255))
    d.point((cx + 10, cy + 14), fill=(225, 230, 240, 255))

    # Frame details for hatch/door
    if frame == 0:
        # Closed: round red window in center
        wx, wy, wr = cx, cy + 2, 11
        draw_metallic_sphere(d, wx, wy, wr, c_glass_red, c_glass_dark, c_dark_groove, c_glass_glow)
        d.ellipse([wx - wr - 1, wy - wr - 1, wx + wr + 1, wy + wr + 1], outline=c_dark_groove, width=1)
    elif frame == 1:
        # Cracking open: window pops forward and lifts
        wx, wy, wr = cx, cy - 2, 11
        d.ellipse([cx - 9, cy - 4, cx + 9, cy + 12], fill=c_interior)
        draw_metallic_sphere(d, wx, wy, wr, c_glass_red, c_glass_dark, c_dark_groove, c_glass_glow)
        d.ellipse([wx - wr - 1, wy - wr - 1, wx + wr + 1, wy + wr + 1], outline=c_dark_groove, width=1)
    elif frame == 2:
        # Hatch halfway open
        d.ellipse([cx - 12, cy - 5, cx + 12, cy + 14], fill=c_interior)
        d.rectangle([cx - 6, cy + 1, cx + 6, cy + 10], fill=c_interior_seat)
        wx, wy = cx, cy - 12
        d.ellipse([wx - 11, wy - 6, wx + 11, wy + 6], fill=c_glass_red, outline=c_dark_groove, width=1)
        d.ellipse([wx - 6, wy - 3, wx + 6, wy + 3], fill=c_glass_glow)
    elif frame == 3:
        # Fully open hatch: cockpit interior
        d.ellipse([cx - 13, cy - 6, cx + 13, cy + 16], fill=c_interior)
        d.rectangle([cx - 8, cy - 2, cx + 8, cy + 11], fill=c_interior_seat)
        d.rectangle([cx - 5, cy - 4, cx + 5, cy], fill=(205, 55, 45, 255))
        d.point((cx - 10, cy + 4), fill=(0, 255, 255, 255))
        d.point((cx + 10, cy + 4), fill=(0, 255, 100, 255))
        wx, wy = cx, cy - 20
        d.ellipse([wx - 12, wy - 5, wx + 12, wy + 5], fill=c_glass_red, outline=c_dark_groove, width=1)
        d.ellipse([wx - 7, wy - 2, wx + 7, wy + 2], fill=c_glass_glow)

    sheet.paste(f_img, (frame * pod_w, 0))

sheet.save("/home/Lixin/stardew-lab/SaiyanPodTeleport/assets/saiyan_pod.png")
print("Saved updated metallic saiyan_pod.png")

# Thruster / Boost flame sheet: 32x48 (3 frames)
flame_w, flame_h = 32, 48
flame_sheet = Image.new("RGBA", (flame_w * 3, flame_h), (0, 0, 0, 0))
flame_colors = [
    (255, 240, 150, 255), # Core yellow
    (255, 140, 30, 240),  # Mid orange
    (230, 40, 20, 180),   # Outer red
]
for f in range(3):
    img = Image.new("RGBA", (flame_w, flame_h), (0, 0, 0, 0))
    d = ImageDraw.Draw(img)
    base_len = 32 + (f * 6 % 14)
    # Outer cone
    d.polygon([(8, 0), (24, 0), (16, base_len)], fill=flame_colors[2])
    # Mid cone
    d.polygon([(10, 0), (22, 0), (16, int(base_len * 0.75))], fill=flame_colors[1])
    # Core
    d.polygon([(12, 0), (20, 0), (16, int(base_len * 0.45))], fill=flame_colors[0])
    flame_sheet.paste(img, (f * flame_w, 0))

flame_sheet.save("/home/Lixin/stardew-lab/SaiyanPodTeleport/assets/thruster_flame.png")
print("Saved thruster_flame.png")

# Remote Controller Item icon (16x16)
remote = Image.new("RGBA", (16, 16), (0, 0, 0, 0))
rd = ImageDraw.Draw(remote)
# Body (dark metallic gray / rounded)
rd.rectangle([3, 1, 12, 14], fill=(55, 58, 65, 255), outline=(30, 32, 38, 255))
# Antenna
rd.line([4, 0, 4, 1], fill=(180, 185, 195, 255))
rd.point((4, 0), fill=(255, 50, 50, 255)) # tiny red led
# Screen / Display (Capsule Corp / Saiyan Scouter green/red)
rd.rectangle([5, 3, 10, 6], fill=(40, 200, 120, 255), outline=(20, 80, 50, 255))
# Big Red Call Button
rd.ellipse([6, 8, 9, 11], fill=(230, 40, 30, 255), outline=(140, 20, 15, 255))
# Grip lines
rd.line([4, 13, 11, 13], fill=(30, 32, 38, 255))
remote.save("/home/Lixin/stardew-lab/SaiyanPodTeleport/assets/remote_item.png")
print("Saved remote_item.png")
