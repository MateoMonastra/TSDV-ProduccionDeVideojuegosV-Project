using UnityEngine;
using UnityEngine.Serialization;

namespace Enemies.BaseEnemy
{
    [CreateAssetMenu(fileName = "EnemyModel", menuName = "Models/Enemy")]
    public class BaseEnemyModel : ScriptableObject
    {
        [Header("Detection Settings")]
        [Tooltip("Radio interno: a esta distancia pasa de Idle a Chase.")]
        [Min(0f)] [SerializeField] private float innerRadius = 4f;

        [Tooltip("Radio externo: al superarlo, vuelve a Idle.")]
        [Min(0f)] [SerializeField] private float outerRadius = 12f;

        [Tooltip("Capa de suelo para raycasts de grounded/impulses.")]
        [SerializeField] private LayerMask groundLayer = ~0;

        [Header("Attack Settings")]
        [Tooltip("Distancia de ataque.")]
        [Min(0f)] [SerializeField] private float attackRange = 2.2f;

        [Tooltip("Duración de la ventana de impacto.")]
        [Min(0f)] [SerializeField] private float attackDuration = 0.15f;

        [Tooltip("Demora desde el inicio de la anim hasta abrir la ventana.")]
        [Min(0f)] [SerializeField] private float attackDelay = 0.25f;

        [Header("Damage Feedback Settings")]
        [Tooltip("Impulso horizontal aplicado al recibir daño.")]
        [Min(0f)] [SerializeField] private float horizontalImpulseForce = 12f;

        [Tooltip("Impulso vertical aplicado al recibir daño.")]
        [Min(0f)] [SerializeField] private float verticalImpulseForce = 3.5f;

        [Tooltip("Multiplicador de caída (gravedad extra hacia abajo).")]
        [Min(0f)] [SerializeField] private float lowJumpMultiplier = 1.5f;

        [Tooltip("Multiplicador cuando asciende (para saltos bajos).")]
        [Min(0f)] [SerializeField] private float fallMultiplier = 2.5f;

        [Tooltip("Stun tras daño (si tu FSM lo usa).")]
        [Min(0f)] [SerializeField] private float damagedStunTime = 0.1f;

        [Tooltip("Tiempo hasta despawn en Death.")]
        [Min(0f)] [SerializeField] private float deathTime = 1.0f;

        // ───────── Read/Write props (compatibles con tu código actual)
        public float InnerRadius { get => innerRadius; set => innerRadius = Mathf.Max(0f, value); }
        public float OuterRadius { get => outerRadius; set => outerRadius = Mathf.Max(0f, value); }
        public LayerMask GroundLayer { get => groundLayer; set => groundLayer = value; }
        public float AttackRange { get => attackRange; set => attackRange = Mathf.Max(0f, value); }
        public float AttackDuration { get => attackDuration; set => attackDuration = Mathf.Max(0f, value); }
        public float AttackDelay { get => attackDelay; set => attackDelay = Mathf.Max(0f, value); }
        public float HorizontalImpulseForce { get => horizontalImpulseForce; set => horizontalImpulseForce = Mathf.Max(0f, value); }
        public float VerticalImpulseForce { get => verticalImpulseForce; set => verticalImpulseForce = Mathf.Max(0f, value); }
        public float LowJumpMultiplier { get => lowJumpMultiplier; set => lowJumpMultiplier = Mathf.Max(0f, value); }
        public float FallMultiplier { get => fallMultiplier; set => fallMultiplier = Mathf.Max(0f, value); }
        public float DamagedStunTime { get => damagedStunTime; set => damagedStunTime = Mathf.Max(0f, value); }
        public float DeathTime { get => deathTime; set => deathTime = Mathf.Max(0f, value); }

        // ───────── Cached squared (evitan sqrt por frame)
        public float SqrInnerRadius => innerRadius * innerRadius;
        public float SqrOuterRadius => outerRadius * outerRadius;
        public float SqrAttackRange => attackRange * attackRange;

        private void OnValidate()
        {
            // Garantizar relaciones sanas
            if (outerRadius < innerRadius) outerRadius = innerRadius;
            if (attackRange > outerRadius) attackRange = outerRadius;

            // Multiplicadores no negativos
            lowJumpMultiplier  = Mathf.Max(0f, lowJumpMultiplier);
            fallMultiplier     = Mathf.Max(0f, fallMultiplier);
        }
    }
}
