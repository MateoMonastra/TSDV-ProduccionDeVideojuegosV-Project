using Health;
using UnityEngine;

namespace Player.New
{
    /// <summary>
    /// Define todos los parámetros del jugador (expuestos en el Inspector) y
    /// también el estado runtime (no serializado). Los parámetros se guardan
    /// en campos privados con [SerializeField] y se exponen a través de
    /// propiedades públicas en UpperCamelCase.
    /// </summary>
    [CreateAssetMenu(fileName = "NewPlayerModel", menuName = "Models/NewPlayerModel")]
    public class PlayerModel : ScriptableObject
    {
        // ───────────────────────────────────────────────────────────────────────

        #region Movement (ground & air)

        // ───────────────────────────────────────────────────────────────────────

        [Header("Movement")] [SerializeField, Tooltip("Velocidad de movimiento en suelo (m/s).")]
        private float moveSpeed = 6f;

        [SerializeField, Tooltip("Aceleración en suelo hacia MoveSpeed (m/s²).")]
        private float moveAcceleration = 30f;

        [SerializeField, Tooltip("Velocidad horizontal máxima en el aire (m/s).")]
        private float airHorizontalSpeed = 6f;

        [SerializeField,
         Tooltip("Al aterrizar, ¿anular el horizontal? (true = frena en seco; false = conserva momentum)")]
        private bool landStopsHorizontal = false;

        /// <summary>Si true, al aterrizar se anula la velocidad horizontal; si false, se conserva.</summary>
        public bool LandStopsHorizontal
        {
            get => landStopsHorizontal;
            set => landStopsHorizontal = value;
        }

        /// <summary>Velocidad de movimiento en suelo (m/s).</summary>
        public float MoveSpeed
        {
            get => moveSpeed;
            set => moveSpeed = value;
        }

        /// <summary>Aceleración en suelo hacia <see cref="MoveSpeed"/> (m/s²).</summary>
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

        // ───────────────────────────────────────────────────────────────────────

        #region Sprint (hold window + behavior)

        // ───────────────────────────────────────────────────────────────────────

        [Header("Sprint")] [SerializeField, Tooltip("Multiplicador de velocidad al correr (moverse * X).")]
        private float sprintSpeedMultiplier = 2.0f;

        [SerializeField, Tooltip("Velocidad horizontal mínima (m/s) para mantener el sprint.")]
        private float sprintMinSpeedToKeep = 0.2f;

        [SerializeField, Tooltip("Ventana tras el dash en la que puedo iniciar sprint manteniendo el botón (s).")]
        private float sprintArmWindow = 0.8f;

        [SerializeField, Tooltip("Tiempo que hay que mantener presionado el botón de Dash para iniciar Sprint (s).")]
        private float sprintHoldTime = 0.25f;

        /// <summary>Multiplicador de velocidad al correr (moverse * X).</summary>
        public float SprintSpeedMultiplier
        {
            get => sprintSpeedMultiplier;
            set => sprintSpeedMultiplier = value;
        }

        /// <summary>Velocidad horizontal mínima (m/s) para no cortar el sprint.</summary>
        public float SprintMinSpeedToKeep
        {
            get => sprintMinSpeedToKeep;
            set => sprintMinSpeedToKeep = value;
        }

        /// <summary>Duración de la ventana post-dash para armar el sprint.</summary>
        public float SprintArmWindow
        {
            get => sprintArmWindow;
            set => sprintArmWindow = value;
        }

        /// <summary>Tiempo de hold requerido para que el sprint comience.</summary>
        public float SprintHoldTime
        {
            get => sprintHoldTime;
            set => sprintHoldTime = value;
        }

        #endregion

        // ───────────────────────────────────────────────────────────────────────

        #region Orientation

        // ───────────────────────────────────────────────────────────────────────

        public enum OrientationMethod
        {
            TowardsMovement,
            TowardsCamera
        }

