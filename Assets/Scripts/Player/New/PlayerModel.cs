using UnityEngine;
using UnityEngine.Serialization;

namespace Player.New
{
    /// <summary>
    /// Método de orientación del personaje:
    /// - TowardsMovement: mira hacia la dirección de movimiento.
    /// - TowardsCamera: mira hacia el forward de la cámara.
    /// </summary>
    public enum OrientationMethod
    {
        TowardsMovement,
        TowardsCamera
    }

    /// <summary>
    /// ScriptableObject con todos los parámetros del jugador (movimiento, combate, tiempos)
    /// y estado runtime. Los parámetros editables se guardan en campos privados serializados
    /// y se exponen mediante propiedades públicas en UpperCamelCase.
    /// </summary>
    [System.Serializable]
    [CreateAssetMenu(fileName = "NewPlayerModel", menuName = "Models/NewPlayerModel")]
    public class PlayerModel : ScriptableObject
    {
        // =======================================================================

        #region MOVEMENT

        // =======================================================================

        [Header("Movement")] [SerializeField, Tooltip("Velocidad de movimiento en suelo (m/s).")]
        private float moveSpeed = 6f;

        [SerializeField, Tooltip("Aceleración en suelo hacia MoveSpeed (m/s²).")]
        private float moveAcceleration = 30f;

        [SerializeField, Tooltip("Velocidad horizontal máxima en el aire (m/s).")]
        private float airHorizontalSpeed = 6f;

        /// <summary>Velocidad de movimiento en suelo (m/s).</summary>
        public float MoveSpeed
        {
            get => moveSpeed;
            set => moveSpeed = value;
        }

        /// <summary>Aceleración en suelo hacia MoveSpeed (m/s²).</summary>
        public float MoveAcceleration
        {
            get => moveAcceleration;
            set => moveAcceleration = value;
        }

        /// <summary>Velocidad horizontal máxima en el aire (m/s).</summary>
        public float AirHorizontalSpeed
        {
            get => airHorizontalSpeed;
            set => airHorizontalSpeed = value;
        }

        #endregion

        // =======================================================================

        #region SPRINT

        // =======================================================================

        [Header("Sprint")] [SerializeField, Tooltip("Multiplicador de velocidad al correr (moverse*X).")]
        private float sprintSpeedMultiplier = 2.0f;

        [SerializeField, Tooltip("Velocidad horizontal mínima (m/s) para mantener el sprint.")]
        private float sprintMinSpeedToKeep = 0.2f;

        [SerializeField, Tooltip("Ventana tras el dash en la que puedo iniciar sprint manteniendo el botón (s).")]
        private float sprintArmWindow = 0.8f;

        [SerializeField, Tooltip("Tiempo que hay que mantener presionado el botón de Dash para iniciar Sprint (s).")]
        private float sprintHoldTime = 0.25f;

        /// <summary>Multiplicador de velocidad al correr (moverse*X).</summary>
        public float SprintSpeedMultiplier
        {
            get => sprintSpeedMultiplier;
            set => sprintSpeedMultiplier = value;
        }

        /// <summary>Velocidad horizontal mínima (m/s) para mantener el sprint.</summary>
        public float SprintMinSpeedToKeep
        {
            get => sprintMinSpeedToKeep;
            set => sprintMinSpeedToKeep = value;
        }

        /// <summary>Ventana tras el dash para poder iniciar el sprint (s).</summary>
        public float SprintArmWindow
        {
            get => sprintArmWindow;
            set => sprintArmWindow = value;
        }

        /// <summary>Tiempo de holdeo requerido (s) para comenzar el sprint.</summary>
        public float SprintHoldTime
        {
            get => sprintHoldTime;
            set => sprintHoldTime = value;
        }

        #endregion

        // =======================================================================

        #region ORIENTATION

        // =======================================================================


        [Header("Orientation")]
        [SerializeField, Tooltip("Cómo se orienta el personaje: hacia el movimiento o hacia la cámara.")]
        private OrientationMethod orientationMethod = OrientationMethod.TowardsMovement;

        [SerializeField, Range(1f, 30f), Tooltip("Qué tan rápido interpola la rotación hacia el objetivo.")]
        private float orientationSharpness = 12f;

        /// <summary>Método de orientación.</summary>
        public OrientationMethod Orientation
        {
            get => orientationMethod;
            set => orientationMethod = value;
        }

