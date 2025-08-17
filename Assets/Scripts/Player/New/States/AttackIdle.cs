using FSM;
using UnityEngine;

namespace Player.New
{
    public class AttackIdle : State
    {
        public const string ToA1 = "AttackIdle->Attack1";

        private readonly System.Action<string> _req;
        private readonly PlayerModel _model;

        public AttackIdle(PlayerModel model, System.Action<string> request)
        { _model = model; _req = request; }

        public override void Tick(float delta)
        {
            base.Tick(delta);
            // Tick de cooldown del combo
            if (_model.attackComboOnCooldown)
            {
                _model.attackComboCooldownLeft = Mathf.Max(0f, _model.attackComboCooldownLeft - delta);
                if (_model.attackComboCooldownLeft <= 0f) _model.attackComboOnCooldown = false;
            }
        }

        public override void HandleInput(params object[] values)
        {
            if (_model.attackComboOnCooldown) return; // aún en CD tras 3er golpe

            if (values is { Length: >=1 } && values[0] is string cmd && cmd == "AttackPressed")
            {
                _req?.Invoke(ToA1);
            }
        }
    }
}