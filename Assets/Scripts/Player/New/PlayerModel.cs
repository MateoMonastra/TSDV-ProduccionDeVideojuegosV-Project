using UnityEngine;

namespace Player.New
{
    public enum OrientationMethod
    {
        TowardsMovement,
        TowardsCamera
    }

    [System.Serializable]
    [CreateAssetMenu(fileName = "NewPlayerModel", menuName = "Models/NewPlayerModel")]
    public class PlayerModel : ScriptableObject
    {
        // ───────────────────────────────── MOVEMENT ─────────────────────────────────
        [Header("Movement")]
        [Tooltip("Velocidad de movimiento en suelo (m/s).")]
        public float moveSpeed = 6f;

        [Tooltip("Aceleración en suelo hacia MoveSpeed (m/s²).")]
        public float moveAcceleration = 30f;

        [Tooltip("Velocidad horizontal máxima en el aire (m/s).")]
        public float airHorizontalSpeed = 6f;

        // ──────────────────────────────── ORIENTATION ───────────────────────────────
        [Header("Orientation")]
        [Tooltip("Cómo se orienta el personaje: hacia el movimiento o hacia la cámara.")]
        public OrientationMethod orientationMethod = OrientationMethod.TowardsMovement;

        [Tooltip("Qué tan rápido interpola la rotación hacia el objetivo.")]
        [Range(1f, 30f)] public float orientationSharpness = 12f;

        // ─────────────────────────────────── JUMP ───────────────────────────────────
        [Header("Jump")]
        [Tooltip("Impulso vertical de cada salto (afecta la altura/tiempo de vuelo).")]
        public float jumpSpeed = 7.5f;

        [Tooltip("Cantidad total de saltos permitidos (2 = doble salto).")]
        public int maxJumps = 2;

        // ─────────────────────────────────── DASH ───────────────────────────────────
        [Header("Dash")]
        [Tooltip("Distancia total del dash (consigna: ≈ 2x tamaño del jugador).")]
        public float dashDistance = 2.0f;

        [Tooltip("Velocidad del dash (m/s). Distancia / Velocidad = duración del dash.")]
        public float dashSpeed = 18f;

        [Tooltip("Cooldown del dash en segundos.")]
        public float dashCooldown = 1.2f;

        [Header("Dash Ease-Out")]
        [Tooltip("Duración del suavizado al terminar el dash (evita 'frenada seca').")]
        public float dashExitBlendTime = 0.12f;

        [Tooltip("Curva de mezcla exponencial hacia la velocidad objetivo (↑ más snappy).")]
        [Range(1f, 30f)] public float dashExitSharpness = 10f;

        // ────────────────────────────────── COMBAT ──────────────────────────────────
        [Header("Combat - Targeting")]
        [Tooltip("Layers considerados como enemigos para los chequeos de hit.")]
        public LayerMask enemyMask = ~0;

        // ───────────────────────────── BASIC COMBO (A1/A2/A3) ───────────────────────
        [Header("Attack (Combo Básico)")]
        [Tooltip("Daño base de cada golpe del combo.")]
        public float attackDamage = 10f;

        [Tooltip("Duración del Ataque 1 (s).")]
        public float attack1Duration = 0.25f;

        [Tooltip("Duración del Ataque 2 (s).")]
        public float attack2Duration = 0.28f;

        [Tooltip("Duración del Ataque 3 (s).")]
        public float attack3Duration = 0.34f;

        [Tooltip("Radio/alcance efectivo frontal del ataque (m).")]
        public float attackRange = 2.0f;

        [Tooltip("Semiancho del cono frontal (grados) para elegir objetivo.")]
        [Range(5f, 90f)] public float attackHalfAngleDegrees = 55f;

        [Tooltip("Ventana para encadenar al siguiente golpe luego de terminar la animación (s).")]
        public float attackChainWindow = 0.30f;

        [Tooltip("Empuje aplicado al enemigo impactado (m).")]
        public float attackKnockbackDistance = 2.5f;

        [Tooltip("Tiempo de 'stagger' del enemigo (s).")]
        public float attackStaggerTime = 0.35f;

        [Tooltip("Cooldown al completar el 3er golpe del combo (s).")]
        public float attackComboCooldown = 0.4f;

