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

        // ─────────────────────────────── SPRINT ───────────────────────────────
        [Header("Sprint")]
        [Tooltip("Multiplicador de velocidad al correr (moverse*X).")]
        public float sprintSpeedMultiplier = 2.0f;

        [Tooltip("Velocidad horizontal mínima (m/s) para mantener el sprint.")]
        public float sprintMinSpeedToKeep = 0.2f;

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

        [Tooltip("Aceleración extra hacia abajo durante el vertical (m/s²).")]
        public float verticalSlamExtraAccel = 40f;

        [Tooltip("Límite de velocidad hacia abajo durante el vertical (m/s).")]
        public float verticalSlamMaxDownSpeed = 30f;

        [Tooltip("Impulso inicial mínimo hacia abajo al empezar el vertical (m/s). 0 = sin impulso.")]
        public float verticalSlamStartDownSpeed = 0f;

        // ───────────────────────────── ATTACK: 360° (CHARGE) ───────────────────────
        [Header("Attack 360° (Carga)")]
        [Tooltip("Tiempo mínimo de carga (mantener presionado) para habilitar el release.")]
        public float spinChargeMinTime = 0.6f;

        [Tooltip("Tiempo de carga para alcanzar potencia/duración máximas.")]
        public float spinChargeMaxTime = 1.6f;

        [Tooltip("Multiplicador de velocidad MIENTRAS se ejecuta el Spin (no bloquea locomoción).")]
        public float spinMoveSpeedMultiplierWhileExecuting = 0.6f;

        [Tooltip("Multiplicador de velocidad de SALTO mientras dura el SpinRelease.")]
        public float spinJumpSpeedMultiplier = 1.15f;

        [Header("Spin: duraciones escalables")]
        [Tooltip("Duración mínima del giro (s).")]
        public float spinMinDuration = 0.55f;

        [Tooltip("Duración máxima del giro (s).")]
        public float spinMaxDuration = 1.10f;

        [Tooltip("Duración base (legacy). No se usa cuando hay carga escalable.")]
        public float spinDuration = 0.7f;

        [Tooltip("Tiempo que el jugador queda inmóvil luego del release (s).")]
        public float spinPostStun = 0.35f;

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

        [Header("Self Stun por Spin (escalable)")]
        [Tooltip("Luego del spin, el jugador queda knockdown.")]
        public bool spinCausesSelfStun = true;

        [Tooltip("Derribo mínimo (s).")]
        public float selfStunMinDuration = 0.60f;

        [Tooltip("Derribo máximo (s).")]
        public float selfStunMaxDuration = 2.00f;

        [Tooltip("Tiempo antes de terminar el derribo para disparar 'GetUp' (s).")]
        public float selfStunGetUpLeadTime = 0.25f;

        // ───────────────────────────── ACTION MODIFIERS/RUNTIME ─────────────────────
        [Header("Action Modifiers (capa de Acciones)")]
        [Tooltip("Multiplica la velocidad de locomoción por acciones (cargar, etc.).")]
        public float actionMoveSpeedMultiplier = 1f;

        [Tooltip("Multiplica la velocidad/impulso de salto por acciones (spin, etc.).")]
        public float actionJumpSpeedMultiplier = 1f;

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

        [HideInInspector] public float spinChargeRatio;
        [HideInInspector] public bool  isSelfStunned;
        [HideInInspector] public float selfStunTimeLeft;
        [HideInInspector] public float selfStunDuration;

        [HideInInspector] public bool sprintArmed;
        [HideInInspector] public bool dashHeld;

        // ──────────────────────────────── UTILITIES ────────────────────────────────
        [ContextMenu("Reset Jumps")] public void ResetJumps() => jumpsLeft = maxJumps;

        public void ClearActionLocks()
        {
            locomotionBlocked = false;
            aimLockActive = false;
            actionMoveSpeedMultiplier = 1f;
            actionJumpSpeedMultiplier = 1f; // ← reset del salto
            invulnerableToEnemies = false;
            aimLockDirection = Vector3.zero;
        }

        // ───────────────────────────── Legacy API (PascalCase) ─────────────────────
        // Movement
        public float MoveSpeed { get => moveSpeed; set => moveSpeed = value; }
        public float MoveAcceleration { get => moveAcceleration; set => moveAcceleration = value; }
        public float AirHorizontalSpeed { get => airHorizontalSpeed; set => airHorizontalSpeed = value; }

        // Jump
        public float JumpSpeed { get => jumpSpeed; set => jumpSpeed = value; }
        public int MaxJumps { get => maxJumps; set => maxJumps = value; }

        // Dash
        public float DashDistance { get => dashDistance; set => dashDistance = value; }
        public float DashSpeed { get => dashSpeed; set => dashSpeed = value; }
        public float DashCooldown { get => dashCooldown; set => dashCooldown = value; }
        public float DashExitBlendTime { get => dashExitBlendTime; set => dashExitBlendTime = value; }
        public float DashExitSharpness { get => dashExitSharpness; set => dashExitSharpness = value; }
        public bool  DashOnCooldown { get => dashOnCooldown; set => dashOnCooldown = value; }
        public float DashCooldownLeft { get => dashCooldownLeft; set => dashCooldownLeft = value; }

        // Basic combo
        public float AttackDamage { get => attackDamage; set => attackDamage = value; }
        public float Attack1Duration { get => attack1Duration; set => attack1Duration = value; }
        public float Attack2Duration { get => attack2Duration; set => attack2Duration = value; }
        public float Attack3Duration { get => attack3Duration; set => attack3Duration = value; }
        public float AttackRange { get => attackRange; set => attackRange = value; }
        public float AttackHalfAngleDegrees { get => attackHalfAngleDegrees; set => attackHalfAngleDegrees = value; }
        public float AttackChainWindow { get => attackChainWindow; set => attackChainWindow = value; }
        public float AttackKnockbackDistance { get => attackKnockbackDistance; set => attackKnockbackDistance = value; }
        public float AttackStaggerTime { get => attackStaggerTime; set => attackStaggerTime = value; }
        public float AttackComboCooldown { get => attackComboCooldown; set => attackComboCooldown = value; }
        public bool  AttackComboOnCooldown { get => attackComboOnCooldown; set => attackComboOnCooldown = value; }
        public float AttackComboCooldownLeft { get => attackComboCooldownLeft; set => attackComboCooldownLeft = value; }

        // Vertical
        public float VerticalAttackRadius { get => verticalAttackRadius; set => verticalAttackRadius = value; }
        public float VerticalDamage { get => verticalDamage; set => verticalDamage = value; }
        public float VerticalKnockbackDistance { get => verticalKnockbackDistance; set => verticalKnockbackDistance = value; }
        public float VerticalStaggerTime { get => verticalStaggerTime; set => verticalStaggerTime = value; }
        public float VerticalAttackCooldown { get => verticalAttackCooldown; set => verticalAttackCooldown = value; }
        public float VerticalAttackPostStun { get => verticalAttackPostStun; set => verticalAttackPostStun = value; }
        public float VerticalSlamExtraAccel { get => verticalSlamExtraAccel; set => verticalSlamExtraAccel = value; }
        public float VerticalSlamMaxDownSpeed { get => verticalSlamMaxDownSpeed; set => verticalSlamMaxDownSpeed = value; }
        public float VerticalSlamStartDownSpeed { get => verticalSlamStartDownSpeed; set => verticalSlamStartDownSpeed = value; }
        public bool  VerticalOnCooldown { get => verticalOnCooldown; set => verticalOnCooldown = value; }
        public float VerticalCooldownLeft { get => verticalCooldownLeft; set => verticalCooldownLeft = value; }

        // Spin (base + escalable)
        public float SpinChargeMinTime { get => spinChargeMinTime; set => spinChargeMinTime = value; }
        public float SpinChargeMaxTime { get => spinChargeMaxTime; set => spinChargeMaxTime = value; }
        public float SpinMinDuration   { get => spinMinDuration;   set => spinMinDuration = value; }
        public float SpinMaxDuration   { get => spinMaxDuration;   set => spinMaxDuration = value; }
        public float SpinDuration      { get => spinDuration;      set => spinDuration = value; }
        public float SpinPostStun      { get => spinPostStun;      set => spinPostStun = value; }
        public float SpinRadius        { get => spinRadius;        set => spinRadius = value; }
        public float SpinDamage        { get => spinDamage;        set => spinDamage = value; }
        public float SpinPushDistance  { get => spinPushDistance;  set => spinPushDistance = value; }
        public float SpinStaggerTime   { get => spinStaggerTime;   set => spinStaggerTime = value; }
        public float SpinCooldown      { get => spinCooldown;      set => spinCooldown = value; }
        public float SpinMoveSpeedMultiplierWhileCharging { get => spinMoveSpeedMultiplierWhileCharging; set => spinMoveSpeedMultiplierWhileCharging = value; }
        public bool  SpinOnCooldown { get => spinOnCooldown; set => spinOnCooldown = value; }
        public float SpinCooldownLeft { get => spinCooldownLeft; set => spinCooldownLeft = value; }
        public bool  SpinCausesSelfStun { get => spinCausesSelfStun; set => spinCausesSelfStun = value; }
        public float SelfStunMinDuration { get => selfStunMinDuration; set => selfStunMinDuration = value; }
        public float SelfStunMaxDuration { get => selfStunMaxDuration; set => selfStunMaxDuration = value; }
        public float SelfStunGetUpLeadTime { get => selfStunGetUpLeadTime; set => selfStunGetUpLeadTime = value; }

        // Targeting
        public LayerMask EnemyMask { get => enemyMask; set => enemyMask = value; }

        // Inputs cache
        public Vector2 RawMoveInput { get => rawMoveInput; set => rawMoveInput = value; }
        public Vector3 MoveInputWorld { get => moveInputWorld; set => moveInputWorld = value; }
        public bool   JumpWasPureVertical { get => jumpWasPureVertical; set => jumpWasPureVertical = value; }
        public int    JumpsLeft { get => jumpsLeft; set => jumpsLeft = value; }

        // Locks
        public float ActionMoveSpeedMultiplier { get => actionMoveSpeedMultiplier; set => actionMoveSpeedMultiplier = value; }
        public float ActionJumpSpeedMultiplier { get => actionJumpSpeedMultiplier; set => actionJumpSpeedMultiplier = value; }
        public bool  AimLockActive { get => aimLockActive; set => aimLockActive = value; }
        public Vector3 AimLockDirection { get => aimLockDirection; set => aimLockDirection = value; }
        public bool  LocomotionBlocked { get => locomotionBlocked; set => locomotionBlocked = value; }
        public bool  InvulnerableToEnemies { get => invulnerableToEnemies; set => invulnerableToEnemies = value; }
        public OrientationMethod Orientation { get => orientationMethod; set => orientationMethod = value; }
        public float OrientationSharpness { get => orientationSharpness; set => orientationSharpness = value; }

        // Spin runtime wrappers
        public float SpinChargeRatio { get => spinChargeRatio; set => spinChargeRatio = value; }
        public bool  IsSelfStunned   { get => isSelfStunned;  set => isSelfStunned = value; }
        public float SelfStunTimeLeft{ get => selfStunTimeLeft; set => selfStunTimeLeft = value; }
        public float SelfStunDuration{ get => selfStunDuration; set => selfStunDuration = value; }
    }
}