        /// <summary>Sharpness de orientación.</summary>
        public float OrientationSharpness
        {
            get => orientationSharpness;
            set => orientationSharpness = value;
        }

        #endregion

        // =======================================================================

        #region JUMP

        // =======================================================================

        [Header("Jump")] [SerializeField, Tooltip("Impulso vertical de cada salto (afecta la altura/tiempo de vuelo).")]
        private float jumpSpeed = 7.5f;

        [SerializeField, Tooltip("Cantidad total de saltos permitidos (2 = doble salto).")]
        private int maxJumps = 2;

        /// <summary>Impulso vertical del salto.</summary>
        public float JumpSpeed
        {
            get => jumpSpeed;
            set => jumpSpeed = value;
        }

        /// <summary>Cantidad total de saltos permitidos.</summary>
        public int MaxJumps
        {
            get => maxJumps;
            set => maxJumps = value;
        }

        #endregion

        // =======================================================================

        #region DASH

        // =======================================================================

        [Header("Dash")] [SerializeField, Tooltip("Distancia total del dash (consigna: ≈ 2x tamaño del jugador).")]
        private float dashDistance = 2.0f;

        [SerializeField, Tooltip("Velocidad del dash (m/s). Distancia / Velocidad = duración del dash.")]
        private float dashSpeed = 18f;

        [SerializeField, Tooltip("Cooldown del dash en segundos.")]
        private float dashCooldown = 1.2f;

        [Header("Dash Ease-Out")]
        [SerializeField, Tooltip("Duración del suavizado al terminar el dash (evita 'frenada seca').")]
        private float dashExitBlendTime = 0.12f;

        [SerializeField, Range(1f, 30f),
         Tooltip("Curva de mezcla exponencial hacia la velocidad objetivo (↑ más snappy).")]
        private float dashExitSharpness = 10f;

        /// <summary>Distancia total del dash.</summary>
        public float DashDistance
        {
            get => dashDistance;
            set => dashDistance = value;
        }

        /// <summary>Velocidad del dash (m/s).</summary>
        public float DashSpeed
        {
            get => dashSpeed;
            set => dashSpeed = value;
        }

        /// <summary>Cooldown del dash (s).</summary>
        public float DashCooldown
        {
            get => dashCooldown;
            set => dashCooldown = value;
        }

        /// <summary>Blend de salida del dash (s).</summary>
        public float DashExitBlendTime
        {
            get => dashExitBlendTime;
            set => dashExitBlendTime = value;
        }

        /// <summary>Sharpness de salida del dash.</summary>
        public float DashExitSharpness
        {
            get => dashExitSharpness;
            set => dashExitSharpness = value;
        }

        #endregion

        // =======================================================================

        #region COMBAT TARGETING

        // =======================================================================

        [Header("Combat - Targeting")]
        [SerializeField, Tooltip("Layers considerados como enemigos para los chequeos de hit.")]
        private LayerMask enemyMask = ~0;

        /// <summary>Máscara de capas para enemigos.</summary>
        public LayerMask EnemyMask
        {
            get => enemyMask;
            set => enemyMask = value;
        }

        #endregion

        // =======================================================================

        #region BASIC COMBO (A1/A2/A3)

        // =======================================================================

        [Header("Attack (Combo Básico)")] [SerializeField, Tooltip("Daño base de cada golpe del combo.")]
        private float attackDamage = 10f;

        [SerializeField, Tooltip("Duración del Ataque 1 (s).")]
        private float attack1Duration = 0.25f;

        [SerializeField, Tooltip("Duración del Ataque 2 (s).")]
        private float attack2Duration = 0.28f;

        [SerializeField, Tooltip("Duración del Ataque 3 (s).")]
        private float attack3Duration = 0.34f;

        [SerializeField, Tooltip("Radio/alcance efectivo frontal del ataque (m).")]
        private float attackRange = 2.0f;

        [SerializeField, Range(5f, 90f), Tooltip("Semiancho del cono frontal (grados) para elegir objetivo.")]
        private float attackHalfAngleDegrees = 55f;

        [SerializeField, Tooltip("Ventana para encadenar al siguiente golpe luego de terminar la animación (s).")]
        private float attackChainWindow = 0.30f;

        [SerializeField, Tooltip("Empuje aplicado al enemigo impactado (m).")]
        private float attackKnockbackDistance = 2.5f;

