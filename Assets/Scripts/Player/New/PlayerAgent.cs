using FSM;
using UnityEngine;
using UnityEngine.Serialization;

namespace Player.New
{
    [RequireComponent(typeof(MyKinematicMotor))]
    public class PlayerAgent : MonoBehaviour
    {
        [Header("Refs")] [SerializeField] private InputReader input;
        [SerializeField] private Camera cameraRef;
        [SerializeField] private MyKinematicMotor motor;
        [SerializeField] private PlayerModel model;
        [SerializeField] private PlayerAnimationController anim;

        // Locomoción
        private Fsm _locomotionFsm;
        private WalkIdle _sIdle;
        private JumpGround _sJumpGround;
        private JumpAir _sJumpAir;
        private Fall _sFall;
        private Dash _sDash;

        // Acciones
        private Fsm _actionFsm;
        private AttackIdle _aIdle;
        private Attack1 _a1;
        private Attack2 _a2;
        private Attack3 _a3;
        private AttackVertical _aVertical;
        private SpinCharge _aSpinCharge;
        private SpinRelease _aSpinRelease;

        private bool IsActionBlocked() => model != null && model.locomotionBlocked;

        private void Awake()
        {
            if (cameraRef == null) cameraRef = Camera.main;
            if (motor == null) motor = GetComponent<MyKinematicMotor>();
            if (model == null) model = ScriptableObject.CreateInstance<PlayerModel>();

            // ----- Locomoción -----
            void LR(string id) => _locomotionFsm.TryTransitionTo(id);
            _sIdle = new WalkIdle(motor, model, cameraRef.transform, LR, onWalk: null, coyoteTime: 0.12f, anim: anim);
            _sJumpGround = new JumpGround(motor, model, cameraRef.transform, LR, airDetectDelay: 0.04f, anim: anim);
            _sJumpAir = new JumpAir(motor, model, cameraRef.transform, LR, airDetectDelay: 0.02f, anim: anim);
            _sFall = new Fall(motor, model, cameraRef.transform, LR, settleTime: 0.04f, anim: anim);
            _sDash = new Dash(motor, model, LR, anim: anim);

            _sIdle.AddTransition(new Transition { From = _sIdle, To = _sJumpGround, ID = WalkIdle.ToJump });
            _sIdle.AddTransition(new Transition { From = _sIdle, To = _sFall, ID = WalkIdle.ToFall });
            _sJumpGround.AddTransition(new Transition { From = _sJumpGround, To = _sFall, ID = JumpGround.ToFall });
            _sJumpGround.AddTransition(
                new Transition { From = _sJumpGround, To = _sJumpAir, ID = JumpGround.ToJumpAir });
            _sJumpAir.AddTransition(new Transition { From = _sJumpAir, To = _sFall, ID = JumpAir.ToFall });
            _sFall.AddTransition(new Transition { From = _sFall, To = _sIdle, ID = Fall.ToWalkIdle });
            _sFall.AddTransition(new Transition { From = _sFall, To = _sJumpAir, ID = Fall.ToJumpAir });

            _locomotionFsm = new Fsm(_sIdle);

            // ----- Acciones -----
            void AR(string id) => _actionFsm.TryTransitionTo(id);
            _aIdle = new AttackIdle(model, AR, anim: anim, motor);
            _a1 = new Attack1(motor, model, AR, anim: anim);
            _a2 = new Attack2(motor, model, AR, anim: anim);
            _a3 = new Attack3(motor, model, AR, anim: anim);
            _aVertical = new AttackVertical(motor, model, AR, anim: anim);
            _aSpinCharge = new SpinCharge(model, cameraRef.transform, AR, anim: anim);
            _aSpinRelease = new SpinRelease(motor, model, AR, anim: anim);

            _aIdle.AddTransition(new Transition { From = _aIdle, To = _a1, ID = AttackIdle.ToA1 });
            _a1.AddTransition(new Transition { From = _a1, To = _a2, ID = Attack1.ToA2 });
            _a1.AddTransition(new Transition { From = _a1, To = _aIdle, ID = Attack1.ToIdle });
            _a2.AddTransition(new Transition { From = _a2, To = _a3, ID = Attack2.ToA3 });
            _a2.AddTransition(new Transition { From = _a2, To = _aIdle, ID = Attack2.ToIdle });
            _a3.AddTransition(new Transition { From = _a3, To = _aIdle, ID = Attack3.ToIdle });
            _aVertical.AddTransition(new Transition { From = _aVertical, To = _aIdle, ID = AttackVertical.ToIdle });

            _aSpinCharge.AddTransition(new Transition
                { From = _aSpinCharge, To = _aSpinRelease, ID = SpinCharge.ToRelease });
            _aSpinCharge.AddTransition(new Transition { From = _aSpinCharge, To = _aIdle, ID = SpinCharge.ToIdle });
            _aSpinRelease.AddTransition(new Transition { From = _aSpinRelease, To = _aIdle, ID = SpinRelease.ToIdle });

            _actionFsm = new Fsm(_aIdle);

            //UI cooldowns
            _sDash.OnDashCooldownUI = t =>
            {
                /* actualizar UI de dash */
            };
            _aSpinRelease.OnSpinCooldownUI = t =>
            {
                /* actualizar UI de spin */
            };
        }

