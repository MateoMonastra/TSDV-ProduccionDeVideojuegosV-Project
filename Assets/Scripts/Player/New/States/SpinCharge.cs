using FSM;
using Player.New.UI;
using UnityEngine;

namespace Player.New
{
    /// <summary>
    /// Carga del ataque 360°.
    /// - Solo puede INICIAR en suelo y si no hay cooldown.
    /// - Mientras carga, el jugador se puede mover (con multiplicador).
    /// - Al SOLTAR (AttackHeavyReleased):
    ///     * Si no llegó al mínimo, se cancela → Idle.
    ///     * Si superó el mínimo, guarda <see cref="PlayerModel.SpinChargeRatio"/> [0..1] y pasa a Release.
    /// - Emite eventos de UI con el progreso de carga.
    /// </summary>
    public class SpinCharge : FinishableState
    {
        // ──────────────────────────────────────────────────────────────────────
        // Transition IDs (limpias)
        public const string ToRelease = "ToRelease";
        public const string ToIdle    = "ToIdle";
        // ──────────────────────────────────────────────────────────────────────

        private readonly PlayerModel _model;
        private readonly System.Action<string> _requestTransition;
        private readonly Transform _cam;
        private readonly MyKinematicMotor _motor;
        private readonly PlayerAnimationController _anim;
        private readonly HUDManager _hud;
        
        private float _t;           
        private bool  _released;    
        private bool  _canStart;

        public SpinCharge(PlayerModel model,
                          System.Action<string> requestTransition,
                          Transform cam,
                          HUDManager hud,
                          MyKinematicMotor motor,
                          PlayerAnimationController anim = null)
        {
            _model = model;
            _requestTransition = requestTransition;
            _cam = cam;
            _hud = hud;
            _motor = motor;
            _anim = anim;
        }

        /// <summary>
        /// Entra al estado:
        /// - Valida inicio (suelo + no cooldown).
        /// - Aplica multiplicador de movimiento de carga.
        /// - Activa flags/anim y arranca UI.
        /// </summary>
        public override void Enter()
        {
            base.Enter();

            _canStart = _motor.IsGrounded && !_model.SpinOnCooldown;
            if (!_canStart)
            {
                _requestTransition?.Invoke(ToIdle);
                Finish();
                return;
            }

            _t = 0f;
            _released = false;

            
            _model.ActionMoveSpeedMultiplier = _model.SpinMoveSpeedMultiplierWhileCharging;
            _model.AimLockActive = false;

      
            _anim?.SetCombatActive(true);
            _anim?.SetSpinCharging(true);
            _hud.OnSpinChargeProgress(0f, _model.SpinChargeMinTime, _model.SpinChargeMaxTime);
        }

        /// <summary>Limpia multiplicadores/flags y cierra la UI de carga.</summary>
        public override void Exit()
        {
            base.Exit();

            _anim?.SetSpinCharging(false);
            _anim?.SetCombatActive(false);

            _model.ActionMoveSpeedMultiplier = 1f;
            _hud.OnSpinChargeEnd();
        }

        /// <summary>
        /// Acumula tiempo, actualiza UI y, si se soltó el botón:
        /// - menor al mínimo: cancela,
        /// - mayor/igual al mínimo: calcula ratio y pasa a Release.
        /// </summary>
        public override void Tick(float dt)
        {
            base.Tick(dt);
            if (!_canStart) return;

            _t += dt;
            
            _hud.OnSpinChargeProgress(_t, _model.SpinChargeMinTime, _model.SpinChargeMaxTime);

            if (_released)
            {
                if (_t < _model.SpinChargeMinTime)
                {
                    _requestTransition?.Invoke(ToIdle);
                    Finish();
                    return;
                }
                
                float minT = _model.SpinChargeMinTime;
                float maxT = Mathf.Max(minT + 0.01f, _model.SpinChargeMaxTime);
                float clamped = Mathf.Clamp(_t, minT, maxT);
                _model.SpinChargeRatio = Mathf.InverseLerp(minT, maxT, clamped);

                _requestTransition?.Invoke(ToRelease);
                Finish();
            }
        }

        /// <summary>Recibe el “soltado” del heavy desde el Agent/Reader.</summary>
        public override void HandleInput(params object[] values)
        {
            if (values is { Length: >= 1 } &&
                values[0] is string cmd &&
                cmd == CommandKeys.AttackHeavyReleased)
            {
                _released = true;
            }
        }
    }
}
