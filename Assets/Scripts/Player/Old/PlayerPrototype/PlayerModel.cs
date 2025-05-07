using KinematicCharacterController.Examples;
using UnityEngine;

namespace Player.Old.PlayerPrototype
{
    [CreateAssetMenu(fileName = "PlayerModel", menuName = "Models/Player")]
    public class PlayerModel : ScriptableObject
    {
        [Header("Stable Movement")]
        public float MaxStableMoveSpeed = 10f;
        public float StableMovementSharpness = 15f;
        public float OrientationSharpness = 10f;
        public OrientationMethod OrientationMethod = OrientationMethod.TowardsCamera;
        
        [Header("Air Movement")]
        public float MaxAirMoveSpeed = 15f;
        public float AirAccelerationSpeed = 15f;
        public float Drag = 0.1f;
        
        [Header("Jumping")]
        public bool AllowJumpingWhenSliding = false;
        public float JumpUpSpeed = 10f;
        public float JumpScalableForwardSpeed = 10f;
        public float JumpPreGroundingGraceTime = 0f;
        public float JumpPostGroundingGraceTime = 0f;
        public int MaxJumps = 2; // Maximum number of jumps allowed
        
        public Vector3 Gravity = new Vector3(0, -30f, 0);
        
        [Header("Dashing")]
        public float DashSpeed = 30f;
        public float DashDuration = 0.2f;
        public float DashCooldown = 1f;
    }
}