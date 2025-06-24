using FSM;
using UnityEngine;

namespace Player.New
{
    public class Fall : State
    {
        private readonly MyKinematicMotor _motor;
        private readonly float _gravity;
        private readonly float _moveSpeed;
        private System.Action _onLand;

        public Fall(MyKinematicMotor motor, float gravity,float moveSpeed, System.Action onLand)
        {
            _motor = motor;
            _gravity = gravity;
            _moveSpeed= moveSpeed;
            _onLand = onLand;
        }

        public override void Tick(float delta)
        {
            _motor.ApplyGravity(_gravity, delta);
            
            if (_motor.IsGrounded)
            {
                _onLand?.Invoke();
            }
        }

        public override void HandleInput(params object[] values)
        {
            Vector2 input = (Vector2)values[0];
            _motor.SetInputDirection(input, _moveSpeed);

            if (!(input.sqrMagnitude > 0.01f)) return;
            
            Vector3 forward = new Vector3(input.x, 0f, input.y).normalized;
            _motor.SetRotation(Quaternion.LookRotation(forward));
        }
    }

}