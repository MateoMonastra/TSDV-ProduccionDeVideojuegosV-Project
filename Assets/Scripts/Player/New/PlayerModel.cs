using UnityEngine;

namespace Player.New
{
    public enum OrientationMethod { TowardsMovement, TowardsCamera }

    [System.Serializable]
    [CreateAssetMenu(fileName = "NewPlayerModel", menuName = "Models/NewPlayerModel")]
    public class PlayerModel : ScriptableObject
    {
        [Header("Movement")]
        [Tooltip("Velocidad de movimiento en suelo")]
        public float MoveSpeed = 6f;
        [Tooltip("Aceleración plana hacia MoveSpeed. Si querés stop instantáneo, lo forzamos en suelo.")]
        public float MoveAcceleration = 30f;
        [Tooltip("Velocidad horizontal en el aire (\"traslado durante el salto\")")]
        public float AirHorizontalSpeed = 6f;

        [Header("Orientation")]
        public OrientationMethod OrientationMethod = OrientationMethod.TowardsMovement;
        [Range(1f, 30f)] public float OrientationSharpness = 12f;

        [Header("Jump")]
        [Tooltip("Impulso vertical (ambos saltos). Ajusta la distancia final.")]
        public float JumpSpeed = 7.5f;
        [Tooltip("Cantidad total de saltos (2 = doble salto)")]
        public int MaxJumps = 2;

        [Header("Dash")]
        [Tooltip("Distancia del dash. Por consigna = 2x tamaño del jugador (radio/alto)")]
        public float DashDistance = 2.0f;
        [Tooltip("Velocidad del dash (m/s)")]
        public float DashSpeed = 18f;
        [Tooltip("CD del dash (s)")]
        public float DashCooldown = 1.2f;
        [Tooltip("cuánto dura la transición suave al terminar el dash")]
        public float DashExitBlendTime = 0.12f;
        [Tooltip("cuán rápido interpola hacia la velocidad objetivo")]
        [Range(1f, 30f)] public float DashExitSharpness = 10f;
        
        [Header("Combat")]
        public LayerMask EnemyMask = ~0;           // Layers de enemigos (configurable en Inspector)
        public float AttackDamage = 10f;           // Daño del golpe básico
        public float AttackChainWindow = 0.30f;    // Ventana para clickear y encadenar al siguiente golpe

        [Header("Runtime - Combo")]
        [HideInInspector] public bool AttackComboOnCooldown;
        [HideInInspector] public float AttackComboCooldownLeft;

        [Header("Attack (combo básico)")]
        [Tooltip("Duración de cada ataque (s)")]
        public float Attack1Duration = 0.25f;
        public float Attack2Duration = 0.28f;
        public float Attack3Duration = 0.34f;
        [Tooltip("Distancia efectiva del ataque frontal")]
        public float AttackRange = 2.0f;
        [Tooltip("CD al terminar los 3 ataques (s)")]
        public float AttackComboCooldown = 0.4f;
        [Tooltip("Empuje al enemigo golpeado (m)")]
        public float AttackKnockbackDistance = 2.5f;
        [Tooltip("Stagger/slow del enemigo (s)")]
        public float AttackStaggerTime = 0.35f;

        [Header("Attack Vertical (aéreo)")]
        public float VerticalAttackDuration = 0.5f;
        public float VerticalAttackRadius = 2.8f;
        public float VerticalAttackCooldown = 1.0f;
        public float VerticalAttackPostStun = 0.25f; // inmóvil luego del impacto

        [Header("Attack 360° (carga)")]
        public float SpinChargeMinTime = 0.6f;
        public float SpinDuration = 0.7f;      // duración del giro/ejecución
        public float SpinRadius = 2.8f;
        public float SpinPushDistance = 2.0f;
        public float SpinCooldown = 3.0f;
        public float SpinMoveSpeedMultiplierWhileCharging = 0.6f;
        public float SpinPostStun = 0.35f;     // inmóvil post-ejecución

        [Header("Action Modifiers (capa de Acciones)")]
        [Tooltip("Multiplica la velocidad resultante (p.ej. cargar = 0.6x)")]
        public float ActionMoveSpeedMultiplier = 1f;
        public bool AimLockActive = false;
        public Vector3 AimLockDirection = Vector3.zero;
        [Tooltip("Bloquea locomoción (ataque vertical, release 360, etc.)")]
        public bool LocomotionBlocked = false;
        [Tooltip("Invulnerable a enemigos (durante dash)")]
        public bool InvulnerableToEnemies = false;

        [Header("Runtime")]
        [HideInInspector] public Vector2 RawMoveInput;
        [HideInInspector] public Vector3 MoveInputWorld;
        [HideInInspector] public int JumpsLeft;
        [HideInInspector] public bool JumpWasPureVertical;     // necesario para ataque vertical
        [HideInInspector] public bool DashOnCooldown;
        [HideInInspector] public float DashCooldownLeft;
        [HideInInspector] public bool SpinOnCooldown;
        [HideInInspector] public float SpinCooldownLeft;
        [HideInInspector] public bool VerticalOnCooldown;
        [HideInInspector] public float VerticalCooldownLeft;

        public void ResetJumps() => JumpsLeft = MaxJumps;
    }
}
