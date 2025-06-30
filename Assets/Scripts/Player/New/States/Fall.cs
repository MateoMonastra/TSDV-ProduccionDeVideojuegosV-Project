using FSM;
using UnityEngine;

namespace Player.New
{
    public class Fall : State
    {
        private readonly MyKinematicMotor _motor;
        private PlayerModel _model;
        private System.Action _onLand;

        public Fall(MyKinematicMotor motor,PlayerModel model, System.Action onLand)
        {
            _motor = motor;
            _model = model;
            _onLand = onLand;
        }

        public override void Tick(float delta)
        {
            Vector3 velocity = _motor.Velocity;
            velocity.y += _model.Gravity * delta;

            velocity = AirMovement(delta, velocity);

            _motor.SetVelocity(velocity);

            if (_model.LookInput.sqrMagnitude > 0.01f)
            {
                _motor.SmoothRotation(_model.LookInput, _model.RotationSharpness, delta);
            }

            if (_motor.IsGrounded && velocity.y <= 0)
            {
                _onLand?.Invoke();
            }
        }
        
        public override void Exit()
        {
            base.Exit();
        }

        private Vector3 AirMovement(float delta, Vector3 velocity)
        {
            if (_model.MoveInput.sqrMagnitude > 0.01f)
            {
                Vector3 addedVelocity = _model.MoveInput * (_model.AirAcceleration * delta);

                // Limitar velocidad horizontal
                Vector3 horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);
                if (horizontalVelocity.magnitude < _model.MaxAirSpeed)
                {
                    horizontalVelocity = Vector3.ClampMagnitude(horizontalVelocity + addedVelocity, _model.MaxAirSpeed);
                    velocity.x = horizontalVelocity.x;
                    velocity.z = horizontalVelocity.z;
                }
            }

            return velocity;
        }

        public override void HandleInput(params object[] values)
        {
            _model.MoveInput = (Vector3)values[0];
            _model.LookInput = values.Length > 1 ? (Vector3)values[1] : _model.MoveInput;
        }
    }
}