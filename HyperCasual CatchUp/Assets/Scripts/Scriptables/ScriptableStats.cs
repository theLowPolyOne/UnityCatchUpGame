using UnityEngine;

namespace Fallencake.CharacterController
{
    [CreateAssetMenu(menuName = "Fallencake/Character Controller/Stats")]
    public class ScriptableStats : ScriptableObject
    {
        [Header("MOVEMENT")]
        [Tooltip("The top horizontal movement speed")]
        public float MaxSpeed = 14;

        [Tooltip("The player's capacity to gain horizontal speed")]
        public float Acceleration = 120;

        [Tooltip("The pace at which the player comes to a stop")]
        public float GroundDeceleration = 60;

        [Tooltip("Deceleration in air only after stopping input mid-air")]
        public float AirDeceleration = 30;

        [Tooltip("A constant downward force applied while grounded. Helps on slopes"), Range(0f, -10f)]
        public float GroundingForce = -1.5f;

        [Tooltip("Minimum input req'd before you mount a ladder or climb a ledge. Avoids unwanted climbing using controllers"), Range(0.01f, 0.99f)]
        public float VerticalDeadzoneThreshold = 0.3f;

        [Tooltip("Minimum input req'd before a left or right is recognized. Avoids drifting with sticky controllers"), Range(0.01f, 0.99f)]
        public float HorizontalDeadzoneThreshold = 0.1f;

        [Header("CROUCHING")]
        [Tooltip("Allows crouching")]
        public bool AllowCrouching = true;

        [Tooltip("A speed multiplier while crouching"), Range(0f, 1f)]
        public float CrouchSpeedPenalty = 0.5f;

        [Tooltip("The amount of frames it takes to hit the full crouch speed penalty. Higher values provide more crouch sliding"), Min(0)]
        public int CrouchSlowdownFrames = 50;

        [Tooltip("Detection height offset from the top of the standing collider. Smaller values risk collisions when standing up")]
        public float CrouchBufferCheck = 0.1f;

        [Header("JUMP")]
        [Tooltip("Amount of air jumps allowed. e.g. 1 is a standard double jump"), Min(0)]
        public int MaxAirJumps = 1;

        [Tooltip("The immediate velocity applied when jumping")]
        public float JumpPower = 36;

        [Tooltip("The maximum vertical movement speed")]
        public float MaxFallSpeed = 40;

        [Tooltip("The player's capacity to gain fall speed")]
        public float FallAcceleration = 110;

        [Tooltip("The gravity multiplier added when jump is released early")]
        public float JumpEndEarlyGravityModifier = 3;

        [Tooltip("The fixed frames before coyote jump becomes unusable. Coyote jump allows jump to execute even after leaving a ledge")]
        public int CoyoteFrames = 7;

        [Tooltip("The amount of fixed frames we buffer a jump. This allows jump input before actually hitting the ground")]
        public int JumpBufferFrames = 7;

        [Header("WALLS")]
        [Tooltip("Allows wall sliding & jumping")]
        public bool AllowWalls = true;

        [Tooltip("Set this to the layer climbable walls are on")]
        public LayerMask ClimbableLayer;

        [Tooltip("Only wall slide when you're physically pushing against the wall")]
        public bool RequireInputPush = false;

        [Tooltip("How fast you climb walls.")]
        public float WallClimbSpeed = 5;

        [Tooltip("The player's capacity to gain wall sliding speed. 0 = stick to wall")]
        public float WallFallAcceleration = 8;

        [Tooltip("Clamps the maximum fall speed")]
        public float MaxWallFallSpeed = 15;

        [Tooltip("The immediate velocity horizontal velocity applied when wall jumping")]
        public Vector3 WallJumpPower = new(30, 25);

        [Tooltip("The frames before full horizontal movement is returned after a wall jump"), Min(1)]
        public int WallJumpInputLossFrames = 18;

        [Header("LADDERS")]
        [Tooltip("Allows climbing up and down ladders")]
        public bool AllowLadders = true;

        [Tooltip("When enabled, nudges the player to be centered on the ladder")]
        public bool SnapToLadders = true;

        [Tooltip("Set this to the layer your ladders are on")]
        public LayerMask LadderLayer;

        [Tooltip("Ladder snap speed")]
        public float LadderSnapSpeed = 0.05f;

        [Tooltip("Horizontal speed multiplier while attached to a ladder")]
        public float LadderShimmySpeedMultiplier = 0.5f;

        [Tooltip("How fast you climb ladders")]
        public float LadderClimbSpeed = 8;

        [Tooltip("How fast you slide down ladders")]
        public float LadderSlideSpeed = 12;

        [Tooltip("How many frames can pass between ladder interactions. Helps jittery movement and jumping glitches")]
        public int LadderCooldownFrames = 8;

        [Header("LEDGE")]
        [Tooltip("Allows ledge grabbing & climbing")]
        public bool AllowLedges = true;

        [Tooltip("The rate at which we slow our velocity to grab a ledge. Too low, we slide off. Too high, we won't match our GrabPoint")]
        public float LedgeGrabDeceleration = 4f;

        [Tooltip("Relative point from the player's position where the ledge corner will be when hanging")]
        public Vector3 LedgeGrabPoint = new(0.3f, 0.7f);

        [Tooltip("Relative point from the ledge corner where the new player position will be after climbing up")]
        public Vector3 StandUpOffset = new(0.25f, 0.1f);

        [Tooltip("Vertical offset above and below your grab point to detect a nearby ledge"), Min(0.05f)]
        public float LedgeRaycastSpacing = 0.3f;

        [Tooltip("How long movement will be locked out. Animation clip length")]
        public float LedgeClimbDuration = 0.5f;

        [Header("DASH")]
        [Tooltip("Allows the player to dash")]
        public bool AllowDash = true;

        [Tooltip("The velocity of the dash")]
        public float DashVelocity = 50;

        [Tooltip("How many fixed frames the dash will last")]
        public int DashDurationFrames = 5;

        [Tooltip("The horizontal speed retained when dash has completed")]
        public float DashEndHorizontalMultiplier = 0.25f;

        [Header("ATTACKS")]
        [Tooltip("Allows the player to attack")]
        public bool AllowAttacks = true;

        [Tooltip("The fixed frame cooldown of your players basic attack")]
        public int AttackFrameCooldown = 15;

        [Header("COLLISIONS")]
        [Tooltip("Set this to the layer your player is on")]
        public LayerMask PlayerLayer;

        [Tooltip("The detection distance for grounding and roof detection")]
        public float GrounderDistance = 0.1f;

        [Tooltip("Bounds for detecting walls on either side. Ensure it's wider than your vertical capsule collider")]
        public Vector3 WallDetectorSize = new(0.75f, 1.25f);

        [Header("ROTATION")]
        [Tooltip("The player's rotation rate")]
        public float RotationSpeed = 50f;
        [Tooltip("The player's rotation angle threshold")]
        public float RotationThreshold = 3f;

        [Header("EXTERNAL")]
        [Tooltip("The rate at which external velocity decays")]
        public int ExternalVelocityDecay = 100;

#if UNITY_EDITOR
        [Header("GIZMOS")]
        [Tooltip("Color: Green")]
        public bool ShowGroundDetection = true;

        [Tooltip("Color: White")]
        public bool ShowWallDetection = true;

        [Tooltip("Color: Red")]
        public bool ShowLedgeDetection = true;
#endif
    }
}