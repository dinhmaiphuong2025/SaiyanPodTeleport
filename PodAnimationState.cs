using System;
using Microsoft.Xna.Framework;

namespace SaiyanPodTeleport;

public enum PodSequencePhase
{
    None,
    Boarding_Fall,      // Pod falls down to farmer
    Boarding_Impact,    // Dust & shake, landing
    Boarding_Opening,   // Door opens
    Boarding_Enter,     // Farmer enters, door closes
    Boarding_Ascent,    // Pod blasts off into sky
    Fade_Transition,    // Global fade out & map warp
    Arrival_Fall,       // Falling into destination map
    Arrival_Impact,     // Ground impact & dust
    Arrival_Opening,    // Door opens
    Arrival_Exit,       // Farmer steps out
    Departure_Ascent    // Empty pod launches back into space
}

public class PodAnimationState
{
    public PodSequencePhase Phase { get; set; } = PodSequencePhase.None;
    public float PhaseTimer { get; set; } = 0f;
    public Vector2 TargetTilePosition { get; set; } // World pixel position where pod lands
    public Vector2 PodCurrentWorldPos { get; set; } // Current moving pixel position of pod
    public float PodScale { get; set; } = 4f; // 64x64 at 4x = 256x256 px
    public int HatchFrame { get; set; } = 0; // 0 = Closed, 1 = Opening, 2 = Half, 3 = Open
    public bool ShowFlame { get; set; } = false;
    public int FlameFrame { get; set; } = 0;
    public float FlameAnimTimer { get; set; } = 0f;
    public TeleportDestination? PendingDestination { get; set; }

    // Farmer walking & boarding animation state
    public bool FarmerInsidePod { get; set; } = false;
    public Vector2 FarmerOriginalPos { get; set; }
    public Vector2 FarmerStepOffset { get; set; } = Vector2.Zero;
    public float FarmerScale { get; set; } = 1f;

    // Thông tin phiên bản Remote kích hoạt lần này
    public RemoteTierInfo ActiveTier { get; set; } = RemoteTierInfo.ProMaxInfo;
}
