using FSM;
using Player.New.Audio;
using Player.New.VFX;
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
        private readonly PlayerVfxController _vfxController;
        private readonly PlayerAudioController _audioController;
        private readonly Transform _cam;

        private Vector3 _dir;
        private float   _duration;
        private float   _t;

        private bool  _recovering;
        private float _recoverT;
        
        private float _dashSpeedSel;
        private float _dashDistSel;

        public System.Action<float> OnDashCooldownUI;

        public Dash(MyKinematicMotor m, PlayerModel model, Transform cam, System.Action<string> req,
            PlayerAnimationController anim = null, PlayerVfxController vfxController = null, PlayerAudioController audioController = null)
        {
            _audioController = audioController;
            _vfxController = vfxController;
            _cam = cam;
            _m = m; _model = model; _req = req; _anim = anim;
        }

        public static bool CanUse(PlayerModel mdl) => !mdl.DashOnCooldown;

        public override void Enter()
        {
            base.Enter();

            Vector3 up = _m.CharacterUp;
            
            // Calculate direction directly from input and camera (like other states)
            if (_model.RawMoveInput.sqrMagnitude > _model.MinInputSqr)
            {
                // Calculate camera-relative directions
                Vector3 camFwd = Vector3.ProjectOnPlane(_cam.forward, up).normalized;
                if (camFwd.sqrMagnitude < _model.MinInputSqr)
                    camFwd = Vector3.ProjectOnPlane(_cam.up, up).normalized;
                
                Vector3 camRight = Vector3.Cross(up, camFwd);
                
                // Calculate input direction in world space
                _dir = (camFwd * _model.RawMoveInput.y + camRight * _model.RawMoveInput.x);
                _dir = Vector3.ProjectOnPlane(_dir, up).normalized;
                
                // Ensure the direction is valid
                if (_dir.sqrMagnitude < _model.MinInputSqr)
                {
                    Vector3 charFwdPlanar = Vector3.ProjectOnPlane(_m.transform.forward, up);
                    _dir = charFwdPlanar.sqrMagnitude > _model.MinInputSqr ? charFwdPlanar.normalized : _m.transform.forward;
                }
            }
            else
            {
                // No input, use character forward
                Vector3 charFwdPlanar = Vector3.ProjectOnPlane(_m.transform.forward, up);
                _dir = charFwdPlanar.sqrMagnitude > _model.MinInputSqr ? charFwdPlanar.normalized : _m.transform.forward;
            }

            _m.SetRotation(_dir);
            
            if (_model.DashBuffPending)
            {
                _dashDistSel = Mathf.Max(0.01f, _model.DashBuffDistance);
                _dashSpeedSel = Mathf.Max(0.01f, _model.DashBuffSpeed);
                _model.DashBuffPending = false;
                _audioController?.PlaySuperDashAudio();
            }
            else
            {
                _dashDistSel = Mathf.Max(0.01f, _model.DashDistance);
                _dashSpeedSel = Mathf.Max(0.01f, _model.DashSpeed);
                _audioController?.PlayDashAudio();
            }

            _duration = _dashDistSel / _dashSpeedSel;
            _t = 0f;

            _anim?.TriggerDash();
            _anim?.SetWalking(false);
            
            _model.InvulnerableToEnemies = true;
            _model.DashOnCooldown = true;
            _model.DashCooldownLeft = _model.DashCooldown;
            OnDashCooldownUI?.Invoke(_model.DashCooldownLeft);

            _m.ForceUnground(0.05f);
            
            // Set velocity based on input direction, completely ignoring current horizontal velocity
            Vector3 v = _m.Velocity;
            float y = v.y < 0f ? 0f : v.y; // Preserve positive Y velocity (upward momentum)
            
            // Set horizontal velocity directly from input direction, ignoring current velocity
            Vector3 h = _dir * _dashSpeedSel;
            v = new Vector3(h.x, y, h.z);

            _m.SetVelocity(v);

            _vfxController?.Play(VfxEvent.Dash);

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
                
                _m.SmoothRotation(_dir, _model.DashRotationSharpness, dt);
                
                // Maintain velocity based on input direction, completely ignoring current horizontal velocity
                Vector3 v = _m.Velocity;
                float y = v.y < 0f ? 0f : v.y; // Preserve positive Y velocity

                // Set horizontal velocity directly from input direction
                Vector3 h = _dir * _dashSpeedSel;
                v = new Vector3(h.x, y, h.z);

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
