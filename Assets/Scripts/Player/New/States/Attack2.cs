using FSM;
using UnityEngine;

namespace Player.New
{
    public class Attack2 : AttackBase
    {
        public const string ToA3 = "Attack2->Attack3";
        public const string ToIdle = "Attack2->AttackIdle";

        private bool _waitingChain;
        private float _chainTimer;

        public Attack2(MyKinematicMotor m, PlayerModel mdl, System.Action<string> req) : base(m, mdl, req) { }

        public override void Enter()
        {
            base.Enter();
            t = 0f;
            Duration = Model.Attack2Duration;
            _waitingChain = false;
            _chainTimer = 0f;
            // TODO: anim/sfx A2
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            t += dt;

            TryDoHitFrontal(0.5f, 55f);

            if (!_waitingChain && t >= Duration)
            {
                _waitingChain = true;
                _chainTimer = Model.AttackChainWindow;
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

        public override void HandleInput(params object[] values)
        {
            if (!_waitingChain) return;
            if (values is { Length: >=1 } && values[0] is string cmd && cmd == "AttackPressed")
            {
                Req?.Invoke(ToA3);
                Finish();
            }
        }
    }
}