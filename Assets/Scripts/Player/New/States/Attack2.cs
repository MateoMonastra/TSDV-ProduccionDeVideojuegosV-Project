using FSM;

namespace Player.New
{
    /// <summary>Segundo golpe del combo. Permite encadenar a A3 durante una ventana.</summary>
    public class Attack2 : AttackBase
    {
        public const string ToAttack3 = "ToAttack3";
        public const string ToIdle    = "ToIdle";

        private bool _waitingChain;
        private float _chainTimer;
        private readonly PlayerAnimationController _anim;

        public Attack2(MyKinematicMotor m, PlayerModel mdl, System.Action<string> req, PlayerAnimationController anim = null)
            : base(m, mdl, req) { _anim = anim; }

        public override void Enter()
        {
            base.Enter();
            if (!M.IsGrounded) { Req?.Invoke(ToIdle); Finish(); return; }

            t = 0f;
            Duration = Model.Attack2Duration;

            _anim?.SetCombatActive(true);
            _anim?.TriggerAttack2();
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            t += dt;

            TryDoHitFrontal(0.5f, Model.AttackHalfAngleDegrees);

            if (!_waitingChain && t >= Duration - Model.AttackChainWindow)
            {
                _waitingChain = true;
                _chainTimer = 0f;
            }
            if (_waitingChain)
            {
                _chainTimer += dt;
                if (_chainTimer > Model.AttackChainWindow)
                {
                    Req?.Invoke(ToIdle);
                    Finish();
                }
            }

            if (t >= Duration)
            {
                Req?.Invoke(ToIdle);
                Finish();
            }
        }

        public override void HandleInput(params object[] values)
        {
            if (_waitingChain && values is { Length: >= 1 } && values[0] is string cmd && cmd == CommandKeys.AttackPressed)
            {
                Req?.Invoke(ToAttack3);
                Finish();
            }
        }
    }
}