        // ───────────────────────────── ATTACK: VERTICAL (AIR) ───────────────────────
        [Header("Attack Vertical (Aéreo)")]
        [Tooltip("Duración total (desde input en aire hasta impacto). Referencia para la animación.")]
        public float verticalAttackDuration = 0.5f;

        [Tooltip("Radio del área de daño al impactar con el suelo (m).")]
        public float verticalAttackRadius = 2.8f;

        [Tooltip("Daño del ataque vertical en área.")]
        public float verticalDamage = 12f;

        [Tooltip("Empuje radial aplicado a cada enemigo impactado (m).")]
        public float verticalKnockbackDistance = 2.5f;

        [Tooltip("Stagger aplicado por el ataque vertical (s).")]
        public float verticalStaggerTime = 0.35f;

        [Tooltip("Cooldown del ataque vertical (s).")]
        public float verticalAttackCooldown = 1.0f;

        [Tooltip("Tiempo que el jugador queda inmóvil luego del impacto (s).")]
        public float verticalAttackPostStun = 0.25f;

        // ───────────────────────────── ATTACK: 360° (CHARGE) ───────────────────────
        [Header("Attack 360° (Carga)")]
        [Tooltip("Tiempo mínimo de carga (mantener presionado) para habilitar el release.")]
        public float spinChargeMinTime = 0.6f;

        [Tooltip("Duración del giro/ejecución del ataque (s).")]
        public float spinDuration = 0.7f;

        [Tooltip("Radio del área de daño del giro (m).")]
        public float spinRadius = 2.8f;

        [Tooltip("Daño del ataque 360° (por evento/‘tick’ de daño).")]
        public float spinDamage = 10f;

        [Tooltip("Empuje radial aplicado en el 360° (m).")]
        public float spinPushDistance = 2.0f;

        [Tooltip("Stagger aplicado por el 360° (s).")]
        public float spinStaggerTime = 0.3f;

        [Tooltip("Cooldown del 360° (s).")]
        public float spinCooldown = 3.0f;

        [Tooltip("Multiplicador de velocidad de movimiento mientras se está cargando (0.6 = 60%).")]
        public float spinMoveSpeedMultiplierWhileCharging = 0.6f;

        [Tooltip("Tiempo que el jugador queda inmóvil luego del release (s).")]
        public float spinPostStun = 0.35f;

        // ───────────────────────────── ACTION MODIFIERS/RUNTIME ─────────────────────
        [Header("Action Modifiers (capa de Acciones)")]
        [Tooltip("Multiplica la velocidad de locomoción por acciones (cargar, etc.).")]
        public float actionMoveSpeedMultiplier = 1f;

        [Tooltip("Bloqueo de orientación (p.ej., fijar mira durante una acción).")]
        public bool aimLockActive = false;

        [Tooltip("Dirección de 'aim lock' cuando está activo (en mundo).")]
        public Vector3 aimLockDirection = Vector3.zero;

        [Tooltip("Bloquea la locomoción (vertical en ejecución, release 360°, etc.).")]
        public bool locomotionBlocked = false;

        [Tooltip("Invulnerabilidad al daño de enemigos (p.ej., durante dash).")]
        public bool invulnerableToEnemies = false;

        // ────────────────────────────────── RUNTIME ─────────────────────────────────
        [Header("Runtime (No editar)")]
        [HideInInspector] public Vector2 rawMoveInput;
        [HideInInspector] public Vector3 moveInputWorld;

        [HideInInspector] public int  jumpsLeft;
        [HideInInspector] public bool jumpWasPureVertical;

        [HideInInspector] public bool dashOnCooldown;
        [HideInInspector] public float dashCooldownLeft;

        [HideInInspector] public bool spinOnCooldown;
        [HideInInspector] public float spinCooldownLeft;

        [HideInInspector] public bool verticalOnCooldown;
        [HideInInspector] public float verticalCooldownLeft;

        [HideInInspector] public bool attackComboOnCooldown;
        [HideInInspector] public float attackComboCooldownLeft;

        // ──────────────────────────────── UTILITIES ────────────────────────────────
        [ContextMenu("Reset Jumps")]
        public void ResetJumps() => jumpsLeft = maxJumps;
        public void ClearActionLocks()
        {
            locomotionBlocked = false;
            aimLockActive = false;
            actionMoveSpeedMultiplier = 1f;
            invulnerableToEnemies = false;
            aimLockDirection = Vector3.zero;
        }
    }
}