        [Header("Orientation")]
        [SerializeField, Tooltip("Cómo se orienta el personaje: hacia el movimiento o hacia la cámara.")]
        private OrientationMethod orientationMethod = OrientationMethod.TowardsMovement;

        [SerializeField, Range(1f, 30f), Tooltip("Qué tan rápido interpola la rotación hacia el objetivo.")]
        private float orientationSharpness = 12f;

        [SerializeField,
         Tooltip(
             "Si es true, el personaje orienta con la cámara cuando está quieto. Si es false, en idle NO gira al mover la cámara.")]
        private bool orientWithCameraWhileIdle = false;

        public bool OrientWithCameraWhileIdle
        {
            get => orientWithCameraWhileIdle;
            set => orientWithCameraWhileIdle = value;
        }

        /// <summary>Método de orientación (movimiento/cámara).</summary>
        public OrientationMethod Orientation
        {
            get => orientationMethod;
            set => orientationMethod = value;
        }

        /// <summary>Sharpness de rotación hacia el objetivo.</summary>
        public float OrientationSharpness
        {
            get => orientationSharpness;
            set => orientationSharpness = value;
        }

        #endregion

        // ───────────────────────────────────────────────────────────────────────

        #region Jump

        // ───────────────────────────────────────────────────────────────────────

        [Header("Jump")] [SerializeField, Tooltip("Impulso vertical del salto (m/s).")]
        private float jumpSpeed = 7.5f;

        [SerializeField, Tooltip("Cantidad total de saltos permitidos (2 = doble salto).")]
        private int maxJumps = 2;

        /// <summary>Impulso vertical del salto (m/s).</summary>
        public float JumpSpeed
        {
            get => jumpSpeed;
            set => jumpSpeed = value;
        }

        /// <summary>Número máximo de saltos (ej.: 2 para doble salto).</summary>
        public int MaxJumps
        {
            get => maxJumps;
            set => maxJumps = value;
        }

        #endregion

        // ───────────────────────────────────────────────────────────────────────

        #region Dash (+ ease-out)

        // ───────────────────────────────────────────────────────────────────────

        [Header("Dash")] [SerializeField, Tooltip("Distancia total del dash (m).")]
        private float dashDistance = 2.0f;

        [SerializeField, Tooltip("Velocidad del dash (m/s).")]
        private float dashSpeed = 18f;

        [SerializeField, Tooltip("Cooldown del dash (s).")]
        private float dashCooldown = 1.2f;

        [Header("Dash Ease-Out")]
        [SerializeField, Tooltip("Tiempo de mezclado al finalizar el dash (evita frenada seca).")]
        private float dashExitBlendTime = 0.12f;

        [SerializeField, Range(1f, 30f), Tooltip("Sharpness del ease-out (↑ más snappy).")]
        private float dashExitSharpness = 10f;

        /// <summary>Distancia total del dash (m).</summary>
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

        /// <summary>Duración del ease-out al finalizar el dash (s).</summary>
        public float DashExitBlendTime
        {
            get => dashExitBlendTime;
            set => dashExitBlendTime = value;
        }

        /// <summary>Sharpness del ease-out del dash.</summary>
        public float DashExitSharpness
        {
            get => dashExitSharpness;
            set => dashExitSharpness = value;
        }

        #endregion

        // ───────────────────────────────────────────────────────────────────────

        #region Pickups

        [Header("Pickups")] [SerializeField, Tooltip("¿Tiene un salto extra pendiente de uso?")]
        private bool hasExtraJump = false;

        [SerializeField, Tooltip("¿Hay un dash buff pendiente para el próximo dash?")]
        private bool dashBuffPending = false;

        [SerializeField, Tooltip("Distancia total del PRÓXIMO dash cuando hay buff (m).")]
        private float dashBuffDistance = 3.0f;

        [SerializeField, Tooltip("Velocidad del PRÓXIMO dash cuando hay buff (m/s).")]
        private float dashBuffSpeed = 24f;

