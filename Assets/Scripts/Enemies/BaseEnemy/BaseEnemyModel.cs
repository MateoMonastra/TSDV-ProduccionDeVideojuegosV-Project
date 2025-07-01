using UnityEngine;
using UnityEngine.Serialization;

namespace Enemies.BaseEnemy
{
    [CreateAssetMenu(fileName = "EnemyModel", menuName = "Models/Enemy")]
    public class BaseEnemyModel : ScriptableObject
    {
        [SerializeField] private float innerRadius;
        [SerializeField] private float outerRadius;
        [SerializeField] private float attackRange;
        [SerializeField] private float attackDuration;
        [SerializeField] private float attackDelay;
        [SerializeField] private float horizontalImpulseForce;
        [SerializeField] private float verticalImpulseForce;
        [SerializeField] private float lowJumpMultiplier;
        [SerializeField] private float fallMultiplier;
        [SerializeField] private float damagedStunTime;
        [SerializeField] private float deathTime;

        public float InnerRadius
        {
            get => innerRadius;
            set => innerRadius = value;
        }

        public float OuterRadius
        {
            get => outerRadius;
            set => outerRadius = value;
        }

        public float AttackRange
        {
            get => attackRange;
            set => attackRange = value;
        }

        public float AttackDuration
        {
            get => attackDuration;
            set => attackDuration = value;
        }

        public float AttackDelay
        {
            get => attackDelay;
            set => attackDelay = value;
        }

        public float HorizontalImpulseForce
        {
            get => horizontalImpulseForce;
            set => horizontalImpulseForce = value;
        }

        public float VerticalImpulseForce
        {
            get => verticalImpulseForce;
            set => verticalImpulseForce = value;
        }
        
        public float LowJumpMultiplier
        {
            get => lowJumpMultiplier;
            set => lowJumpMultiplier = value;
        }

        public float FallMultiplier
        {
            get => fallMultiplier;
            set => fallMultiplier = value;
        }
        
        public float DamagedStunTime
        {
            get => damagedStunTime;
            set => damagedStunTime = value;
        }
        
        public float DeathTime
        {
            get => deathTime;
            set => deathTime = value;
        }
    }
}