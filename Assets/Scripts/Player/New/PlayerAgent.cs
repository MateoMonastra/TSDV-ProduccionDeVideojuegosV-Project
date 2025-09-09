using FSM;
using Health;
using KinematicCharacterController.Examples;
using Player.New.States;
using Player.New.UI;
using UnityEngine;

namespace Player.New
{
    /// <summary>
    /// Orquesta entrada de usuario, máquinas de estados (locomoción/acciones),
    /// aplica cooldowns y conecta anim/UI. No contiene tuning: toma todo de PlayerModel.
    /// </summary>
    [RequireComponent(typeof(MyKinematicMotor))]
    [DisallowMultipleComponent]
    public class PlayerAgent : MonoBehaviour
    {
        // ───────────────────────────────────────────────────────────────────────

        #region Inspector References

        // ───────────────────────────────────────────────────────────────────────

        [Header("Refs")] [SerializeField] private InputReader input;
        [SerializeField] private Camera cameraRef;
        [SerializeField] private Camera deadCameraRef;
        [SerializeField] private MyKinematicMotor motor;
        [SerializeField] private PlayerModel model;
        [SerializeField] private PlayerAnimationController anim;
        [SerializeField] private HUDManager hud;
        [SerializeField] private Health.HealthController health;
        [SerializeField] private InteractController interactController;

        #endregion

        // ───────────────────────────────────────────────────────────────────────

        #region FSMs (Locomotion / Actions)

        // ───────────────────────────────────────────────────────────────────────

        // Locomoción
        private Fsm _locomotionFsm;
        private WalkIdle _sIdle;
        private JumpGround _sJumpGround;
        private JumpAir _sJumpAir;
        private Interact _sInteract;
        private Fall _sFall;
        private Dash _sDash;
        private Sprint _sSprint;
        private Death _sDeath;
        private PlayerHit _sHit;

        // Acciones
        private Fsm _actionFsm;
        private AttackIdle _aIdle;
        private Attack1 _a1;
        private Attack2 _a2;
        private Attack3 _a3;
        private AttackVertical _aVertical;
        private SpinCharge _aSpinCharge;
        private SpinRelease _aSpinRelease;
        private SelfStun _aSelfStun;

        #endregion

        // ───────────────────────────────────────────────────────────────────────

        #region Unity Messages

        // ───────────────────────────────────────────────────────────────────────

        private void Awake()
        {
            InitRefs();
            BuildLocomotionFsm();
            BuildActionFsm();
        }

        private void OnEnable()
        {
            SubscribeInputs(true);
            if (health != null) health.OnDeath += OnPlayerDeath;
            if (health) health.OnTakeDamage += OnPlayerDamaged;

            if (interactController) interactController.OnStartInteractAction += OnInteractStarted;
            if (interactController) interactController.OnEndInteractAction += OnInteractEnded;
        }

        private void OnDisable()
        {
            SubscribeInputs(false);
            if (health != null) health.OnDeath -= OnPlayerDeath;
            if (health) health.OnTakeDamage -= OnPlayerDamaged;
            
            if (interactController) interactController.OnStartInteractAction -= OnInteractStarted;
            if (interactController) interactController.OnEndInteractAction -= OnInteractEnded;
        }

        private void Start()
        {
            model.RespawnPosition = transform.position;
            model.RespawnRotation = transform.rotation;
        }

        private void Update()
        {
            float dt = Time.deltaTime;
            UpdateCooldowns(dt);
            interactController.DetectInteractions();
            
            _locomotionFsm.Update();
            _actionFsm.Update();
        }

        private void FixedUpdate()
        {
            _locomotionFsm.FixedUpdate();
            _actionFsm.FixedUpdate();
        }

        #endregion

        // ───────────────────────────────────────────────────────────────────────

        #region Input Handlers

        // ───────────────────────────────────────────────────────────────────────

        /// <summary>Actualiza el input de movimiento (clamp a 1 para diagonales).</summary>
        private void OnMove(Vector2 move) => model.RawMoveInput = Vector2.ClampMagnitude(move, 1f);

        /// <summary>Solicita Dash (si no hay bloqueo ni cooldown).</summary>
        private void OnDash()
        {
            if (IsActionBlocked()) return;
            if (!Dash.CanUse(model)) return;
            _locomotionFsm.ForceTransition(_sDash);
        }

