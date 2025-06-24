using FSM;
using UnityEngine;

namespace Player.New
{
    public class Jump : State
    {
        private readonly MyKinematicMotor _motor;
        private readonly float _jumpVelocity;
        private readonly float _gravity;
        private readonly float _moveSpeed;
        private float _elapsedTime = 0f;
        private readonly float _maxAirTime = 0.5f;

        private System.Action _onLand;
        private System.Action _onFall;

        public Jump(MyKinematicMotor motor, float jumpVelocity, float gravity, float moveSpeed,
            System.Action onLand, System.Action onFall)
        {
            _motor = motor;
            _jumpVelocity = jumpVelocity;
            _gravity = gravity;
            _moveSpeed = moveSpeed;
            _onLand = onLand;
            _onFall = onFall;
        }

        public override void Enter()
        {
            base.Enter();

            _elapsedTime = 0f;
            Vector3 velocity = _motor.Velocity;
            velocity.y = _jumpVelocity;
            _motor.SetVelocity(velocity);
        }

        public override void Tick(float delta)
        {
            _elapsedTime += delta;
            
            _motor.ApplyGravity(_gravity, delta);

            if (_motor.IsGrounded && _elapsedTime > 0.1f)
            {
                _onLand?.Invoke();
            }
            else if (_elapsedTime >= _maxAirTime)
            {
                _onFall?.Invoke();
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