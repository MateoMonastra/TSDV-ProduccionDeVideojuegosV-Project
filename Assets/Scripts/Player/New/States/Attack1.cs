using FSM;
using UnityEngine;

namespace Player.New
{
    /// <summary>Primer golpe del combo. Permite encadenar a A2 durante una ventana.</summary>
    public class Attack1 : AttackBase
    {
        public const string ToAttack2 = "ToAttack2";
        public const string ToIdle    = "ToIdle";

        private bool _waitingChain;
        private float _chainTimer;
        private readonly PlayerAnimationController _anim;

        public Attack1(MyKinematicMotor m, PlayerModel mdl, System.Action<string> req, PlayerAnimationController anim = null)
            : base(m, mdl, req) { _anim = anim; }

        public override void Enter()
        {
            base.Enter();

            if (!M.IsGrounded) { Req?.Invoke(ToIdle); Finish(); return; }

            t = 0f;
            Duration = Model.Attack1Duration;

            _anim?.SetCombatActive(true);
            _anim?.TriggerAttack1();
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            t += dt;

            // Hacer daño (una vez) hacia el frente
            TryDoHitFrontal(0.5f, Model.AttackHalfAngleDegrees);

            // Ventana para chain
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
                    // se cerró la ventana sin input
                    Req?.Invoke(ToIdle);
                    Finish();
                }
            }

            // auto-salida si llegó al final y no hubo chain input
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
                Req?.Invoke(ToAttack2);
                Finish();
            }
        }
    }
}
