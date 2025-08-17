using FSM;
using UnityEngine;

namespace Player.New
{
    public class Dash : FinishableState
    {
        public const string ToFall = "Dash->Fall";
        public const string ToWalkIdle = "Dash->WalkIdle";
        public const string ToDash = "Locomotion->Dash"; // ID para transicionar desde cualquier estado de locomoción

        private readonly MyKinematicMotor _m;
        private readonly PlayerModel _model;
        private readonly System.Action<string> _req;

        private Vector3 _dir;
        private float _duration;
        private float _t;

        public System.Action<float> OnDashCooldownUI; // opcional UI

        public Dash(MyKinematicMotor m, PlayerModel model, System.Action<string> request)
        { _m = m; _model = model; _req = request; }

        public override void Enter()
        {
            base.Enter();

            Vector3 forward = _m.transform.forward;
            Vector3 inputDir = _model.MoveInputWorld.sqrMagnitude > 1e-5f ? _model.MoveInputWorld.normalized : forward;
            _dir = new Vector3(inputDir.x, 0f, inputDir.z).normalized;
            if (_dir.sqrMagnitude < 1e-5f) _dir = forward;

            _duration = Mathf.Max(0.01f, _model.DashDistance / Mathf.Max(0.01f, _model.DashSpeed));
            _t = 0f;

            _model.InvulnerableToEnemies = true;

            var v = _m.Velocity;
            v.x = _dir.x * _model.DashSpeed;
            v.z = _dir.z * _model.DashSpeed;
            _m.SetVelocity(v);

            _model.DashOnCooldown = true;
            _model.DashCooldownLeft = _model.DashCooldown;
            OnDashCooldownUI?.Invoke(_model.DashCooldownLeft);
            // TODO anim/sfx
        }

        public override void Exit()
        {
            base.Exit();
            _model.InvulnerableToEnemies = false;
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            _t += dt;

            var v = _m.Velocity;
            v.x = _dir.x * _model.DashSpeed;
            v.z = _dir.z * _model.DashSpeed;
            _m.SetVelocity(v);

            if (_model.DashOnCooldown)
            {
                _model.DashCooldownLeft = Mathf.Max(0f, _model.DashCooldownLeft - dt);
                OnDashCooldownUI?.Invoke(_model.DashCooldownLeft);
                if (_model.DashCooldownLeft <= 0f) _model.DashOnCooldown = false;
            }

            if (_t >= _duration)
            {
                if (_m.IsGrounded)
                {
                    v = _m.Velocity; v.x = 0f; v.z = 0f; _m.SetVelocity(v);
                    _req?.Invoke(ToWalkIdle);
                }
                else _req?.Invoke(ToFall);
                Finish();
            }
        }

        public static bool CanUse(PlayerModel model) => !model.DashOnCooldown;
    }
}
