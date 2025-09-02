using FSM;
using UnityEngine;

namespace Player.New
{
    /// <summary>
    /// Estado de “knockdown / auto-stun” posterior al SpinRelease (o similares).
    /// Bloquea locomoción, anula movimiento horizontal, reproduce anims de caída
    /// y, al agotarse el tiempo configurado, reproduce “get up” y vuelve a Idle.
    /// </summary>
    public class SelfStun : FinishableState
    {
        // ──────────────────────────────────────────────────────────────────────
        // Transition IDs (limpias)
        public const string ToIdle = "ToIdle";
        // ──────────────────────────────────────────────────────────────────────

        private readonly MyKinematicMotor _motor;
        private readonly PlayerModel _model;
        private readonly System.Action<string> _requestTransition;
        private readonly PlayerAnimationController _anim;

        private float _t;                 
        private bool  _playedGetUp;      

        public SelfStun(MyKinematicMotor motor,
                        PlayerModel model,
                        System.Action<string> requestTransition,
                        PlayerAnimationController anim = null)
        {
            _motor = motor;
            _model = model;
            _requestTransition = requestTransition;
            _anim = anim;
        }

        /// <summary>
        /// Entrar al SelfStun:
        /// - Bloquea locomoción y pone multiplicadores a 0.
        /// - Marca estado de self-stun y setea el tiempo restante.
        /// - Anula la velocidad horizontal.
        /// - Dispara anim de caída/knockdown.
        /// </summary>
        public override void Enter()
        {
            base.Enter();

            _t = 0f;
            _playedGetUp = false;

            // Bloqueos y multiplicadores
            _model.LocomotionBlocked         = true;
            _model.ActionMoveSpeedMultiplier = 0f;
            _model.InvulnerableToEnemies     = false;
            _model.AimLockActive             = false;

            // Flag + temporizador de stun
            _model.IsSelfStunned   = true;
            _model.SelfStunTimeLeft = _model.SelfStunDuration;

            // Detener desplazamiento horizontal
            ZeroHorizontalVelocity();

            // Animación
            _anim?.SetCombatActive(true);
            _anim?.TriggerKnockdown();
        }

        /// <summary>Salir: limpia flag y locks (vía <see cref="PlayerModel.ClearActionLocks"/>).</summary>
        public override void Exit()
        {
            base.Exit();
            _model.IsSelfStunned = false;
            _model.ClearActionLocks();
            _anim?.SetCombatActive(false);
        }

        /// <summary>
        /// Mantiene al personaje inmóvil horizontalmente, cuenta el tiempo de stun y
        /// dispara “get up” cerca del final. Al finalizar, vuelve a Idle.
        /// </summary>
        public override void Tick(float dt)
        {
            base.Tick(dt);

            _t += dt;

            // Forzar inmovilidad horizontal
            ZeroHorizontalVelocity();

            // Actualizar contador visible/restante
            _model.SelfStunTimeLeft = Mathf.Max(0f, _model.SelfStunDuration - _t);

            // Disparar pre-anim de “levantarse” con antelación configurable
            if (!_playedGetUp && _model.SelfStunDuration - _t <= _model.SelfStunGetUpLeadTime)
            {
                _playedGetUp = true;
                _anim?.TriggerGetUp();
            }

            // Fin del stun → volver a Idle
            if (_t >= _model.SelfStunDuration)
            {
                _requestTransition?.Invoke(ToIdle);
                Finish();
            }
        }

        // ──────────────────────────────────────────────────────────────────────
        #region Helpers
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>Anula la velocidad horizontal conservando la componente vertical.</summary>
        private void ZeroHorizontalVelocity()
        {
            Vector3 v = _motor.Velocity;
            v.x = 0f; v.z = 0f;
            _motor.SetVelocity(v);
        }

        #endregion
    }
}
