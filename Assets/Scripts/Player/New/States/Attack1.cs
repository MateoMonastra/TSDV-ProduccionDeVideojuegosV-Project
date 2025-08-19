using FSM;
using UnityEngine;

namespace Player.New
{
    public class Attack1 : AttackBase
    {
        public const string ToA2   = "Attack1->Attack2";
        public const string ToIdle = "Attack1->AttackIdle";

        private bool _waitingChain;
        private float _chainTimer;

        private readonly PlayerAnimationController _anim;
        public Attack1(MyKinematicMotor m, PlayerModel mdl, System.Action<string> req, PlayerAnimationController anim = null)
            : base(m, mdl, req) { _anim = anim; }

        public override void Enter()
        {
            base.Enter();
            
            if (!M.IsGrounded)
            {
                Req?.Invoke(ToIdle);
                Finish();
                return;
            }

            t = 0f;
            float minDur = 0.05f;
            Duration = Mathf.Max(minDur, Model.attack1Duration);
            _waitingChain = false;
            _chainTimer = 0f;

            _anim?.SetCombatActive(true);
            _anim?.TriggerAttack1();
            if (_anim != null) _anim.OnAnim_AttackHit += OnAnimHit;
        }


        public override void Exit()
        {
            base.Exit();
            if (_anim != null) _anim.OnAnim_AttackHit -= OnAnimHit;
            _anim?.SetCombatActive(false); // apagar Combat al salir
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            t += dt;

            // Impacto: a mitad del clip (o por Animation Event via OnAnimHit)
            TryDoHitFrontal(0.5f, Model.attackHalfAngleDegrees);

            // Abre ventana para encadenar SOLO al terminar la animación
            if (!_waitingChain && t >= Duration)
            {
                _waitingChain = true;
                // Ventana nunca 0 para que no vuelva a Idle al instante
                _chainTimer = Mathf.Max(0.05f, Model.attackChainWindow);
            }

            // Si no encadena dentro de la ventana, vuelve a Idle
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

        private void OnAnimHit() => TryDoHitFrontal(0f);

        public override void HandleInput(params object[] values)
        {
            if (!_waitingChain) return; // solo acepta input cuando ya terminó el clip y hay ventana
            if (values is { Length: >= 1 } && values[0] is string cmd && cmd == "AttackPressed")
            {
                Req?.Invoke(ToA2);
                Finish();
            }
        }
    }
}
