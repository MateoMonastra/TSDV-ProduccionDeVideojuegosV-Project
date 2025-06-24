using FSM;
using UnityEngine;

namespace Player.New
{
    public class WalkIdle : State
    {
        private readonly MyKinematicMotor _motor;
        private readonly float _moveSpeed;
        private readonly float _gravity;
        private System.Action _onFall;
        private float _ungroundedTimer;
        private Vector2 _currentInput;

        public WalkIdle(MyKinematicMotor motor, float moveSpeed, float gravity, System.Action onFall)
        {
            _motor = motor;
            _moveSpeed = moveSpeed;
            _gravity = gravity;
            _onFall = onFall;
        }

        public override void Enter()
        {
            _motor.SetVelocity(new Vector3(_motor.Velocity.x, 0, _motor.Velocity.z));
        }

        public override void Tick(float delta)
        {
            ApplyMovement(_currentInput, delta);
            
            HandleGravityAndTransitions(delta);
        }

        public override void HandleInput(params object[] values)
        {
            _currentInput = (Vector2)values[0];

            if (!(_currentInput.sqrMagnitude > 0.01f)) return;
            Vector3 forward = new Vector3(_currentInput.x, 0f, _currentInput.y).normalized;
            _motor.SetRotation(Quaternion.LookRotation(forward));
        }

        private void ApplyMovement(Vector2 input, float deltaTime)
        {
            Vector3 planarVelocity = new Vector3(input.x, 0f, input.y) * _moveSpeed;
            
            Vector3 newVelocity = new Vector3(planarVelocity.x, _motor.Velocity.y, planarVelocity.z);
            _motor.SetVelocity(newVelocity);
        }

        private void HandleGravityAndTransitions(float deltaTime)
        {
            if (!_motor.IsGrounded)
            {
                _motor.ApplyGravity(_gravity, deltaTime);
                _ungroundedTimer += deltaTime;
                
                if (_ungroundedTimer > 0.1f)
                {
                    _onFall?.Invoke();
                    _ungroundedTimer = 0f;
                }
            }
            else
            {
                _ungroundedTimer = 0f;

                if (!(_motor.Velocity.y < 0)) return;
                Vector3 velocity = _motor.Velocity;
                velocity.y = 0;
                _motor.SetVelocity(velocity);
            }
        }
    }
}