        private void OnEnable()
        {
            if (input != null)
            {
                input.OnMove += OnMove;
                input.OnJump += OnJump;
                input.OnClick += OnAttackBasic;
                input.OnDash += OnDash;
                input.OnAttackHeavyPressed += OnAttackHeavyPressed;
                input.OnAttackHeavyReleased += OnAttackHeavyReleased;
            }
        }

        private void OnDisable()
        {
            if (input != null)
            {
                input.OnMove -= OnMove;
                input.OnJump -= OnJump;
                input.OnClick -= OnAttackBasic;
                input.OnDash -= OnDash;
                input.OnAttackHeavyPressed -= OnAttackHeavyPressed;
                input.OnAttackHeavyReleased -= OnAttackHeavyReleased;
            }
        }

        private void OnMove(Vector2 move) => model.rawMoveInput = Vector2.ClampMagnitude(move, 1f);

        private void OnJump()
        {
            if (IsActionBlocked()) return;
            _locomotionFsm.GetCurrentState()?.HandleInput("Jump", true);
        }

        private void OnDash()
        {
            if (IsActionBlocked()) return;
            if (!Dash.CanUse(model)) return;
            _locomotionFsm.ForceTransition(_sDash);
        }

        private void OnAttackBasic()
        {
            if (IsActionBlocked()) return;

            if (!motor.IsGrounded)
            {
                if (AttackVertical.CanUse(motor, model))
                {
                    _actionFsm.ForceTransition(_aVertical);
                }

                return;
            }

            _actionFsm.GetCurrentState()?.HandleInput("AttackPressed");
        }


        private void OnAttackHeavyPressed()
        {
            if (IsActionBlocked()) return;
            if (!motor.IsGrounded) return;
            if (model.spinOnCooldown) return;

            _actionFsm.GetCurrentState()?.HandleInput("AttackHeavyPressed");
        }

        private void OnAttackHeavyReleased()
        {
            if (IsActionBlocked()) return;
            _actionFsm.GetCurrentState()?.HandleInput("AttackHeavyReleased");
        }

        private void Update()
        {
            float dt = Time.deltaTime;

            //Cooldowns
            // --- D A S H ---
            if (model.dashOnCooldown)
            {
                model.dashCooldownLeft = Mathf.Max(0f, model.dashCooldownLeft - dt);
                if (model.dashCooldownLeft <= 0f) model.dashOnCooldown = false;
                _sDash?.OnDashCooldownUI?.Invoke(model.dashCooldownLeft);
            }

            // --- S P I N 360° ---
            if (model.spinOnCooldown)
            {
                model.spinCooldownLeft = Mathf.Max(0f, model.spinCooldownLeft - dt);
                if (model.spinCooldownLeft <= 0f) model.spinOnCooldown = false;
                _aSpinRelease?.OnSpinCooldownUI?.Invoke(model.spinCooldownLeft);
            }

            // --- A T T A C K  V E R T I C A L ---
            if (model.verticalOnCooldown)
            {
                model.verticalCooldownLeft = Mathf.Max(0f, model.verticalCooldownLeft - dt);
                if (model.verticalCooldownLeft <= 0f) model.verticalOnCooldown = false;
            }

            // --- A T T A C K S ---
            if (model.attackComboOnCooldown)
            {
                model.attackComboCooldownLeft = Mathf.Max(0f, model.attackComboCooldownLeft - dt);
                if (model.attackComboCooldownLeft <= 0f) model.attackComboOnCooldown = false;
            }

            _locomotionFsm.Update();
            _actionFsm.Update();
        }

        private void FixedUpdate()
        {
            _locomotionFsm.FixedUpdate();
            _actionFsm.FixedUpdate();
        }
    }
}