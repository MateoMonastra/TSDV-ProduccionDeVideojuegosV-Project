using FSM;
using UnityEngine;

// tu InputReader

namespace Player.New
{
    [RequireComponent(typeof(MyKinematicMotor))]
    public class PlayerAgent : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private InputReader _input;
        [SerializeField] private Camera _cameraRef;
        [SerializeField] private MyKinematicMotor _motor;
        [SerializeField] private PlayerModel _model;

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
        private Attack1 _a1; private Attack2 _a2; private Attack3 _a3;
        private AttackVertical _aVertical;
        private SpinCharge _aSpinCharge;
        private SpinRelease _aSpinRelease;

        private bool IsActionBlocked() => _model != null && _model.LocomotionBlocked;

        private void Awake()
        {
            if (_cameraRef == null) _cameraRef = Camera.main;
            if (_motor == null) _motor = GetComponent<MyKinematicMotor>();
            if (_model == null) _model = new PlayerModel();

            // ----- Locomoción -----
            void LR(string id) => _locomotionFsm.TryTransitionTo(id);
            _sIdle       = new WalkIdle(_motor, _model, _cameraRef.transform, LR, onWalk: null, coyoteTime: 0.12f);
            _sJumpGround = new JumpGround(_motor, _model, _cameraRef.transform, LR, airDetectDelay: 0.04f);
            _sJumpAir    = new JumpAir(_motor, _model, _cameraRef.transform, LR, airDetectDelay: 0.02f);
            _sFall       = new Fall(_motor, _model, _cameraRef.transform, LR, settleTime: 0.04f);
            _sDash       = new Dash(_motor, _model, LR);

            _sIdle.AddTransition(new Transition{ From=_sIdle,       To=_sJumpGround, ID=WalkIdle.ToJump });
            _sIdle.AddTransition(new Transition{ From=_sIdle,       To=_sFall,       ID=WalkIdle.ToFall });
            _sJumpGround.AddTransition(new Transition{ From=_sJumpGround, To=_sFall,    ID=JumpGround.ToFall });
            _sJumpGround.AddTransition(new Transition{ From=_sJumpGround, To=_sJumpAir, ID=JumpGround.ToJumpAir });
            _sJumpAir.AddTransition(new Transition{ From=_sJumpAir,  To=_sFall,       ID=JumpAir.ToFall });
            _sFall.AddTransition(new Transition{ From=_sFall,        To=_sIdle,       ID=Fall.ToWalkIdle });

            // Dash desde cualquier estado de locomoción
            _sIdle.AddTransition(new Transition{ From=_sIdle,       To=_sDash, ID=Dash.ToDash });
            _sJumpGround.AddTransition(new Transition{ From=_sJumpGround, To=_sDash, ID=Dash.ToDash });
            _sJumpAir.AddTransition(new Transition{ From=_sJumpAir, To=_sDash, ID=Dash.ToDash });
            _sFall.AddTransition(new Transition{ From=_sFall,       To=_sDash, ID=Dash.ToDash });
            _sDash.AddTransition(new Transition{ From=_sDash, To=_sFall, ID=Dash.ToFall });
            _sDash.AddTransition(new Transition{ From=_sDash, To=_sIdle, ID=Dash.ToWalkIdle });

            _locomotionFsm = new Fsm(_sIdle);

            // ----- Acciones -----
            void AR(string id) => _actionFsm.TryTransitionTo(id);
            _aIdle = new AttackIdle(_model, AR);
            _a1          = new Attack1(_motor, _model, AR);
            _a2          = new Attack2(_motor, _model, AR);
            _a3          = new Attack3(_motor, _model, AR);
            _aVertical   = new AttackVertical(_motor, _model, AR);
            _aSpinCharge = new SpinCharge(_model, _cameraRef.transform, AR);
            _aSpinRelease= new SpinRelease(_motor, _model, AR);

            _aIdle.AddTransition(new Transition{ From=_aIdle, To=_a1, ID=AttackIdle.ToA1 });
            _a1.AddTransition(new Transition{ From=_a1, To=_a2, ID=Attack1.ToA2 });
            _a1.AddTransition(new Transition{ From=_a1, To=_aIdle, ID=Attack1.ToIdle });
            _a2.AddTransition(new Transition{ From=_a2, To=_a3, ID=Attack2.ToA3 });
            _a2.AddTransition(new Transition{ From=_a2, To=_aIdle, ID=Attack2.ToIdle });
            _a3.AddTransition(new Transition{ From=_a3, To=_aIdle, ID=Attack3.ToIdle });

            _aSpinCharge.AddTransition(new Transition{ From=_aSpinCharge, To=_aSpinRelease, ID=SpinCharge.ToRelease });
            _aSpinCharge.AddTransition(new Transition{ From=_aSpinCharge, To=_aIdle,        ID=SpinCharge.ToIdle });
            _aSpinRelease.AddTransition(new Transition{ From=_aSpinRelease, To=_aIdle,      ID=SpinRelease.ToIdle });

            _actionFsm = new Fsm(_aIdle);

            // (Opcional) UI cooldowns
            _sDash.OnDashCooldownUI = t => { /* UI dash */ };
            _aSpinRelease.OnSpinCooldownUI = t => { /* UI spin */ };
        }

        private void OnEnable()
        {
            if (_input != null)
            {
                _input.OnMove += OnMove;
                _input.OnJump += OnJump;
                _input.OnClick += OnAttackBasic;
                _input.OnDash += OnDash;
                _input.OnAttackHeavyPressed += OnAttackHeavyPressed;
                _input.OnAttackHeavyReleased += OnAttackHeavyReleased;
            }
        }
        private void OnDisable()
        {
            if (_input != null)
            {
                _input.OnMove -= OnMove;
                _input.OnJump -= OnJump;
                _input.OnClick -= OnAttackBasic;
                _input.OnDash -= OnDash;
                _input.OnAttackHeavyPressed -= OnAttackHeavyPressed;
                _input.OnAttackHeavyReleased -= OnAttackHeavyReleased;
            }
        }

        private void OnMove(Vector2 move) => _model.RawMoveInput = Vector2.ClampMagnitude(move, 1f);

        private void OnJump()
        {
            if (IsActionBlocked()) return;
            _locomotionFsm.GetCurrentState()?.HandleInput("Jump", true);
        }

        private void OnDash()
        {
            if (IsActionBlocked()) return;
            if (!Dash.CanUse(_model)) return;
            _locomotionFsm.TryTransitionTo(Dash.ToDash);
        }

        private void OnAttackBasic()
        {
            if (IsActionBlocked()) return;

            if (AttackVertical.CanUse(_motor, _model))
            {
                _actionFsm.ForceTransition(_aVertical);
                return;
            }
            _actionFsm.GetCurrentState()?.HandleInput("AttackPressed");
        }

        private void OnAttackHeavyPressed()
        {
            if (IsActionBlocked()) return;
            if (!_motor.IsGrounded) return;       // no iniciar carga en el aire
            if (_model.SpinOnCooldown) return;

            _actionFsm.GetCurrentState()?.HandleInput("AttackHeavyPressed");
        }

        private void OnAttackHeavyReleased()
        {
            if (IsActionBlocked()) return;        // durante release/post-stun ignoramos
            _actionFsm.GetCurrentState()?.HandleInput("AttackHeavyReleased");
        }

        private void Update()
        {
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
