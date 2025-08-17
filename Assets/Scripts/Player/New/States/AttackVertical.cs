using FSM;
using UnityEngine;

namespace Player.New
{
    public class AttackVertical : FinishableState
    {
        public const string ToIdle = "AttackVertical->AttackIdle";

        private readonly MyKinematicMotor _m;
        private readonly PlayerModel _model;
        private readonly System.Action<string> _req;

        private float _t;
        private bool _impactDone;

        public AttackVertical(MyKinematicMotor m, PlayerModel mdl, System.Action<string> req)
        { _m = m; _model = mdl; _req = req; }

        public override void Enter()
        {
            base.Enter();

            if (_m.IsGrounded || !_model.JumpWasPureVertical || _model.VerticalOnCooldown)
            { _req?.Invoke(ToIdle); Finish(); return; }

            _t = 0f; _impactDone = false;
            _model.LocomotionBlocked = true;
            // TODO anim/sfx vertical
        }

        public override void Exit()
        {
            base.Exit();
            _model.LocomotionBlocked = false;
            _model.VerticalOnCooldown = true;
            _model.VerticalCooldownLeft = _model.VerticalAttackCooldown;
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            _t += dt;

            if (_m.IsGrounded && !_impactDone)
            {
                _impactDone = true;
                // TODO daño en área (_model.VerticalAttackRadius)
                _model.LocomotionBlocked = true;
                _t = 0f; // post-stun
            }

            if (_impactDone && _t >= _model.VerticalAttackPostStun)
            {
                _model.LocomotionBlocked = false;
                _req?.Invoke(ToIdle);
                Finish();
            }

            if (_model.VerticalOnCooldown)
            {
                _model.VerticalCooldownLeft = Mathf.Max(0f, _model.VerticalCooldownLeft - dt);
                if (_model.VerticalCooldownLeft <= 0f) _model.VerticalOnCooldown = false;
            }
        }

        public static bool CanUse(MyKinematicMotor m, PlayerModel mdl)
            => !m.IsGrounded && mdl.JumpWasPureVertical && !mdl.VerticalOnCooldown;
    }
}
