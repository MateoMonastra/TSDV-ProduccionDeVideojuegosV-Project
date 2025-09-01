using FSM;
using UnityEngine;

namespace Player.New
{
    /// <summary>
    /// Orquesta entrada de usuario, máquinas de estados (locomoción/acciones),
    /// y enfila cooldowns/retroalimentación UI para el jugador.
    /// </summary>
    [RequireComponent(typeof(MyKinematicMotor))]
    [DisallowMultipleComponent]
    public class PlayerAgent : MonoBehaviour
    {
        // ─────────────────────────────────────────────────────────────────────────

        #region Inspector Refs

        // ─────────────────────────────────────────────────────────────────────────

        [Header("Refs")] [SerializeField] private InputReader input;
        [SerializeField] private Camera cameraRef;
        [SerializeField] private MyKinematicMotor motor;
        [SerializeField] private PlayerModel model;
        [SerializeField] private PlayerAnimationController anim;
        [SerializeField] private UI.CombatUIController combatUI;

        #endregion

        // ─────────────────────────────────────────────────────────────────────────

        #region FSM: Locomotion

        // ─────────────────────────────────────────────────────────────────────────

        private Fsm _locomotionFsm;
        private WalkIdle _sIdle;
        private JumpGround _sJumpGround;
        private JumpAir _sJumpAir;
        private Fall _sFall;
        private Dash _sDash;
        private Sprint _sSprint;

        #endregion

        // ─────────────────────────────────────────────────────────────────────────

        #region FSM: Actions

        // ─────────────────────────────────────────────────────────────────────────

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

        // ─────────────────────────────────────────────────────────────────────────

        #region Unity Messages

        // ─────────────────────────────────────────────────────────────────────────

        private void Awake()
        {
            InitRefs();
            BuildLocomotionFsm();
            BuildActionFsm();
            WireCombatUI();
        }

        private void OnEnable() => SubscribeInputs(true);
        private void OnDisable() => SubscribeInputs(false);

        private void Update()
        {
            float dt = Time.deltaTime;

            UpdateCooldowns(dt);

            _locomotionFsm.Update();
            _actionFsm.Update();
        }

