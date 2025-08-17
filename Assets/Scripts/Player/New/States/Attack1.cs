using FSM;
using UnityEngine;

namespace Player.New
{
    public class Attack1 : AttackBase
    {
        public const string ToA2 = "Attack1->Attack2";
        public const string ToIdle = "Attack1->AttackIdle";

        private bool _waitingChain;
        private float _chainTimer;
        private readonly PlayerAnimationController _anim;
        public Attack1(MyKinematicMotor m, PlayerModel mdl, System.Action<string> req, PlayerAnimationController anim = null)
            : base(m, mdl, req) { _anim = anim; }

        public override void Enter()
        {
            base.Enter();
            t = 0f; Duration = Model.attack1Duration;
            _anim?.TriggerAttack1();
            if (_anim != null) _anim.OnAnim_AttackHit += OnAnimHit; // animation event
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            t += dt;

            // Hacer daño una vez (en el medio por defecto)
            TryDoHitFrontal(0.5f, 55f);

            if (!_waitingChain && t >= Duration)
            {
                _waitingChain = true;
                _chainTimer = Model.attackChainWindow; // comienza ventana para encadenar
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
            if (!_waitingChain) return; // solo acepta input cuando terminó la anim y hay ventana

            if (values is { Length: >=1 } && values[0] is string cmd && cmd == "AttackPressed")
            {
                Req?.Invoke(ToA2);
                Finish();
            }
        }
    }
}