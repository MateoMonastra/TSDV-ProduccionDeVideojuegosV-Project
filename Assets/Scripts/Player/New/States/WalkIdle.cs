using FSM;
using UnityEngine;

namespace Player.New
{
    public class WalkIdle : State
    {
        private readonly MyKinematicMotor _motor;
        private readonly float _moveSpeed;
        private readonly float _rotationSharpness;
        private readonly float _gravity;
        
        private Vector3 _moveInputVector;
        private Vector3 _lookInputVector;
        private float _ungroundedTime;
        private System.Action _onFall;

        public WalkIdle(MyKinematicMotor motor, float moveSpeed, float rotationSharpness, 
                       float gravity, System.Action onFall)
        {
            _motor = motor;
            _moveSpeed = moveSpeed;
            _rotationSharpness = rotationSharpness;
            _gravity = gravity;
            _onFall = onFall;
        }

        public override void Enter()
        {
            _ungroundedTime = 0f;
 
            Vector3 velocity = _motor.Velocity;
            velocity.y = Mathf.Min(velocity.y, 0);
            _motor.SetVelocity(velocity);
        }

        public override void Tick(float delta)
        {
            Vector3 targetVelocity = _moveInputVector * _moveSpeed;
            Vector3 currentVelocity = _motor.Velocity;
            
            currentVelocity = Vector3.Lerp(
                currentVelocity,
                new Vector3(targetVelocity.x, currentVelocity.y, targetVelocity.z),
                1 - Mathf.Exp(-_rotationSharpness * delta)
            );
            
            if (!_motor.IsGrounded)
            {
                currentVelocity.y += _gravity * 0.1f * delta; 
                _ungroundedTime += delta;
                
                if (_ungroundedTime > 0.15f)
                {
                    _onFall?.Invoke();
                }
            }
            else
            {
                _ungroundedTime = 0f;
                currentVelocity.y = Mathf.Min(currentVelocity.y, 0);
            }
            
            _motor.SetVelocity(currentVelocity);
            
            if (_lookInputVector.sqrMagnitude > 0.01f)
            {
                _motor.SmoothRotation(_lookInputVector, _rotationSharpness, delta);
            }
        }

        public override void Exit()
        {
            base.Exit();
            _moveInputVector = Vector3.zero;
            _lookInputVector = Vector3.zero;
        }

        public override void HandleInput(params object[] values)
        {
            _moveInputVector = (Vector3)values[0];
            _lookInputVector = values.Length > 1 ? (Vector3)values[1] : _moveInputVector;
        }
    }
}