        private void FixedUpdate()
        {
            _locomotionFsm.FixedUpdate();
            _actionFsm.FixedUpdate();
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────

        #region Input Handlers

        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>Movimiento 2D crudo (clamp a 1).</summary>
        private void OnMove(Vector2 move) => model.RawMoveInput = Vector2.ClampMagnitude(move, 1f);

        /// <summary>Saltos: se enruta al estado actual de locomoción.</summary>
        private void OnJump()
        {
            if (IsActionBlocked()) return;
            _locomotionFsm.GetCurrentState()?.HandleInput(CommandKeys.Jump, true);
        }

        /// <summary>Dash: chequea bloqueo y cooldown global.</summary>
        private void OnDash()
        {
            if (IsActionBlocked()) return;
            if (!Dash.CanUse(model)) return;
            _locomotionFsm.ForceTransition(_sDash);
        }

        /// <summary>Click ataque básico o vertical aéreo.</summary>
        private void OnAttackBasic()
        {
            if (IsActionBlocked()) return;

            if (!motor.IsGrounded)
            {
                if (AttackVertical.CanUse(motor, model))
                    _actionFsm.ForceTransition(_aVertical);
                return;
            }

            _actionFsm.GetCurrentState()?.HandleInput(CommandKeys.AttackPressed);
        }

        /// <summary>Heavy (spin) presionado: entra a carga si hay suelo y no hay cooldown.</summary>
        private void OnHeavyPressed()
        {
            if (motor.IsGrounded && !model.SpinOnCooldown)
                _actionFsm.ForceTransition(_aSpinCharge);
        }

        /// <summary>Heavy soltado: se notifica al estado actual de acciones.</summary>
        private void OnHeavyReleased()
        {
            _actionFsm.GetCurrentState()?.HandleInput(CommandKeys.AttackHeavyReleased);
        }

        /// <summary>Dash mantenido: se cachea en el modelo (usado por WalkIdle/Sprint).</summary>
        private void OnDashHeldChanged(bool held) => model.DashHeld = held;

        #endregion

        // ─────────────────────────────────────────────────────────────────────────

        #region Cooldowns & UI

        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>Aplica el tick de todos los cooldowns y actualiza UI asociada.</summary>
        private void UpdateCooldowns(float dt)
        {
            UpdateDashCooldown(dt);
            UpdateSpinCooldown(dt);
            UpdateVerticalCooldown(dt);
            UpdateComboCooldown(dt);
        }

        private void UpdateDashCooldown(float dt)
        {
            if (!model.DashOnCooldown) return;

            model.DashCooldownLeft = Mathf.Max(0f, model.DashCooldownLeft - dt);
            if (model.DashCooldownLeft <= 0f) model.DashOnCooldown = false;

            _sDash?.OnDashCooldownUI?.Invoke(model.DashCooldownLeft);
        }

        private void UpdateSpinCooldown(float dt)
        {
            if (!model.SpinOnCooldown) return;

            model.SpinCooldownLeft = Mathf.Max(0f, model.SpinCooldownLeft - dt);
            if (model.SpinCooldownLeft <= 0f) model.SpinOnCooldown = false;

            _aSpinRelease?.OnSpinCooldownUI?.Invoke(model.SpinCooldownLeft);
        }

        private void UpdateVerticalCooldown(float dt)
        {
            if (!model.VerticalOnCooldown) return;

            model.VerticalCooldownLeft = Mathf.Max(0f, model.VerticalCooldownLeft - dt);
            if (model.VerticalCooldownLeft <= 0f) model.VerticalOnCooldown = false;
        }

        private void UpdateComboCooldown(float dt)
        {
            if (!model.AttackComboOnCooldown) return;

            model.AttackComboCooldownLeft = Mathf.Max(0f, model.AttackComboCooldownLeft - dt);
            if (model.AttackComboCooldownLeft <= 0f) model.AttackComboOnCooldown = false;
        }

        #endregion

        // ─────────────────────────────────────────────────────────────────────────

        #region Helpers (setup / guards / wiring)

        // ─────────────────────────────────────────────────────────────────────────

        /// <summary>Inicializa referencias de cámara, motor y modelo.</summary>
        private void InitRefs()
        {
            if (cameraRef == null) cameraRef = Camera.main;
            if (motor == null) motor = GetComponent<MyKinematicMotor>();
            if (model == null) model = ScriptableObject.CreateInstance<PlayerModel>();
        }
        
        /// <summary>Construye la FSM de locomoción y define sus transiciones.</summary>
        private void BuildLocomotionFsm()
        {
            // Local function con nombre claro para pedir una transición de locomoción
            void RequestLocomotionTransition(string transitionId) => _locomotionFsm.TryTransitionTo(transitionId);

            _sIdle = new WalkIdle(motor, model, cameraRef.transform, RequestLocomotionTransition, onWalk: null,
                coyoteTime: 0.12f, anim: anim);
            _sJumpGround = new JumpGround(motor, model, cameraRef.transform, RequestLocomotionTransition,
                airDetectDelay: 0.04f, anim: anim);
            _sJumpAir = new JumpAir(motor, model, cameraRef.transform, RequestLocomotionTransition,
                airDetectDelay: 0.02f, anim: anim);
            _sFall = new Fall(motor, model, cameraRef.transform, RequestLocomotionTransition, settleTime: 0.04f,
                anim: anim);
            _sDash = new Dash(motor, model, RequestLocomotionTransition, anim: anim);
            _sSprint = new Sprint(motor, model, cameraRef.transform, RequestLocomotionTransition, anim);

            _sSprint.AddTransition(new Transition { From = _sSprint, To = _sIdle, ID = Sprint.ToWalkIdle });
            _sSprint.AddTransition(new Transition { From = _sSprint, To = _sJumpGround, ID = Sprint.ToJump });
            _sSprint.AddTransition(new Transition { From = _sSprint, To = _sFall, ID = Sprint.ToFall });

            _sIdle.AddTransition(new Transition { From = _sIdle, To = _sSprint, ID = WalkIdle.ToSprint });
            _sIdle.AddTransition(new Transition { From = _sIdle, To = _sJumpGround, ID = WalkIdle.ToJump });
            _sIdle.AddTransition(new Transition { From = _sIdle, To = _sFall, ID = WalkIdle.ToFall });

            _sJumpGround.AddTransition(new Transition { From = _sJumpGround, To = _sFall, ID = JumpGround.ToFall });
            _sJumpGround.AddTransition(
                new Transition { From = _sJumpGround, To = _sJumpAir, ID = JumpGround.ToJumpAir });
            _sJumpAir.AddTransition(new Transition { From = _sJumpAir, To = _sFall, ID = JumpAir.ToFall });
            _sFall.AddTransition(new Transition { From = _sFall, To = _sIdle, ID = Fall.ToWalkIdle });
            _sFall.AddTransition(new Transition { From = _sFall, To = _sJumpAir, ID = Fall.ToJumpAir });
            _sDash.AddTransition(new Transition { From = _sDash, To = _sIdle, ID = Dash.ToWalkIdle });
            _sDash.AddTransition(new Transition { From = _sDash, To = _sFall, ID = Dash.ToFall });

            _locomotionFsm = new Fsm(_sIdle);
        }

        /// <summary>Construye la FSM de acciones (combo, vertical, spin, self-stun).</summary>
        private void BuildActionFsm()
        {
            // Local function con nombre claro para pedir una transición de acciones
            void RequestActionTransition(string transitionId) => _actionFsm.TryTransitionTo(transitionId);

            _aIdle = new AttackIdle(model, RequestActionTransition, anim, motor);
            _a1 = new Attack1(motor, model, RequestActionTransition, anim);
            _a2 = new Attack2(motor, model, RequestActionTransition, anim);
            _a3 = new Attack3(motor, model, RequestActionTransition, anim);
            _aVertical = new AttackVertical(motor, model, RequestActionTransition, anim);
            _aSpinCharge = new SpinCharge(model, RequestActionTransition, cameraRef.transform, motor, anim);
            _aSpinRelease = new SpinRelease(motor, model, RequestActionTransition, anim);
            _aSelfStun = new SelfStun(motor, model, RequestActionTransition, anim);

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
        
        /// <summary>Conecta eventos de los estados a la UI (cooldowns, carga, etc.).</summary>
        private void WireCombatUI()
        {
            if (!combatUI) return;

            if (_aSpinCharge != null)
            {
                _aSpinCharge.OnSpinChargeProgress += combatUI.OnSpinChargeProgress;
                _aSpinCharge.OnSpinChargeEnd += combatUI.OnSpinChargeEnd;
            }

            if (_aSpinRelease != null)
            {
                _aSpinRelease.OnSpinCooldownUI += combatUI.OnSpinCooldown;
            }
        }

        /// <summary>Suscribe/Desuscribe inputs del <see cref="InputReader"/>.</summary>
        private void SubscribeInputs(bool subscribe)
        {
            if (input == null) return;

            if (subscribe)
            {
                input.OnMove += OnMove;
                input.OnJump += OnJump;
                input.OnClick += OnAttackBasic;
                input.OnDash += OnDash;
                input.OnAttackHeavyPressed += OnHeavyPressed;
                input.OnAttackHeavyReleased += OnHeavyReleased;
                input.OnDashHeldChanged += OnDashHeldChanged;
            }
            else
            {
                input.OnMove -= OnMove;
                input.OnJump -= OnJump;
                input.OnClick -= OnAttackBasic;
                input.OnDash -= OnDash;
                input.OnAttackHeavyPressed -= OnHeavyPressed;
                input.OnAttackHeavyReleased -= OnHeavyReleased;
                input.OnDashHeldChanged -= OnDashHeldChanged;
            }
        }

        /// <summary>Verdadero cuando alguna acción bloquea la locomoción.</summary>
        private bool IsActionBlocked() => model != null && model.LocomotionBlocked;

        #endregion
    }
}