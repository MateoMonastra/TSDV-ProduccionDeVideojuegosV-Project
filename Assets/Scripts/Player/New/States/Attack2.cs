using FSM;

namespace Player.New
{
    public class Attack2 : AttackBase
    {
        public const string ToA3 = "Attack2->Attack3";
        public const string ToIdle = "Attack2->AttackIdle";

        private bool _waitingChain;
        private float _chainTimer;

        private readonly PlayerAnimationController _anim;
        public Attack2(MyKinematicMotor m, PlayerModel mdl, System.Action<string> req, PlayerAnimationController anim = null)
            : base(m, mdl, req) { _anim = anim; }

        public override void Enter()
        {
            base.Enter();
            t = 0f;
            Duration = Model.attack2Duration;     // <--- FIX
            _waitingChain = false;                // <--- FIX
            _chainTimer = 0f;                     // <--- FIX
            _anim?.TriggerAttack2();
            if (_anim != null) _anim.OnAnim_AttackHit += OnAnimHit;
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            t += dt;

            TryDoHitFrontal(0.5f, 55f);

            if (!_waitingChain && t >= Duration)
            {
                _waitingChain = true;
                _chainTimer = Model.attackChainWindow;
            }

            if (_waitingChain)
            {
                _chainTimer -= dt;
                if (_chainTimer <= 0f)
                {
                    Req?.Invoke(ToIdle);
                    Finish();
                }
            }
        }

        public override void Exit()
        {
            base.Exit();
            if (_anim != null) _anim.OnAnim_AttackHit -= OnAnimHit;
        }
        private void OnAnimHit() => TryDoHitFrontal(0f);

        public override void HandleInput(params object[] values)
        {
            if (!_waitingChain) return;
            if (values is { Length: >= 1 } && values[0] is string cmd && cmd == "AttackPressed")
            {
                Req?.Invoke(ToA3);
                Finish();
            }
        }
    }
}