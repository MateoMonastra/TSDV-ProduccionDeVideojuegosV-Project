using UnityEngine;

namespace Player.New
{
    [CreateAssetMenu(fileName = "PlayerModel", menuName = "Models/NewPlayer")]
    public class PlayerModel : ScriptableObject
    {
        [Header("Settings")]
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float jumpVelocity = 7f;
        [SerializeField] private float gravity = -25f;
        [SerializeField] private float airAcceleration = 15f;
        [SerializeField] private float maxAirSpeed = 10f;
        [SerializeField] private float rotationSharpness = 15f;
        [SerializeField] private float airControlSharpness = 5f;
        
        internal Vector3 MoveInput;
        internal Vector3 LookInput;
        
        public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }
        public float JumpVelocity { get => jumpVelocity; set => jumpVelocity = value; }
        public float Gravity { get => gravity; set => gravity = value; }
        public float AirAcceleration { get => airAcceleration; set => airAcceleration = value; }
        public float MaxAirSpeed { get => maxAirSpeed; set => maxAirSpeed = value; }
        public float RotationSharpness { get => rotationSharpness; set => rotationSharpness = value; }
        public float AirControlSharpness { get => airControlSharpness; set => airControlSharpness = value; }
    }
}