        private void OnJump()
        {
            if (IsActionBlocked()) return;

            _locomotionFsm.GetCurrentState()?.HandleInput(CommandKeys.Jump, true);
        }

        private void OnInteract()
        {
            if (IsActionBlocked()) return;
            
            interactController.Interact();
        }

        private void OnAttackBasic()
        {
            if (IsActionBlocked())
            {
                return;
            }

            if (!motor.IsGrounded)
            {
                if (AttackVertical.CanUse(motor, model))
                {
                    _actionFsm.ForceTransition(_aVertical);
                }

                return;
            }

            _actionFsm.GetCurrentState()?.HandleInput(CommandKeys.AttackPressed);
        }

        /// <summary>Heavy presionado: entra a SpinCharge (si grounded y sin cooldown).</summary>
        private void OnAttackHeavyPressed()
        {
            if (motor.IsGrounded && !model.SpinOnCooldown)
                _actionFsm.ForceTransition(_aSpinCharge);
        }

        /// <summary>Heavy soltado: lo procesa el estado actual (p. ej. SpinCharge → Release).</summary>
        private void OnAttackHeavyReleased()
        {
            _actionFsm.GetCurrentState()?.HandleInput(CommandKeys.AttackHeavyReleased);
        }

        /// <summary>Estado de “dash mantenido” para la mecánica de sprint.</summary>
        private void OnDashHeldChanged(bool held) => model.DashHeld = held;

        private void OnPlayerDeath()
        {
            anim?.SetCombatActive(false);
            _actionFsm?.ForceTransition(_aIdle);
            interactController.InterruptInteraction();
            _locomotionFsm.ForceTransition(_sDeath);
            GameEvents.GameEvents.PlayerDied(gameObject);
        }

        private void OnPlayerDamaged(DamageInfo info)
        {
            model.LastDamage = info;
            hud.OnDamaged();
            hud.SetHealth(health.GetCurrentHealth());
            interactController.InterruptInteraction();
            _locomotionFsm.ForceTransition(_sHit);
        }

        private void OnInteractStarted(InteractData data)
        {
            _sInteract.SetData(data);
            _locomotionFsm.ForceTransition(_sInteract);
        }

        private void OnInteractEnded(InteractData data)
        {
            _locomotionFsm.ForceTransition(_sIdle);
        }
        
        #endregion

        // ───────────────────────────────────────────────────────────────────────

        #region Cooldowns & UI

        // ───────────────────────────────────────────────────────────────────────

        /// <summary>Aplica tick a todos los cooldowns y actualiza la UI (si existe).</summary>
        private void UpdateCooldowns(float dt)
        {
            // Dash
            if (model.DashOnCooldown)
            {
                model.DashCooldownLeft = Mathf.Max(0f, model.DashCooldownLeft - dt);
                if (model.DashCooldownLeft <= 0f) model.DashOnCooldown = false;
                _sDash?.OnDashCooldownUI?.Invoke(model.DashCooldownLeft);
            }

            // Spin
            if (model.SpinOnCooldown)
            {
                model.SpinCooldownLeft = Mathf.Max(0f, model.SpinCooldownLeft - dt);
                if (model.SpinCooldownLeft <= 0f) model.SpinOnCooldown = false;
            }

            // Vertical attack
            if (model.VerticalOnCooldown)
            {
                model.VerticalCooldownLeft = Mathf.Max(0f, model.VerticalCooldownLeft - dt);
                if (model.VerticalCooldownLeft <= 0f) model.VerticalOnCooldown = false;
            }

            // Combo
            if (model.AttackComboOnCooldown)
            {
                model.AttackComboCooldownLeft = Mathf.Max(0f, model.AttackComboCooldownLeft - dt);
                if (model.AttackComboCooldownLeft <= 0f) model.AttackComboOnCooldown = false;
            }
        }

        #endregion

        // ───────────────────────────────────────────────────────────────────────

        #region Setup / Wiring

        // ───────────────────────────────────────────────────────────────────────

        /// <summary>Inicializa referencias si no fueron seteadas por Inspector.</summary>
        private void InitRefs()
        {
            if (cameraRef == null) cameraRef = Camera.main;
            if (motor == null) motor = GetComponent<MyKinematicMotor>();
            if (model == null) model = ScriptableObject.CreateInstance<PlayerModel>();

            if (!model) return;
            model.HasExtraJump = false;
            model.DashBuffPending = false;
        }

