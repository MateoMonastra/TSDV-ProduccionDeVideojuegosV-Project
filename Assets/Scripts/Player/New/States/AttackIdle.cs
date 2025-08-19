using FSM;
using UnityEngine;

namespace Player.New
{
    public class AttackIdle : State
    {
        public const string ToA1 = "AttackIdle->Attack1";

        private readonly System.Action<string> _req;
        private readonly PlayerModel _model;
        private readonly PlayerAnimationController _anim;
        private readonly MyKinematicMotor _motor;

        public AttackIdle(PlayerModel model, System.Action<string> request, PlayerAnimationController anim = null, MyKinematicMotor motor = null)
        {
            _model = model; _req = request; _anim = anim; _motor = motor;
        }

        public override void Enter()
        {
            base.Enter();
            _model.ClearActionLocks();
            _anim?.SetCombatActive(false);
        }
        
        public override void Tick(float delta)
        {
            base.Tick(delta);
            
            if (_model.attackComboOnCooldown)
            {
                _model.attackComboCooldownLeft = Mathf.Max(0f, _model.attackComboCooldownLeft - delta);
                if (_model.attackComboCooldownLeft <= 0f) _model.attackComboOnCooldown = false;
            }
        }

        public override void HandleInput(params object[] values)
        {
            if (_model.attackComboOnCooldown) return;
            if (_motor != null && !_motor.IsGrounded) return;

            if (values is { Length: >= 1 } && values[0] is string cmd && cmd == "AttackPressed")
                _req?.Invoke(ToA1);
        }
    }
}