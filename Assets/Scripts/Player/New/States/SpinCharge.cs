using FSM;
using UnityEngine;

namespace Player.New
{
    /// <summary>
    /// Mantener para cargar. Solo inicia en suelo. Al soltar, guarda spinChargeRatio (0..1).
    /// Emite eventos de UI durante la carga.
    /// </summary>
    public class SpinCharge : FinishableState
    {
        public const string ToRelease = "SpinCharge->SpinRelease";
        public const string ToIdle    = "SpinCharge->AttackIdle";

        private readonly PlayerModel _model;
        private readonly System.Action<string> _req;
        private readonly Transform _cam;
        private readonly MyKinematicMotor _motor;
        private readonly PlayerAnimationController _anim;

        private float _t;         // tiempo cargando
        private bool  _released;
        private bool  _canStart;

        // === Eventos para UI ===
        // (t, min, max) => progreso de carga en segundos
        public System.Action<float, float, float> OnSpinChargeProgress;
        // fin/cancelación de la carga (ocultar UI de carga)
        public System.Action OnSpinChargeEnd;

        public SpinCharge(PlayerModel model,
                          System.Action<string> request,
                          Transform cam,
                          MyKinematicMotor motor,
                          PlayerAnimationController anim = null)
        {
            _model = model; _req = request; _cam = cam; _motor = motor; _anim = anim;
        }

        public override void Enter()
        {
            base.Enter();

            _canStart = _motor.IsGrounded && !_model.SpinOnCooldown;
            if (!_canStart) { _req?.Invoke(ToIdle); Finish(); return; }

            _t = 0f; _released = false;

            // Moverse más lento al cargar
            _model.actionMoveSpeedMultiplier = _model.SpinMoveSpeedMultiplierWhileCharging;
            _model.aimLockActive = false;

            _anim?.SetCombatActive(true);
            _anim?.SetSpinCharging(true);

            // UI: mostrar desde 0
            OnSpinChargeProgress?.Invoke(0f, _model.SpinChargeMinTime, _model.SpinChargeMaxTime);
        }

        public override void Exit()
        {
            base.Exit();
            _anim?.SetSpinCharging(false);
            _anim?.SetCombatActive(false);
            _model.actionMoveSpeedMultiplier = 1f;

            // UI: ocultar al salir
            OnSpinChargeEnd?.Invoke();
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            if (!_canStart) return;

            _t += dt;

            // UI: progreso en tiempo real
            OnSpinChargeProgress?.Invoke(_t, _model.SpinChargeMinTime, _model.SpinChargeMaxTime);

            if (_released)
            {
                // Suelta ANTES del mínimo -> cancelar
                if (_t < _model.SpinChargeMinTime)
                {
                    _req?.Invoke(ToIdle);
                    Finish();
                    return;
                }

                // Suelta DESPUÉS del mínimo -> calcular ratio y pasar a Release
                float minT = _model.SpinChargeMinTime;
                float maxT = Mathf.Max(minT + 0.01f, _model.SpinChargeMaxTime);
                float clamped = Mathf.Clamp(_t, minT, maxT);
                _model.spinChargeRatio = Mathf.InverseLerp(minT, maxT, clamped);

                _req?.Invoke(ToRelease);
                Finish();
            }
        }

        public override void HandleInput(params object[] values)
        {
            if (values is { Length: >=1 } && values[0] is string cmd && cmd == "AttackHeavyReleased")
                _released = true;
        }
    }
}
