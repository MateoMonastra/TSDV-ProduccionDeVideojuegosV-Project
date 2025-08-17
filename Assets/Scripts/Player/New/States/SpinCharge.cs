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

        private readonly PlayerAnimationController _anim;
        public SpinCharge(PlayerModel model, Transform cam, System.Action<string> request, PlayerAnimationController anim = null)
        { _model = model; _cam = cam; _req = request; _anim = anim; }


        public override void Enter()
        {
            base.Enter();
            _model.locomotionBlocked = false; // se puede mover lento mientras carga
            _model.actionMoveSpeedMultiplier = _model.spinMoveSpeedMultiplierWhileCharging;

            _anim?.SetCombatActive(true);
            _anim?.SetSpinCharging(true);
        }

        public override void Exit()
        {
            base.Exit();
            _anim?.SetSpinCharging(false);
            _anim?.SetCombatActive(false);
            _model.actionMoveSpeedMultiplier = 1f;
        }


        public override void Tick(float dt)
        {
            base.Tick(dt);
            _t += dt;

            if (_releaseQueued && _t >= _model.spinChargeMinTime)
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