        /// <summary>
        /// Construye la FSM de locomoción (Idle, JumpGround/Air, Fall, Dash, Sprint).
        /// Los parámetros (coyote, delays, settle time) se toman del <see cref="PlayerModel"/>.
        /// </summary>
        private void BuildLocomotionFsm()
        {
            // Función local descriptiva para solicitar transiciones de locomoción
            void RequestLocomotionTransition(string transitionId) => _locomotionFsm.TryTransitionTo(transitionId);

            _sIdle = new WalkIdle(motor, model, cameraRef.transform, RequestLocomotionTransition, anim: anim);

            _sJumpGround = new JumpGround(motor, model, cameraRef.transform, RequestLocomotionTransition, anim: anim);

            _sJumpAir = new JumpAir(motor, model, cameraRef.transform, RequestLocomotionTransition, anim: anim);

            _sInteract = new Interact(motor, model, RequestLocomotionTransition, anim: anim);
            
            _sFall = new Fall(motor, model, cameraRef.transform, RequestLocomotionTransition, anim: anim);

            _sDash = new Dash(motor, model, RequestLocomotionTransition, anim: anim);
            _sSprint = new Sprint(motor, model, cameraRef.transform, RequestLocomotionTransition, anim: anim);

            _sDeath = new Death(
                motor,
                model,
                deadCameraRef,
                cameraRef,
                RequestLocomotionTransition,
                anim,
                () => RespawnAt(model.RespawnPosition, model.RespawnRotation, resetHealth: true)
            );

            _sHit = new PlayerHit(motor, model, RequestLocomotionTransition, anim: anim);

            // Transiciones de locomoción
            _sSprint.AddTransition(new Transition { From = _sSprint, To = _sIdle, ID = Sprint.ToWalkIdle });
            _sSprint.AddTransition(new Transition { From = _sSprint, To = _sJumpGround, ID = Sprint.ToJump });
            _sSprint.AddTransition(new Transition { From = _sSprint, To = _sFall, ID = Sprint.ToFall });

            _sIdle.AddTransition(new Transition { From = _sIdle, To = _sSprint, ID = WalkIdle.ToSprint });
            _sIdle.AddTransition(new Transition { From = _sIdle, To = _sJumpGround, ID = WalkIdle.ToJump });
            _sIdle.AddTransition(new Transition { From = _sIdle, To = _sFall, ID = WalkIdle.ToFall });
            _sIdle.AddTransition(new Transition{From = _sIdle, To = _sInteract, ID = WalkIdle.ToInteract});

            _sJumpGround.AddTransition(new Transition { From = _sJumpGround, To = _sFall, ID = JumpGround.ToFall });
            _sJumpGround.AddTransition(
                new Transition { From = _sJumpGround, To = _sJumpAir, ID = JumpGround.ToJumpAir });
            _sJumpAir.AddTransition(new Transition { From = _sJumpAir, To = _sFall, ID = JumpAir.ToFall });

            _sFall.AddTransition(new Transition { From = _sFall, To = _sIdle, ID = Fall.ToWalkIdle });
            _sFall.AddTransition(new Transition { From = _sFall, To = _sJumpAir, ID = Fall.ToJumpAir });

            _sDash.AddTransition(new Transition { From = _sDash, To = _sIdle, ID = Dash.ToWalkIdle });
            _sDash.AddTransition(new Transition { From = _sDash, To = _sFall, ID = Dash.ToFall });

            _sDeath.AddTransition(new Transition { From = _sDeath, To = _sIdle, ID = Death.ToWalkIdle });

            _sHit.AddTransition(new Transition { From = _sHit, To = _sIdle, ID = PlayerHit.ToWalkIdle });

            _locomotionFsm = new Fsm(_sIdle);
        }

