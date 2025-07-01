using FSM;
using UnityEngine;

namespace Player.New
{
    public class WalkIdle : State
    {
        private readonly MyKinematicMotor _motor;
        private System.Action _onFall;
        private PlayerModel _model;

        private float _ungroundedTime;

        public WalkIdle(MyKinematicMotor motor, PlayerModel model, System.Action onFall)
        {
            _motor = motor;
            _model = model;
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
            Vector3 targetVelocity = _model.MoveInput * _model.MoveSpeed;
            Vector3 currentVelocity = _motor.Velocity;
            
            currentVelocity = Vector3.Lerp(
                currentVelocity,
                new Vector3(targetVelocity.x, currentVelocity.y, targetVelocity.z),
                1 - Mathf.Exp(-_model.RotationSharpness * delta)
            );
            
            if (_model.MoveInput.sqrMagnitude > 0.001f)
            {
                Quaternion currentRot = _motor.transform.rotation;

                Quaternion targetRot = Quaternion.LookRotation(_model.MoveInput.normalized, Vector3.up);

                Quaternion newRot = Quaternion.Slerp(
                    currentRot,
                    targetRot,
                    1 - Mathf.Exp(-_model.RotationSharpness * delta)
                );

                _motor.SetRotation(newRot);
            }
            
            if (!_motor.IsGrounded)
            {
                currentVelocity.y += _model.Gravity * 0.1f * delta;
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
        }

        public override void Exit()
        {
            base.Exit();
        }

        public override void HandleInput(params object[] values)
        {
            _model.MoveInput = (Vector3)values[0];
        }
    }
}