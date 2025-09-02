using FSM;
using UnityEngine;

namespace Player.New
{
    /// <summary>
    /// Reposo del combo. Escucha clic para iniciar el primer golpe si está en suelo.
    /// </summary>
    public class AttackIdle : State
    {
        public const string ToAttack1 = "ToAttack1";

        private readonly System.Action<string> _req;
        private readonly PlayerModel _model;
        private readonly PlayerAnimationController _anim;
        private readonly MyKinematicMotor _motor;

        public AttackIdle(PlayerModel model, System.Action<string> request, PlayerAnimationController anim = null, MyKinematicMotor motor = null)
        { _model = model; _req = request; _anim = anim; _motor = motor; }

        public override void Enter()
        {
            base.Enter();
            _model.ClearActionLocks();
            _anim?.SetCombatActive(false);
        }

        public override void HandleInput(params object[] values)
        {
            if (values is { Length: >= 1 } && values[0] is string cmd && cmd == CommandKeys.AttackPressed)
            {
                if (_motor == null || _motor.IsGrounded)
                {
                    _req?.Invoke(ToAttack1);
                }
            }
        }
    }
}