        [SerializeField, Tooltip("Tiempo de 'stagger' del enemigo (s).")]
        private float attackStaggerTime = 0.35f;

        [SerializeField, Tooltip("Cooldown al completar el 3er golpe del combo (s).")]
        private float attackComboCooldown = 0.4f;

        public float AttackDamage
        {
            get => attackDamage;
            set => attackDamage = value;
        }

        public float Attack1Duration
        {
            get => attack1Duration;
            set => attack1Duration = value;
        }

        public float Attack2Duration
        {
            get => attack2Duration;
            set => attack2Duration = value;
        }

        public float Attack3Duration
        {
            get => attack3Duration;
            set => attack3Duration = value;
        }

        public float AttackRange
        {
            get => attackRange;
            set => attackRange = value;
        }

        public float AttackHalfAngleDegrees
        {
            get => attackHalfAngleDegrees;
            set => attackHalfAngleDegrees = value;
        }

        public float AttackChainWindow
        {
            get => attackChainWindow;
            set => attackChainWindow = value;
        }

        public float AttackKnockbackDistance
        {
            get => attackKnockbackDistance;
            set => attackKnockbackDistance = value;
        }

        public float AttackStaggerTime
        {
            get => attackStaggerTime;
            set => attackStaggerTime = value;
        }

        public float AttackComboCooldown
        {
            get => attackComboCooldown;
            set => attackComboCooldown = value;
        }

        #endregion

        // =======================================================================

        #region ATTACK: VERTICAL (AIR)

        // =======================================================================
        
        [Header("Attack Vertical (Aéreo)")]
        [SerializeField, Tooltip("Radio del área de daño al impactar con el suelo (m).")]
        private float verticalAttackRadius = 2.8f;

        [SerializeField, Tooltip("Daño del ataque vertical en área.")]
        private float verticalDamage = 12f;

        [SerializeField, Tooltip("Empuje radial aplicado a cada enemigo impactado (m).")]
        private float verticalKnockbackDistance = 2.5f;

        [SerializeField, Tooltip("Stagger aplicado por el ataque vertical (s).")]
        private float verticalStaggerTime = 0.35f;

        [SerializeField, Tooltip("Cooldown del ataque vertical (s).")]
        private float verticalAttackCooldown = 1.0f;

        [SerializeField, Tooltip("Tiempo que el jugador queda inmóvil luego del impacto (s).")]
        private float verticalAttackPostStun = 0.25f;

        [SerializeField, Tooltip("Aceleración extra hacia abajo durante el vertical (m/s²).")]
        private float verticalSlamExtraAccel = 40f;

        [SerializeField, Tooltip("Límite de velocidad hacia abajo durante el vertical (m/s).")]
        private float verticalSlamMaxDownSpeed = 30f;

        [SerializeField, Tooltip("Impulso inicial mínimo hacia abajo al empezar el vertical (m/s). 0 = sin impulso.")]
        private float verticalSlamStartDownSpeed = 0f;

        public float VerticalAttackRadius
        {
            get => verticalAttackRadius;
            set => verticalAttackRadius = value;
        }

        public float VerticalDamage
        {
            get => verticalDamage;
            set => verticalDamage = value;
        }

        public float VerticalKnockbackDistance
        {
            get => verticalKnockbackDistance;
            set => verticalKnockbackDistance = value;
        }

        public float VerticalStaggerTime
        {
            get => verticalStaggerTime;
            set => verticalStaggerTime = value;
        }

        public float VerticalAttackCooldown
        {
            get => verticalAttackCooldown;
            set => verticalAttackCooldown = value;
        }

        public float VerticalAttackPostStun
        {
            get => verticalAttackPostStun;
            set => verticalAttackPostStun = value;
        }

        public float VerticalSlamExtraAccel
        {
            get => verticalSlamExtraAccel;
            set => verticalSlamExtraAccel = value;
        }

        public float VerticalSlamMaxDownSpeed
        {
            get => verticalSlamMaxDownSpeed;
            set => verticalSlamMaxDownSpeed = value;
        }

        public float VerticalSlamStartDownSpeed
        {
            get => verticalSlamStartDownSpeed;
            set => verticalSlamStartDownSpeed = value;
        }

        #endregion

        // =======================================================================

        #region ATTACK 360° (SPIN)

        // =======================================================================


