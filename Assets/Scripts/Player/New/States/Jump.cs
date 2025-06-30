using FSM;
using UnityEngine;

namespace Player.New
{
    public class Jump : State
    {
        private readonly MyKinematicMotor _motor;
        private readonly float _jumpUpSpeed;
        private readonly float _jumpScalableForwardSpeed;
        private readonly float _gravity;
        private readonly float _rotationSharpness;
        private readonly float _airControlSharpness;

        private Vector3 _moveInputVector;
        private Vector3 _lookInputVector;
        private float _elapsedTime;
        private bool _jumpConsumed;

        
        private System.Action _onFall;

        public Jump(MyKinematicMotor motor, float jumpUpSpeed, float jumpScalableForwardSpeed,
            float gravity, float rotationSharpness, float airControlSharpness,System.Action onFall)
        {
            _motor = motor;
            _jumpUpSpeed = jumpUpSpeed;
            _jumpScalableForwardSpeed = jumpScalableForwardSpeed;
            _gravity = gravity;
            _rotationSharpness = rotationSharpness;
            _airControlSharpness = airControlSharpness;
            _onFall = onFall;
        }

        public override void Enter()
        {
            base.Enter();
            _elapsedTime = 0f;
            _jumpConsumed = false;

            Vector3 jumpVelocity = _motor.Velocity;
            jumpVelocity.y = _jumpUpSpeed;

            if (_moveInputVector.sqrMagnitude > 0.01f)
            {
                jumpVelocity += _moveInputVector * _jumpScalableForwardSpeed;
            }

            _motor.SetVelocity(jumpVelocity);
        }

        public override void Tick(float delta)
        {
            _elapsedTime += delta;

            Vector3 velocity = _motor.Velocity;
            velocity.y += _gravity * delta;

            velocity = AirMovement(delta, velocity);

            _motor.SetVelocity(velocity);

            if (_lookInputVector.sqrMagnitude > 0.01f)
            {
                _motor.SmoothRotation(_lookInputVector, _rotationSharpness, delta);
            }

  
            if (velocity.y <= 0 || _elapsedTime > 1.5f)
            {
                _onFall?.Invoke();
            }
        }

        public override void Exit()
        {
            base.Exit();
            _moveInputVector = Vector3.zero;
            _lookInputVector = Vector3.zero;
        }

        private Vector3 AirMovement(float delta, Vector3 velocity)
        {
            if (_moveInputVector.sqrMagnitude > 0.01f)
            {
                Vector3 targetVelocity = _moveInputVector * _jumpScalableForwardSpeed;
                velocity = Vector3.Lerp(velocity,
                    new Vector3(targetVelocity.x, velocity.y, targetVelocity.z),
                    1 - Mathf.Exp(-_airControlSharpness * delta));
            }

            return velocity;
        }

        public override void HandleInput(params object[] values)
        {
            _moveInputVector = (Vector3)values[0];
            _lookInputVector = values.Length > 1 ? (Vector3)values[1] : _moveInputVector;
        }
    }
}