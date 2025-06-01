using UnityEngine;
using UnityEngine.Serialization;

namespace Hazards.Catapult
{
    [CreateAssetMenu(fileName = "CatapultModel", menuName = "Models/Catapult")]
    public class CatapultModel : ScriptableObject
    {
        [SerializeField] private float attackRange;
        [SerializeField] private float flightDuration;
        [SerializeField] private bool debugDrawTrajectory = false;
        [SerializeField] private int drawResolution = 30;
        [SerializeField] private float maxRayDistance;

        public float AttackRange
        {
            get => attackRange;
            set => attackRange = value;
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
    }
}