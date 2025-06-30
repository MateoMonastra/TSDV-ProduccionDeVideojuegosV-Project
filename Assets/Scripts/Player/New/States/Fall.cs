using FSM;
using UnityEngine;

namespace Player.New
{
    public class Fall : State
    {
        private readonly MyKinematicMotor _motor;
        private readonly float _gravity;
        private readonly float _airAcceleration;
        private readonly float _maxAirSpeed;
        private readonly float _rotationSharpness;

        private Vector3 _moveInputVector;
        private Vector3 _lookInputVector;
        private System.Action _onLand;

        public Fall(MyKinematicMotor motor, float gravity, float airAcceleration,
            float maxAirSpeed, float rotationSharpness, System.Action onLand)
        {
            _motor = motor;
            _gravity = gravity;
            _airAcceleration = airAcceleration;
            _maxAirSpeed = maxAirSpeed;
            _rotationSharpness = rotationSharpness;
            _onLand = onLand;
        }

        public override void Tick(float delta)
        {
            Vector3 velocity = _motor.Velocity;
            velocity.y += _gravity * delta;


            velocity = AirMovement(delta, velocity);

            _motor.SetVelocity(velocity);

            if (_lookInputVector.sqrMagnitude > 0.01f)
            {
                _motor.SmoothRotation(_lookInputVector, _rotationSharpness, delta);
            }

            if (_motor.IsGrounded && velocity.y <= 0)
            {
                _onLand?.Invoke();
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
                Vector3 addedVelocity = _moveInputVector * (_airAcceleration * delta);

                // Limitar velocidad horizontal
                Vector3 horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);
                if (horizontalVelocity.magnitude < _maxAirSpeed)
                {
                    horizontalVelocity = Vector3.ClampMagnitude(horizontalVelocity + addedVelocity, _maxAirSpeed);
                    velocity.x = horizontalVelocity.x;
                    velocity.z = horizontalVelocity.z;
                }
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