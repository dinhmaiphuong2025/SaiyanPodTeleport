
from PIL import Image

pod = Image.open("/home/Lixin/stardew-lab/SaiyanPodTeleport/assets/saiyan_pod.png")
flame = Image.open("/home/Lixin/stardew-lab/SaiyanPodTeleport/assets/thruster_flame.png")
remote = Image.open("/home/Lixin/stardew-lab/SaiyanPodTeleport/assets/remote_item.png")

# Preview image at 3x scale
preview = Image.new("RGBA", (pod.width + 60, pod.height + flame.height + 20), (35, 38, 48, 255))
preview.paste(pod, (10, 10), pod)
preview.paste(flame, (10, 10 + pod.height), flame)
preview.paste(remote.resize((32, 32), Image.Resampling.NEAREST), (pod.width + 15, 15), remote.resize((32, 32), Image.Resampling.NEAREST))

preview.save("/home/Lixin/stardew-lab/SaiyanPodTeleport/assets/preview.png")
print("Saved preview:", preview.size)