        public float DashBuffDistance
        {
            get => dashBuffDistance;
            set => dashBuffDistance = value;
        }

        public float DashBuffSpeed
        {
            get => dashBuffSpeed;
            set => dashBuffSpeed = value;
        }

        /// <summary>True si el jugador tiene un salto extra pendiente.</summary>
        public bool HasExtraJump
        {
            get => hasExtraJump;
            set => hasExtraJump = value;
        }

        /// <summary>True si el próximo dash debe usar el buff de distancia.</summary>
        public bool DashBuffPending
        {
            get => dashBuffPending;
            set => dashBuffPending = value;
        }

        #endregion

        // ───────────────────────────────────────────────────────────────────────

        #region Daño recibido (hit)
        [Header("Hit (daño recibido)")]
        [SerializeField, Tooltip("Tiempo de stun al ser golpeado (s)")]
        private float hitStunTime = 0.35f;

        [SerializeField, Tooltip("Impulso horizontal aplicado al ser golpeado (m/s)")]
        private float hitKnockbackHorizontal = 7.5f;

        [SerializeField, Tooltip("Impulso vertical aplicado al ser golpeado (m/s)")]
        private float hitKnockbackUp = 2.0f;

        public float HitStunTime => hitStunTime;
        public float HitKnockbackHorizontal => hitKnockbackHorizontal;
        public float HitKnockbackUp => hitKnockbackUp;

        /// <summary>Último daño recibido (lo escribe PlayerAgent al llegar OnTakeDamage).</summary>
        public DamageInfo? LastDamage { get; set; }
        #endregion
        
        // ───────────────────────────────────────────────────────────────────────

        #region Combat Targeting

        // ───────────────────────────────────────────────────────────────────────

        [Header("Combat - Targeting")] [SerializeField, Tooltip("Layers considerados enemigos para chequeos de hit.")]
        private LayerMask enemyMask = ~0;

        /// <summary>Máscara de capas para enemigos.</summary>
        public LayerMask EnemyMask
        {
            get => enemyMask;
            set => enemyMask = value;
        }

        #endregion

        // ───────────────────────────────────────────────────────────────────────

        #region Basic combo (Attack1/2/3)

        // ───────────────────────────────────────────────────────────────────────

        [Header("Attack (Combo Básico)")] [SerializeField, Tooltip("Daño base por golpe.")]
        private int attackDamage = 10;

        [SerializeField, Tooltip("Duración del ataque 1 (s).")]
        private float attack1Duration = 0.25f;

        [SerializeField, Tooltip("Duración del ataque 2 (s).")]
        private float attack2Duration = 0.28f;

        [SerializeField, Tooltip("Duración del ataque 3 (s).")]
        private float attack3Duration = 0.34f;

        [SerializeField, Tooltip("Alcance efectivo frontal (m).")]
        private float attackRange = 2.0f;

        [SerializeField, Range(5f, 90f), Tooltip("Semiancho del cono frontal (°).")]
        private float attackHalfAngleDegrees = 55f;

        [SerializeField, Tooltip("Ventana para encadenar siguiente golpe (s).")]
        private float attackChainWindow = 0.30f;

        [SerializeField, Tooltip("Tolerancia posterior al fin del ataque para aceptar el chain (s).")]
        private float attackLateChainGrace = 0.08f;

        // [SerializeField, Tooltip("Empuje al enemigo impactado (m).")]
        // private float attackKnockbackDistance = 2.5f;
        //
        // [SerializeField, Tooltip("Tiempo de stagger al enemigo (s).")]
        // private float attackStaggerTime = 0.35f;

        [SerializeField, Tooltip("Cooldown al terminar el 3er golpe (s).")]
        private float attackComboCooldown = 0.4f;

        public int AttackDamage
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

        public float AttackLateChainGrace
        {
            get => attackLateChainGrace;
            set => attackLateChainGrace = value;
        }

        // public float AttackKnockbackDistance
        // {
        //     get => attackKnockbackDistance;
        //     set => attackKnockbackDistance = value;
        // }

