using FSM;
using UnityEngine;

namespace Player.New
{
    public class Jump : State
    {
        private readonly MyKinematicMotor _motor;
        private readonly PlayerModel _model;
        private float _elapsedTime;
        private bool _jumpConsumed;

        
        private System.Action _onFall;

        public Jump(MyKinematicMotor motor, PlayerModel model,System.Action onFall)
        {
            _motor = motor;
            _model = model;
            _onFall = onFall;
        }

        public override void Enter()
        {
            base.Enter();
            _elapsedTime = 0f;
            _jumpConsumed = false;

            Vector3 jumpVelocity = _motor.Velocity;
            jumpVelocity.y = _model.JumpVelocity;

            if (_model.MoveInput.sqrMagnitude > 0.01f)
            {
                jumpVelocity += _model.MoveInput * _model.AirAcceleration;
            }

            _motor.SetVelocity(jumpVelocity);
        }

        public override void Tick(float delta)
        {
            _elapsedTime += delta;

            Vector3 velocity = _motor.Velocity;
            velocity.y += _model.Gravity * delta;

            velocity = AirMovement(delta, velocity);

            _motor.SetVelocity(velocity);

            if (_model.LookInput.sqrMagnitude > 0.01f)
            {
                _motor.SmoothRotation(_model.LookInput, _model.RotationSharpness, delta);
            }

  
            if (velocity.y <= 0 || _elapsedTime > 1.5f)
            {
                _onFall?.Invoke();
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
                Vector3 targetVelocity = _model.MoveInput * _model.AirAcceleration;
                velocity = Vector3.Lerp(velocity,
                    new Vector3(targetVelocity.x, velocity.y, targetVelocity.z),
                    1 - Mathf.Exp(-_model.AirControlSharpness * delta));
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