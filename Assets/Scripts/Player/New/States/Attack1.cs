using FSM;

namespace Player.New
{
    /// <summary>Primer golpe del combo. Buffer + late-grace para encadenar a A2.</summary>
    public class Attack1 : AttackBase
    {
        public const string ToAttack2 = "ToAttack2";
        public const string ToIdle    = "ToIdle";

        private bool _windowOpen;
        private readonly PlayerAnimationController _anim;

        public Attack1(MyKinematicMotor m, PlayerModel mdl, System.Action<string> req, PlayerAnimationController anim = null)
            : base(m, mdl, req) { _anim = anim; }

        public override void Enter()
        {
            base.Enter();

            if (!M.IsGrounded) { Req?.Invoke(ToIdle); Finish(); return; }

            Duration = Model.Attack1Duration;   // ← usa la duración específica
            _windowOpen = false;

            _anim?.SetCombatActive(true);
            _anim?.TriggerAttack1();
            if (_anim != null) _anim.OnAnim_AttackHit += OnAnimHit;
        }

        public override void Exit()
        {
            base.Exit();
            if (_anim != null) _anim.OnAnim_AttackHit -= OnAnimHit;
            // CombatActive lo apagamos al volver a Idle para evitar flicker entre golpes
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            t += dt;

            // Hit a mitad del clip (o por evento de anim)
            TryDoHitFrontal(0.5f, Model.AttackHalfAngleDegrees);

            float chainWindow = Model.AttackChainWindow;
            float lateGrace   = Model.AttackLateChainGrace;

            // Abrir ventana ANTES de terminar el clip
            if (!_windowOpen && t >= Duration - chainWindow)
            {
                _windowOpen = true;

                // Si ya había buffer, encadenar de inmediato
                if (ChainBuffered)
                {
                    Req?.Invoke(ToAttack2);
                    Finish();
                    return;
                }
            }

            // Fin del clip → aceptar late-grace si hubo buffer
            if (t >= Duration)
            {
                if (ChainBuffered && (t - Duration) <= lateGrace)
                {
                    Req?.Invoke(ToAttack2);
                    Finish();
                    return;
                }

                // Sin chain → Idle
                Req?.Invoke(ToIdle);
                Finish();
            }
        }

        public override void HandleInput(params object[] values)
        {
            if (values is { Length: >= 1 } &&
                values[0] is string cmd &&
                cmd == CommandKeys.AttackPressed)
            {
                BufferChain();

                // Si la ventana ya está abierta o estamos dentro del late-grace, encadenar ahora
                if (_windowOpen || (t >= Duration && (t - Duration) <= Model.AttackLateChainGrace))
                {
                    Req?.Invoke(ToAttack2);
                    Finish();
                }
            }
        }

        private void OnAnimHit() => TryDoHitFrontal(0f);
    }
}