        [Header("Attack 360° (Carga/Release)")]
        [SerializeField, Tooltip("Tiempo mínimo de carga (mantener presionado) para habilitar el release.")]
        private float spinChargeMinTime = 0.6f;

        [SerializeField, Tooltip("Tiempo de carga para alcanzar potencia/duración máximas.")]
        private float spinChargeMaxTime = 1.6f;

        [SerializeField, Tooltip("Multiplicador de velocidad MIENTRAS se ejecuta el Spin (no bloquea locomoción).")]
        private float spinMoveSpeedMultiplierWhileExecuting = 0.6f;

        [SerializeField, Tooltip("Multiplicador de velocidad de SALTO mientras dura el SpinRelease.")]
        private float spinJumpSpeedMultiplier = 1.15f;

        [Header("Spin: duraciones escalables")] [SerializeField, Tooltip("Duración mínima del giro (s).")]
        private float spinMinDuration = 0.55f;

        [SerializeField, Tooltip("Duración máxima del giro (s).")]
        private float spinMaxDuration = 1.10f;

        [SerializeField, Tooltip("Duración base (legacy). No se usa cuando hay carga escalable.")]
        private float spinDuration = 0.7f;

        [SerializeField, Tooltip("Tiempo que el jugador queda inmóvil luego del release (s).")]
        private float spinPostStun = 0.35f;

        [SerializeField, Tooltip("Radio del área de daño del giro (m).")]
        private float spinRadius = 2.8f;

        [SerializeField, Tooltip("Daño del ataque 360° (por evento/‘tick’ de daño).")]
        private float spinDamage = 10f;

        [SerializeField, Tooltip("Empuje radial aplicado en el 360° (m).")]
        private float spinPushDistance = 2.0f;

        [SerializeField, Tooltip("Stagger aplicado por el 360° (s).")]
        private float spinStaggerTime = 0.3f;

        [SerializeField, Tooltip("Cooldown del 360° (s).")]
        private float spinCooldown = 3.0f;

        [SerializeField, Tooltip("Multiplicador de velocidad de movimiento mientras se está cargando (0.6 = 60%).")]
        private float spinMoveSpeedMultiplierWhileCharging = 0.6f;


        [Header("Self Stun por Spin (escalable)")]
        [SerializeField, Tooltip("Luego del spin, el jugador queda knockdown.")]
        private bool spinCausesSelfStun = true;

        [SerializeField, Tooltip("Derribo mínimo (s).")]
        private float selfStunMinDuration = 0.60f;

        [SerializeField, Tooltip("Derribo máximo (s).")]
        private float selfStunMaxDuration = 2.00f;

        [SerializeField, Tooltip("Tiempo antes de terminar el derribo para disparar 'GetUp' (s).")]
        private float selfStunGetUpLeadTime = 0.25f;

        public float SpinChargeMinTime
        {
            get => spinChargeMinTime;
            set => spinChargeMinTime = value;
        }

        public float SpinChargeMaxTime
        {
            get => spinChargeMaxTime;
            set => spinChargeMaxTime = value;
        }

        public float SpinMoveSpeedMultiplierWhileExecuting
        {
            get => spinMoveSpeedMultiplierWhileExecuting;
            set => spinMoveSpeedMultiplierWhileExecuting = value;
        }

        public float SpinJumpSpeedMultiplier
        {
            get => spinJumpSpeedMultiplier;
            set => spinJumpSpeedMultiplier = value;
        }

        public float SpinMinDuration
        {
            get => spinMinDuration;
            set => spinMinDuration = value;
        }

        public float SpinMaxDuration
        {
            get => spinMaxDuration;
            set => spinMaxDuration = value;
        }

        public float SpinDuration
        {
            get => spinDuration;
            set => spinDuration = value;
        }

        public float SpinPostStun
        {
            get => spinPostStun;
            set => spinPostStun = value;
        }

        public float SpinRadius
        {
            get => spinRadius;
            set => spinRadius = value;
        }

        public float SpinDamage
        {
            get => spinDamage;
            set => spinDamage = value;
        }

        public float SpinPushDistance
        {
            get => spinPushDistance;
            set => spinPushDistance = value;
        }

        public float SpinStaggerTime
        {
            get => spinStaggerTime;
            set => spinStaggerTime = value;
        }

        public float SpinCooldown
        {
            get => spinCooldown;
            set => spinCooldown = value;
        }