        // public float AttackStaggerTime
        // {
        //     get => attackStaggerTime;
        //     set => attackStaggerTime = value;
        // }

        public float AttackComboCooldown
        {
            get => attackComboCooldown;
            set => attackComboCooldown = value;
        }

        #endregion

        // ───────────────────────────────────────────────────────────────────────

        #region Vertical attack (air slam)

        // ───────────────────────────────────────────────────────────────────────

        [Header("Attack Vertical (Aéreo)")] [SerializeField, Tooltip("Radio del área de daño en el impacto (m).")]
        private float verticalAttackRadius = 2.8f;

        [SerializeField, Tooltip("Daño del ataque vertical.")]
        private int verticalDamage = 12;

        [SerializeField, Tooltip("Empuje radial (m).")]
        private float verticalKnockbackDistance = 2.5f;

        [SerializeField, Tooltip("Stagger en enemigos (s).")]
        private float verticalStaggerTime = 0.35f;

        [SerializeField, Tooltip("Cooldown del ataque vertical (s).")]
        private float verticalAttackCooldown = 1.0f;

        [SerializeField, Tooltip("Tiempo inmóvil del jugador tras el impacto (s).")]
        private float verticalAttackPostStun = 0.25f;

        [SerializeField, Tooltip("Aceleración extra hacia abajo durante la caída (m/s²).")]
        private float verticalSlamExtraAccel = 40f;

        [SerializeField, Tooltip("Velocidad máxima hacia abajo (m/s).")]
        private float verticalSlamMaxDownSpeed = 30f;

        [SerializeField, Tooltip("Impulso vertical inicial negativo (m/s), 0 = sin impulso.")]
        private float verticalSlamStartDownSpeed = 0f;
        
        [Header("Attack Vertical - Targeting/Physics Extra")]
        [SerializeField, Tooltip("Máscara de capas que recibe el impacto del vertical (enemigos, rompibles, props…).")]
        private LayerMask verticalHitMask = ~0;

        [SerializeField, Tooltip("¿Aplicar impulso a rigidbodies al impactar?")]
        private bool verticalAffectsRigidbodies = true;

        [SerializeField, Tooltip("Impulso aplicado a rigidbodies (m/s) en el impacto.")]
        private float verticalRigidbodyImpulse = 6f;

        [SerializeField, Tooltip("Componente vertical añadida al impulso de rigidbodies (0..1).")]
        private float verticalRigidbodyUpFactor = 0.35f;
        
        
        public LayerMask VerticalHitMask
        {
            get => verticalHitMask;
            set => verticalHitMask = value;
        }

        public bool VerticalAffectsRigidbodies
        {
            get => verticalAffectsRigidbodies;
            set => verticalAffectsRigidbodies = value;
        }

        public float VerticalRigidbodyImpulse
        {
            get => verticalRigidbodyImpulse;
            set => verticalRigidbodyImpulse = value;
        }

        public float VerticalRigidbodyUpFactor
        {
            get => verticalRigidbodyUpFactor;
            set => verticalRigidbodyUpFactor = value;
        }

        
        public float VerticalAttackRadius
        {
            get => verticalAttackRadius;
            set => verticalAttackRadius = value;
        }

        public int VerticalDamage
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

        // ───────────────────────────────────────────────────────────────────────

        #region Spin attack (360° charge/release)

        // ───────────────────────────────────────────────────────────────────────

        [Header("Attack 360° (Carga/Release)")] [SerializeField, Tooltip("Tiempo mínimo de carga (s).")]
        private float spinChargeMinTime = 0.6f;

        [SerializeField, Tooltip("Tiempo de carga para potencia/duración máximas (s).")]
        private float spinChargeMaxTime = 1.6f;

        [SerializeField, Tooltip("Mult. de movimiento mientras se ejecuta el SpinRelease.")]
        private float spinMoveSpeedMultiplierWhileExecuting = 0.6f;

