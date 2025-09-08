using FSM;
using UnityEngine;

namespace Player.New
{
    /// <summary>
    /// Desplazamiento rápido en dirección de movimiento (o mirada si no hay input).
    /// Tiene cooldown y un ease-out configurable para evitar “frenada seca”.
    /// Al terminar, arma la ventana de Sprint.
    /// </summary>
    public class Dash : FinishableState
    {
        public const string ToFall     = "ToFall";
        public const string ToWalkIdle = "ToWalkIdle";

        private readonly MyKinematicMotor _m;
        private readonly PlayerModel _model;
        private readonly System.Action<string> _req;
        private readonly PlayerAnimationController _anim;

        private Vector3 _dir;
        private float   _duration;
        private float   _t;
        
        private bool  _recovering;
        private float _recoverT;

        public System.Action<float> OnDashCooldownUI;

        public Dash(MyKinematicMotor m, PlayerModel model, System.Action<string> req, PlayerAnimationController anim = null)
        { _m = m; _model = model; _req = req; _anim = anim; }

        /// <summary>Usable cuando no está en cooldown.</summary>
        public static bool CanUse(PlayerModel mdl) => !mdl.DashOnCooldown;

        public override void Enter()
        {
            base.Enter();
            
            Vector3 up = _m.CharacterUp;
            Vector3 charFwd = Vector3.ProjectOnPlane(_m.transform.forward, up);
            _dir = charFwd.sqrMagnitude > 1e-6f ? charFwd.normalized : _m.transform.forward;
            if (_model.MoveInputWorld.sqrMagnitude > 1e-6f)
                _dir = _model.MoveInputWorld.normalized;
            
            float effectiveDistance = Mathf.Max(0.01f, _model.DashDistance);
            float effectiveSpeed    = Mathf.Max(0.01f, _model.DashSpeed);

            if (_model.DashBuffPending)
            {
                effectiveDistance = Mathf.Max(0.01f, _model.DashBuffDistance);
                effectiveSpeed    = Mathf.Max(0.01f, _model.DashBuffSpeed);

                _model.DashBuffPending = false;
            }
            
            _duration = effectiveDistance / effectiveSpeed;
            _t = 0f;
            
            _anim?.TriggerDash();
            _model.InvulnerableToEnemies = true;
            
            _model.DashOnCooldown   = true;
            _model.DashCooldownLeft = _model.DashCooldown;
            OnDashCooldownUI?.Invoke(_model.DashCooldownLeft);
            
            var v = _m.Velocity;
            v.x = _dir.x * effectiveSpeed;
            v.z = _dir.z * effectiveSpeed;
            _m.SetVelocity(v);
        }


        public override void Exit()
        {
            base.Exit();
            _model.BeginSprintWindow();
            _model.InvulnerableToEnemies = false;
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);

            if (!_recovering)
            {
                _t += dt;
                
                var v = _m.Velocity;
                v.x = _dir.x * _model.DashSpeed;
                v.z = _dir.z * _model.DashSpeed;
                _m.SetVelocity(v);

                if (_t >= _duration)
                {
                    _recovering = true;
                    _recoverT = 0f;
                    
                    _model.BeginSprintWindow();
                }
            }
            else
            {
                _recoverT += dt;
                float k = Mathf.Clamp01(_recoverT / Mathf.Max(0.01f, _model.DashExitBlendTime));
                
                Vector3 desired = _model.MoveInputWorld * _model.MoveSpeed;
                Vector3 v = _m.Velocity;
                Vector3 h = new Vector3(v.x, 0f, v.z);
                h = Vector3.Lerp(h, desired, 1f - Mathf.Exp(-_model.DashExitSharpness * k * dt));
                v.x = h.x; v.z = h.z;
                _m.SetVelocity(v);

                if (_recoverT >= _model.DashExitBlendTime)
                {
                    _recovering = false;
                    _req?.Invoke(_m.IsGrounded ? ToWalkIdle : ToFall);
                    Finish();
                }
            }
        }
    }
}