        public float SpinMoveSpeedMultiplierWhileCharging
        {
            get => spinMoveSpeedMultiplierWhileCharging;
            set => spinMoveSpeedMultiplierWhileCharging = value;
        }

        public bool SpinCausesSelfStun
        {
            get => spinCausesSelfStun;
            set => spinCausesSelfStun = value;
        }

        public float SelfStunMinDuration
        {
            get => selfStunMinDuration;
            set => selfStunMinDuration = value;
        }

        public float SelfStunMaxDuration
        {
            get => selfStunMaxDuration;
            set => selfStunMaxDuration = value;
        }

        public float SelfStunGetUpLeadTime
        {
            get => selfStunGetUpLeadTime;
            set => selfStunGetUpLeadTime = value;
        }

        #endregion

        // =======================================================================

        #region ACTION MODIFIERS (LOCKS)

        // =======================================================================

        [Header("Action Modifiers (capa de Acciones)")]
        [SerializeField, Tooltip("Multiplica la velocidad de locomoción por acciones (cargar, etc.).")]
        private float actionMoveSpeedMultiplier = 1f;

        [SerializeField, Tooltip("Multiplica la velocidad/impulso de salto por acciones (spin, etc.).")]
        private float actionJumpSpeedMultiplier = 1f;

        [SerializeField, Tooltip("Bloquea la locomoción (vertical en ejecución, release 360°, etc.).")]
        private bool locomotionBlocked = false;

        [SerializeField, Tooltip("Invulnerabilidad al daño de enemigos (p.ej., durante dash).")]
        private bool invulnerableToEnemies = false;

        [SerializeField, Tooltip("Bloqueo de orientación (p.ej., fijar mira durante una acción).")]
        private bool aimLockActive = false;

        [SerializeField, Tooltip("Dirección de 'aim lock' cuando está activo (en mundo).")]
        private Vector3 aimLockDirection = Vector3.zero;

        public float ActionMoveSpeedMultiplier
        {
            get => actionMoveSpeedMultiplier;
            set => actionMoveSpeedMultiplier = value;
        }

        public float ActionJumpSpeedMultiplier
        {
            get => actionJumpSpeedMultiplier;
            set => actionJumpSpeedMultiplier = value;
        }

        public bool LocomotionBlocked
        {
            get => locomotionBlocked;
            set => locomotionBlocked = value;
        }

        public bool InvulnerableToEnemies
        {
            get => invulnerableToEnemies;
            set => invulnerableToEnemies = value;
        }

        public bool AimLockActive
        {
            get => aimLockActive;
            set => aimLockActive = value;
        }

        public Vector3 AimLockDirection
        {
            get => aimLockDirection;
            set => aimLockDirection = value;
        }

        #endregion

        // =======================================================================

        #region RUNTIME (NO SERIALIZAR)

        // =======================================================================

        [Header("Runtime (No editar)")] [System.NonSerialized]
        private Vector2 _rawMoveInput;

        [System.NonSerialized] private Vector3 _moveInputWorld;

        [System.NonSerialized] private int _jumpsLeft;
        [System.NonSerialized] private bool _jumpWasPureVertical;

        [System.NonSerialized] private bool _dashOnCooldown;
        [System.NonSerialized] private float _dashCooldownLeft;

        [System.NonSerialized] private bool _spinOnCooldown;
        [System.NonSerialized] private float _spinCooldownLeft;

        [System.NonSerialized] private bool _verticalOnCooldown;
        [System.NonSerialized] private float _verticalCooldownLeft;

        [System.NonSerialized] private bool _attackComboOnCooldown;
        [System.NonSerialized] private float _attackComboCooldownLeft;

        [System.NonSerialized] private float _spinChargeRatio;
        [System.NonSerialized] private bool _isSelfStunned;
        [System.NonSerialized] private float _selfStunTimeLeft;
        [System.NonSerialized] private float _selfStunDuration;

        [System.NonSerialized] private bool _sprintArmed;
        [System.NonSerialized] private bool _dashHeld;
        [System.NonSerialized] private float _sprintArmTimeLeft;
        [System.NonSerialized] private float _sprintHoldCounter;

        /// <summary>Input crudo de movimiento (Vector2).</summary>
        public Vector2 RawMoveInput
        {
            get => _rawMoveInput;
            set => _rawMoveInput = value;
        }