        [SerializeField, Tooltip("Mult. de salto mientras dura el SpinRelease.")]
        private float spinJumpSpeedMultiplier = 1.15f;

        [Header("Spin: duraciones")] [SerializeField, Tooltip("Duración mínima del giro (s).")]
        private float spinMinDuration = 0.55f;

        [SerializeField, Tooltip("Duración máxima del giro (s).")]
        private float spinMaxDuration = 1.10f;

        [SerializeField, Tooltip("Duración base (legacy).")]
        private float spinDuration = 0.7f;

        [SerializeField, Tooltip("Tiempo inmóvil luego del release (s).")]
        private float spinPostStun = 0.35f;

        [SerializeField, Tooltip("Radio del área de daño (m).")]
        private float spinRadius = 2.8f;

        [SerializeField, Tooltip("Daño del 360°.")]
        private int spinDamage = 10;

        [SerializeField, Tooltip("Empuje radial (m).")]
        private float spinPushDistance = 2.0f;

        [SerializeField, Tooltip("Stagger aplicado (s).")]
        private float spinStaggerTime = 0.3f;

        [SerializeField, Tooltip("Cooldown (s).")]
        private float spinCooldown = 3.0f;

        [SerializeField, Tooltip("Multiplicador de movimiento mientras se carga.")]
        private float spinMoveSpeedMultiplierWhileCharging = 0.6f;

        [Header("Self Stun por Spin")] [SerializeField, Tooltip("Luego del spin, el jugador queda knockdown.")]
        private bool spinCausesSelfStun = true;

        [SerializeField, Tooltip("Derribo mínimo (s).")]
        private float selfStunMinDuration = 0.60f;

        [SerializeField, Tooltip("Derribo máximo (s).")]
        private float selfStunMaxDuration = 2.00f;

        [SerializeField, Tooltip("Tiempo previo para 'GetUp' (s).")]
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

        public int SpinDamage
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

        // ───────────────────────────────────────────────────────────────────────

        #region Action modifiers (locks / multipliers)

        // ───────────────────────────────────────────────────────────────────────

        [Header("Action Modifiers")]
        [SerializeField, Tooltip("Multiplica la velocidad de locomoción por acciones (carga, etc.).")]
        private float actionMoveSpeedMultiplier = 1f;

        [SerializeField, Tooltip("Multiplica velocidad/impulso de salto por acciones.")]
        private float actionJumpSpeedMultiplier = 1f;

        [SerializeField, Tooltip("Bloquea la locomoción (vertical en ejecución, etc.).")]
        private bool locomotionBlocked = false;

        [SerializeField, Tooltip("Invulnerable a enemigos durante ciertas mecánicas.")]
        private bool invulnerableToEnemies = false;

        [SerializeField, Tooltip("Bloqueo de orientación (p. ej. fijar la mira).")]
        private bool aimLockActive = false;

        [SerializeField, Tooltip("Dirección de aim-lock (mundo).")]
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

        // ───────────────────────────────────────────────────────────────────────

        #region State tuning (sacamos números mágicos del Agent)

        // ───────────────────────────────────────────────────────────────────────

        [Header("State Tuning")] [SerializeField, Tooltip("Coyote time al dejar el suelo (s).")]
        private float coyoteTime = 0.12f;

        [SerializeField, Tooltip("Retardo de detección de aire al iniciar JumpGround (s).")]
        private float jumpGroundAirDetectDelay = 0.04f;

        [SerializeField, Tooltip("Retardo de detección de aire al iniciar JumpAir (s).")]
        private float jumpAirAirDetectDelay = 0.02f;

        [SerializeField, Tooltip("Tiempo de asentamiento al caer antes de marcar grounded (s).")]
        private float fallSettleTime = 0.04f;
        
        [SerializeField, Tooltip("Layer del Player")]
        private LayerMask playerLayer;

