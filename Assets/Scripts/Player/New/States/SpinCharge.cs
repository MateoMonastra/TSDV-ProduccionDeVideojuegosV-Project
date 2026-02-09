using FSM;
using Player.New.Audio;
using Player.New.VFX;
using UI;
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
        public const string ToRelease = "ToRelease";
        public const string ToIdle    = "ToIdle";

        private readonly PlayerModel _model;
        private readonly System.Action<string> _requestTransition;
        private readonly MyKinematicMotor _motor;
        private readonly PlayerAnimationController _anim;
        private readonly HUDManager _hud;
        private readonly PlayerAudioController _audioController;
        private readonly PlayerVfxController _vfxController;
        
        private float _t;           
        private bool  _released;    
        private bool  _canStart;
        private bool charge1, charge2, charge3;

        public SpinCharge(PlayerModel model,
                          System.Action<string> requestTransition,
                          Transform cam,
                          HUDManager hud,
                          MyKinematicMotor motor,
                          PlayerVfxController vfxController,
                          PlayerAnimationController anim = null,
                          PlayerAudioController audioController = null
                          )
        {
            _model = model;
            _requestTransition = requestTransition;
            _hud = hud;
            _motor = motor;
            _vfxController = vfxController;
            _anim = anim;
            _audioController = audioController;
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
            _model.JumpBlocked = true;
            _model.DashBlocked = true;

            _vfxController.Stop(VfxEvent.BaseAttack);
      
            _anim?.ResetSpinInterruption();
            _anim?.SetSpinCharging(true);
            _anim?.SetCombatActive(false);
            _anim?.TriggerSpinChargeStart();
            _hud.OnSpinChargeProgress(0f, _model.SpinChargeMinTime, _model.SpinChargeMaxTime);

            _audioController.PlayPlayerChargeStart();
            
            charge1 = false;
            charge2 = false;
            charge3 = false;
        }

        /// <summary>Limpia multiplicadores/flags y cierra la UI de carga.</summary>
        public override void Exit()
        {
            base.Exit();
            
            _model.ActionMoveSpeedMultiplier = 1f;
            _hud.OnSpinChargeEnd();
            
            _vfxController.Stop(VfxEvent.SpinCharge1);
            _vfxController.Stop(VfxEvent.SpinCharge2);
            _vfxController.Stop(VfxEvent.SpinCharge3);
            
            _audioController.PlayPlayerChargeStopFail();
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

            float aminT = _model.SpinChargeMinTime;
            float amaxT = Mathf.Max(aminT, _model.SpinChargeMaxTime);
            float aclamped = Mathf.Clamp(_t, aminT, amaxT);
            float valor = Mathf.InverseLerp(aminT, amaxT, aclamped);

            if (!charge1)
            {
                charge1 = true;
                _vfxController.Play(VfxEvent.SpinCharge1);
                _audioController.PlayPlayerSpinCharge1();
            }
            else if (_t >= _model.SpinChargeMinTime && !charge2)
            {
                charge2 = true;
                _vfxController.Play(VfxEvent.SpinCharge2);
                _audioController.PlayPlayerSpinCharge2();
            }
            else if (valor > 0.9f && !charge3)
            {
                charge3 = true;
                _vfxController.Play(VfxEvent.SpinCharge3);
                _audioController.PlayPlayerSpinCharge3();
            }
            
            if (_released)
            {
                if (_t < _model.SpinChargeMinTime)
                {
                    _model.JumpBlocked = false;
                    _model.DashBlocked = false;
                    _requestTransition?.Invoke(ToIdle);
                    _audioController.PlayPlayerChargeStopFail();

                    _anim?.TriggerSpinInterruption();
                    _anim?.SetSpinCharging(false);
                    
                    Finish();
                    return;
                }
                
                float minT = _model.SpinChargeMinTime;
                float maxT = Mathf.Max(minT, _model.SpinChargeMaxTime);
                float clamped = Mathf.Clamp(_t, minT, maxT);
                _model.SpinChargeRatio = Mathf.InverseLerp(minT, maxT, clamped);

                _anim?.SetSpinCharging(false);
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