        /// <summary>Dirección de movimiento en mundo (Vector3).</summary>
        public Vector3 MoveInputWorld
        {
            get => _moveInputWorld;
            set => _moveInputWorld = value;
        }

        /// <summary>Saltos restantes.</summary>
        public int JumpsLeft
        {
            get => _jumpsLeft;
            set => _jumpsLeft = value;
        }

        /// <summary>Si el salto fue sin input horizontal.</summary>
        public bool JumpWasPureVertical
        {
            get => _jumpWasPureVertical;
            set => _jumpWasPureVertical = value;
        }

        public bool DashOnCooldown
        {
            get => _dashOnCooldown;
            set => _dashOnCooldown = value;
        }

        public float DashCooldownLeft
        {
            get => _dashCooldownLeft;
            set => _dashCooldownLeft = value;
        }

        public bool SpinOnCooldown
        {
            get => _spinOnCooldown;
            set => _spinOnCooldown = value;
        }

        public float SpinCooldownLeft
        {
            get => _spinCooldownLeft;
            set => _spinCooldownLeft = value;
        }

        public bool VerticalOnCooldown
        {
            get => _verticalOnCooldown;
            set => _verticalOnCooldown = value;
        }

        public float VerticalCooldownLeft
        {
            get => _verticalCooldownLeft;
            set => _verticalCooldownLeft = value;
        }

        public bool AttackComboOnCooldown
        {
            get => _attackComboOnCooldown;
            set => _attackComboOnCooldown = value;
        }

        public float AttackComboCooldownLeft
        {
            get => _attackComboCooldownLeft;
            set => _attackComboCooldownLeft = value;
        }

        public float SpinChargeRatio
        {
            get => _spinChargeRatio;
            set => _spinChargeRatio = value;
        }

        public bool IsSelfStunned
        {
            get => _isSelfStunned;
            set => _isSelfStunned = value;
        }

        public float SelfStunTimeLeft
        {
            get => _selfStunTimeLeft;
            set => _selfStunTimeLeft = value;
        }

        public float SelfStunDuration
        {
            get => _selfStunDuration;
            set => _selfStunDuration = value;
        }

        public bool SprintArmed
        {
            get => _sprintArmed;
            set => _sprintArmed = value;
        }

        public bool DashHeld
        {
            get => _dashHeld;
            set => _dashHeld = value;
        }

        public float SprintArmTimeLeft
        {
            get => _sprintArmTimeLeft;
            set => _sprintArmTimeLeft = value;
        }

        public float SprintHoldCounter
        {
            get => _sprintHoldCounter;
            set => _sprintHoldCounter = value;
        }

        #endregion

        // =======================================================================

        #region UTILITIES

        // =======================================================================

        /// <summary>Resetea la cantidad de saltos disponibles al máximo configurado.</summary>
        [ContextMenu("Reset Jumps")]
        public void ResetJumps() => _jumpsLeft = maxJumps;

        /// <summary>Limpia locks/flags de acción y devuelve multiplicadores a 1.</summary>
        public void ClearActionLocks()
        {
            locomotionBlocked = false;
            aimLockActive = false;
            actionMoveSpeedMultiplier = 1f;
            actionJumpSpeedMultiplier = 1f;
            invulnerableToEnemies = false;
            aimLockDirection = Vector3.zero;
        }

        /// <summary>Abre la ventana para iniciar Sprint (se usa al terminar el dash).</summary>
        public void BeginSprintWindow()
        {
            _sprintArmed = true;
            _sprintArmTimeLeft = sprintArmWindow;
            _sprintHoldCounter = 0f;
        }

        /// <summary>Cierra la ventana para iniciar Sprint y limpia contadores.</summary>
        public void CloseSprintWindow()
        {
            _sprintArmed = false;
            _sprintArmTimeLeft = 0f;
            _sprintHoldCounter = 0f;
        }

        /// <summary>Resetea todos los cooldowns (útil para debug/cheats).</summary>
        public void ResetAllCooldowns()
        {
            _dashOnCooldown = false;
            _dashCooldownLeft = 0f;
            _spinOnCooldown = false;
            _spinCooldownLeft = 0f;
            _verticalOnCooldown = false;
            _verticalCooldownLeft = 0f;
            _attackComboOnCooldown = false;
            _attackComboCooldownLeft = 0f;
        }

        #endregion
    }
}