        /// <summary>Coyote time al dejar el suelo (s).</summary>
        public float CoyoteTime
        {
            get => coyoteTime;
            set => coyoteTime = value;
        }

        /// <summary>Retardo de detección de aire en <c>JumpGround</c> (s).</summary>
        public float JumpGroundAirDetectDelay
        {
            get => jumpGroundAirDetectDelay;
            set => jumpGroundAirDetectDelay = value;
        }

        /// <summary>Retardo de detección de aire en <c>JumpAir</c> (s).</summary>
        public float JumpAirAirDetectDelay
        {
            get => jumpAirAirDetectDelay;
            set => jumpAirAirDetectDelay = value;
        }

        /// <summary>Tiempo de asentamiento del estado <c>Fall</c> (s).</summary>
        public float FallSettleTime
        {
            get => fallSettleTime;
            set => fallSettleTime = value;
        }
        public LayerMask PlayerLayer
        {
            get => playerLayer;
            set => playerLayer = value;
        }

        #endregion

        // ───────────────────────────────────────────────────────────────────────

        #region Runtime (No serializado)

        // ───────────────────────────────────────────────────────────────────────

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
        
        [System.NonSerialized] public Vector3 RespawnPosition;
        [System.NonSerialized] public Quaternion RespawnRotation;

        [System.NonSerialized] private bool _isDead;

        public bool IsDead
        {
            get => _isDead;
            set => _isDead = value;
        }


        /// <summary>Input crudo de movimiento (Vector2).</summary>
        public Vector2 RawMoveInput
        {
            get => _rawMoveInput;
            set => _rawMoveInput = value;
        }

        /// <summary>Dirección de movimiento en espacio mundo (Vector3).</summary>
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

        /// <summary>Si el salto fue sin input horizontal (para habilitar vertical attack).</summary>
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

        /// <summary>Ratio [0..1] de carga del spin (se setea en SpinCharge).</summary>
        public float SpinChargeRatio
        {
            get => _spinChargeRatio;
            set => _spinChargeRatio = value;
        }

        /// <summary>Si el jugador está en estado de SelfStun (knockdown).</summary>
        public bool IsSelfStunned
        {
            get => _isSelfStunned;
            set => _isSelfStunned = value;
        }

        /// <summary>Tiempo restante del SelfStun actual (s).</summary>
        public float SelfStunTimeLeft
        {
            get => _selfStunTimeLeft;
            set => _selfStunTimeLeft = value;
        }

        /// <summary>Duración objetivo del SelfStun actual (s).</summary>
        public float SelfStunDuration
        {
            get => _selfStunDuration;
            set => _selfStunDuration = value;
        }
        
        /// <summary>Ventana post-dash armada para iniciar sprint.</summary>
        public bool SprintArmed
        {
            get => _sprintArmed;
            set => _sprintArmed = value;
        }

        /// <summary>Estado de botón de dash mantenido (para hold de sprint).</summary>
        public bool DashHeld
        {
            get => _dashHeld;
            set => _dashHeld = value;
        }

        /// <summary>Tiempo restante de la ventana de sprint (s).</summary>
        public float SprintArmTimeLeft
        {
            get => _sprintArmTimeLeft;
            set => _sprintArmTimeLeft = value;
        }

        /// <summary>Acumulador de hold (s) mientras se mantiene el botón.</summary>
        public float SprintHoldCounter
        {
            get => _sprintHoldCounter;
            set => _sprintHoldCounter = value;
        }

        #endregion

        // ───────────────────────────────────────────────────────────────────────

        #region Utilities

        // ───────────────────────────────────────────────────────────────────────

        /// <summary>Resetea la cantidad de saltos disponibles al máximo configurado.</summary>
        [ContextMenu("Reset Jumps")]
        public void ResetJumps() => _jumpsLeft = maxJumps;

        /// <summary>Limpia locks y devuelve los multiplicadores de acción a 1x.</summary>
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

        /// <summary>Cierra la ventana de Sprint y limpia contadores.</summary>
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