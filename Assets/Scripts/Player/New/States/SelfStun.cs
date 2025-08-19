using FSM;
using UnityEngine;

namespace Player.New
{
    public class SelfStun : FinishableState
    {
        public const string ToIdle = "SelfStun->AttackIdle";

        private readonly MyKinematicMotor _m;
        private readonly PlayerModel _model;
        private readonly System.Action<string> _req;
        private readonly PlayerAnimationController _anim;

        private float _t;
        private bool _playedGetUp;

        public SelfStun(MyKinematicMotor motor, PlayerModel model, System.Action<string> request, PlayerAnimationController anim = null)
        { _m = motor; _model = model; _req = request; _anim = anim; }

        public override void Enter()
        {
            base.Enter();
            _t = 0f; _playedGetUp = false;

            _model.locomotionBlocked = true;
            _model.actionMoveSpeedMultiplier = 0f;
            _model.invulnerableToEnemies = false;
            _model.aimLockActive = false;

            _model.isSelfStunned = true;
            _model.selfStunTimeLeft = _model.selfStunDuration;

            var v = _m.Velocity; v.x = 0f; v.z = 0f; _m.SetVelocity(v);

            _anim?.SetCombatActive(true);
            _anim?.TriggerKnockdown();
        }

        public override void Exit()
        {
            base.Exit();
            _model.isSelfStunned = false;
            _model.ClearActionLocks();
            _anim?.SetCombatActive(false);
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            _t += dt;

            var v = _m.Velocity; v.x = 0f; v.z = 0f; _m.SetVelocity(v);
            _model.selfStunTimeLeft = Mathf.Max(0f, _model.selfStunDuration - _t);

            if (!_playedGetUp && _model.selfStunDuration - _t <= _model.selfStunGetUpLeadTime)
            {
                _playedGetUp = true;
                _anim?.TriggerGetUp();
            }

            if (_t >= _model.selfStunDuration)
            {
                _req?.Invoke(ToIdle);
                Finish();
            }
        }
    }
}
