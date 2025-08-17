using FSM;
using UnityEngine;

namespace Player.New
{
    public class SpinCharge : State
    {
        public const string ToRelease = "SpinCharge->SpinRelease";
        public const string ToIdle = "SpinCharge->AttackIdle";

        private readonly PlayerModel _model;
        private readonly System.Action<string> _req;
        private readonly Transform _cam;

        private float _t;
        private bool _releaseQueued;

        public SpinCharge(PlayerModel model, Transform cam, System.Action<string> request)
        { _model = model; _cam = cam; _req = request; }

        public override void Enter()
        {
            base.Enter();
            if (_model.SpinOnCooldown) { _req?.Invoke(ToIdle); return; }

            _t = 0f; _releaseQueued = false;
            _model.ActionMoveSpeedMultiplier = _model.SpinMoveSpeedMultiplierWhileCharging;
            _model.AimLockActive = true;
            Vector3 fwd = Vector3.ProjectOnPlane(_cam.forward, Vector3.up).normalized;
            if (fwd.sqrMagnitude < 1e-4f) fwd = _cam.forward;
            _model.AimLockDirection = fwd;
        }

        public override void Exit()
        {
            base.Exit();
            // limpio en Release
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            _t += dt;

            if (_releaseQueued && _t >= _model.SpinChargeMinTime)
                _req?.Invoke(ToRelease);
        }

        public override void HandleInput(params object[] values)
        {
            if (values is { Length: >=1 } && values[0] is string cmd &&
                (cmd == "AttackHeavyReleased" || cmd == "AttackHeavyPressed"))
                _releaseQueued = true;
        }
    }
}