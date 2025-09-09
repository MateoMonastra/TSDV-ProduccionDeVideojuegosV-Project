using UnityEngine;
using UnityEngine.InputSystem;

namespace Player.New
{
    public class CameraRotationHandler
    {
        private readonly MyCharacterCamera _camera;

        private Vector3 _planarDirection;
        private float _targetVerticalAngle;
        public Vector3 PlanarDirection => _planarDirection;
        public CameraRotationHandler(MyCharacterCamera camera)
        {
            _camera = camera;
            _planarDirection = camera.followTransform ? camera.followTransform.forward : Vector3.forward;
            _targetVerticalAngle = camera.defaultVerticalAngle;
        }

        public void ProcessRotationInput(float deltaTime, Vector3 rotationInput, InputDevice inputDevice)
        {
            float speed = inputDevice == Gamepad.current ? _camera.joystickRotationSpeed : _camera.mouseRotationSpeed;
            float stabilizer = 100f;
            float rotationSpeed = speed / stabilizer;

            if (_camera.invertX) rotationInput.x *= -1f;
            if (_camera.invertY) rotationInput.y *= -1f;

            Quaternion inputRotation = Quaternion.Euler(_camera.followTransform.up * (rotationInput.x * rotationSpeed));
            _planarDirection = inputRotation * _planarDirection;
            _planarDirection = Vector3.Cross(
                _camera.followTransform.up,
                Vector3.Cross(_planarDirection, _camera.followTransform.up)
            );

            _targetVerticalAngle -= rotationInput.y * rotationSpeed;
            _targetVerticalAngle = Mathf.Clamp(
                _targetVerticalAngle,
                _camera.minVerticalAngle,
                _camera.maxVerticalAngle
            );
        }

        public Quaternion GetCameraRotation()
        {
            Quaternion planarRot = Quaternion.LookRotation(_planarDirection, _camera.followTransform.up);
            Quaternion verticalRot = Quaternion.Euler(_targetVerticalAngle, 0f, 0f);
            return planarRot * verticalRot;
        }
    }
}