        /// <summary>
        /// Construye la FSM de acciones (Idle, combo básico, vertical, spin, self-stun).
        /// </summary>
        private void BuildActionFsm()
        {
            // Función local descriptiva para solicitar transiciones de acciones
            void RequestActionTransition(string transitionId) => _actionFsm.TryTransitionTo(transitionId);

            _aIdle = new AttackIdle(model, RequestActionTransition, anim, motor);
            _a1 = new Attack1(motor, model, RequestActionTransition, anim);
            _a2 = new Attack2(motor, model, RequestActionTransition, anim);
            _a3 = new Attack3(motor, model, RequestActionTransition, anim);
            _aVertical = new AttackVertical(motor, model, RequestActionTransition, anim);
            _aSpinCharge = new SpinCharge(model, RequestActionTransition, cameraRef.transform, hud, motor, anim);
            _aSpinRelease = new SpinRelease(motor, model, hud, RequestActionTransition, anim);
            _aSelfStun = new SelfStun(motor, model, RequestActionTransition, anim);

            // Transiciones de acciones
            _aIdle.AddTransition(new Transition { From = _aIdle, To = _a1, ID = AttackIdle.ToAttack1 });
            _a1.AddTransition(new Transition { From = _a1, To = _a2, ID = Attack1.ToAttack2 });
            _a1.AddTransition(new Transition { From = _a1, To = _aIdle, ID = Attack1.ToIdle });
            _a2.AddTransition(new Transition { From = _a2, To = _a3, ID = Attack2.ToAttack3 });
            _a2.AddTransition(new Transition { From = _a2, To = _aIdle, ID = Attack2.ToIdle });
            _a3.AddTransition(new Transition { From = _a3, To = _aIdle, ID = Attack3.ToIdle });
            _aVertical.AddTransition(new Transition { From = _aVertical, To = _aIdle, ID = AttackVertical.ToIdle });

            _aSpinCharge.AddTransition(new Transition
                { From = _aSpinCharge, To = _aSpinRelease, ID = SpinCharge.ToRelease });
            _aSpinCharge.AddTransition(new Transition { From = _aSpinCharge, To = _aIdle, ID = SpinCharge.ToIdle });
            _aSpinRelease.AddTransition(new Transition
                { From = _aSpinRelease, To = _aSelfStun, ID = SpinRelease.ToSelfStun });
            _aSpinRelease.AddTransition(new Transition { From = _aSpinRelease, To = _aIdle, ID = SpinRelease.ToIdle });
            _aSelfStun.AddTransition(new Transition { From = _aSelfStun, To = _aIdle, ID = SelfStun.ToIdle });

            _actionFsm = new Fsm(_aIdle);
        }

        /// <summary>Suscribe o desuscribe callbacks del <see cref="InputReader"/>.</summary>
        private void SubscribeInputs(bool subscribe)
        {
            if (input == null) return;

            if (subscribe)
            {
                input.OnMove += OnMove;
                input.OnJump += OnJump;
                input.OnInteract += OnInteract;
                input.OnClick += OnAttackBasic;
                input.OnDash += OnDash;
                input.OnAttackHeavyPressed += OnAttackHeavyPressed;
                input.OnAttackHeavyReleased += OnAttackHeavyReleased;
                input.OnDashHeldChanged += OnDashHeldChanged;
            }
            else
            {
                input.OnMove -= OnMove;
                input.OnJump -= OnJump;
                input.OnInteract -= OnInteract;
                input.OnClick -= OnAttackBasic;
                input.OnDash -= OnDash;
                input.OnAttackHeavyPressed -= OnAttackHeavyPressed;
                input.OnAttackHeavyReleased -= OnAttackHeavyReleased;
                input.OnDashHeldChanged -= OnDashHeldChanged;
            }
        }

        /// <summary>Verdadero cuando una acción bloquea la locomoción (vertical, knockdown, etc.).</summary>
        private bool IsActionBlocked() => model != null && model.LocomotionBlocked;

        #endregion

        // ───────────────────────────────────────────────────────────────────────

        #region Utilities

        public PlayerModel GetPlayerModel() => model;

        public void RespawnAt(Vector3 pos, Quaternion rot, bool resetHealth = true)
        {
            anim?.SetCombatActive(false);

            _actionFsm?.ForceTransition(_aIdle);
            _locomotionFsm?.ForceTransition(_sIdle);

            model.ClearActionLocks();
            model.ResetJumps();
            model.LocomotionBlocked = false;
            model.IsDead = false;

            motor.WarpTo(pos, rot);
            motor.SetVelocity(Vector3.zero);

            if (!resetHealth || health == null) return;
            health.ResetHealth();
            hud.SetHealth(health.GetCurrentHealth());
        }

        #endregion
    }
}