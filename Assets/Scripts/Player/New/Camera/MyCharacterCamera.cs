using UnityEngine;
using UnityEngine.InputSystem;

namespace Player.New
{
    [RequireComponent(typeof(Camera))]
    public class MyCharacterCamera : MonoBehaviour
    {
        [Header("Framing")]
        public Vector2 followPointFraming = new Vector2(0f, 0f);
        public float followingSharpness = 10000f;

        [Header("Distance")]
        public float defaultDistance = 6f;
        public float minDistance = 0f;
        public float maxDistance = 10f;
        public float distanceMovementSpeed = 5f;
        public float distanceMovementSharpness = 10f;

        [Header("Rotation")]
        public bool invertX = false;
        public bool invertY = false;
        [Range(-90f, 90f)] public float defaultVerticalAngle = 20f;
        [Range(-90f, 90f)] public float minVerticalAngle = -90f;
        [Range(-90f, 90f)] public float maxVerticalAngle = 90f;
        public float mouseRotationSpeed = 0.1f;
        public float joystickRotationSpeed = 1f;
        public float rotationSharpness = 10000f;
        public bool rotateWithPhysicsMover = false;

        [Header("Obstruction")]
        public float obstructionCheckRadius = 0.2f;
        public LayerMask obstructionLayers = -1;
        public float obstructionSharpness = 10000f;
        public Collider[] ignoredColliders;

        public Transform followTransform;

        private Transform _transform;
        private CameraRotationHandler _rotationHandler;
        private CameraDistanceHandler _distanceHandler;
        private CameraFramingHandler _framingHandler;
        private CameraObstructionHandler _obstructionHandler;

        private Vector3 _currentFollowPosition;

        public Vector3 PlanarDirection => _rotationHandler.PlanarDirection;
        private void Awake()
        {
            _transform = transform;
            _rotationHandler = new CameraRotationHandler(this);
            _distanceHandler = new CameraDistanceHandler(this);
            _framingHandler = new CameraFramingHandler(this);
            _obstructionHandler = new CameraObstructionHandler(this, _distanceHandler);

            _currentFollowPosition = followTransform.position;
        }

        public void UpdateCamera(float deltaTime, float zoomInput, Vector3 rotationInput, InputDevice inputDevice)
        {
            if (followTransform == null) return;

            _rotationHandler.ProcessRotationInput(deltaTime, rotationInput, inputDevice);
            _distanceHandler.ProcessZoomInput(zoomInput);

            _currentFollowPosition = Vector3.Lerp(
                _currentFollowPosition,
                followTransform.position,
                1f - Mathf.Exp(-followingSharpness * deltaTime));

            float currentDistance = _obstructionHandler.GetAdjustedDistance(
                _currentFollowPosition, _rotationHandler.GetCameraRotation(), deltaTime);

            Vector3 targetPosition = _currentFollowPosition -
                                     (_rotationHandler.GetCameraRotation() * Vector3.forward * currentDistance);

            targetPosition = _framingHandler.ApplyFramingOffset(targetPosition, _transform);

            _transform.position = targetPosition;
            _transform.rotation = _rotationHandler.GetCameraRotation();
        }
    }
}