using UnityEngine;
using UnityEngine.Serialization;

namespace Hazards.Cannon
{
    [CreateAssetMenu(fileName = "CannonModel", menuName = "Models/Cannon")]
    public class CannonModel : ScriptableObject
    {
        [SerializeField] private float attackRange;
        [SerializeField] private float cooldownBetweenAttacks;
        [SerializeField] private float flightDuration;
        [SerializeField] private bool debugDrawTrajectory = false;
        [SerializeField] private int drawResolution = 30;
        [SerializeField] private float maxRayDistance;
        [SerializeField] private float rotateVelocity;

        public float AttackRange
        {
            get => attackRange;
            set => attackRange = value;
        }
        
        public float CooldownBetweenAttacks
        {
            get => cooldownBetweenAttacks;
            set => cooldownBetweenAttacks = value;
        }

        public float FlightDuration
        {
            get => flightDuration;
            set => flightDuration = value;
        }

        public bool DebugDrawTrajectory
        {
            get => debugDrawTrajectory;
            set => debugDrawTrajectory = value;
        }

        public int DrawResolution
        {
            get => drawResolution;
            set => drawResolution = value;
        }

        public float MaxRayDistance
        {
            get => maxRayDistance;
            set => maxRayDistance = value;
        }
        public float RotateVelocity
        {
            get => rotateVelocity;
            set => rotateVelocity = value;
        }
    }
}