using UnityEngine;
using UnityEngine.InputSystem;

namespace Player.New
{
    [RequireComponent(typeof(Camera))]
    public class MyCharacterCamera : MonoBehaviour
    {
        [Header("Framing")]
        public Vector2 followPointFraming = new Vector2(0f, 0f);
        public float followingSharpness = 12f;

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
        [Range(-90f, 90f)] public float minVerticalAngle = -60f;
        [Range(-90f, 90f)] public float maxVerticalAngle = 80f;
        public float mouseRotationSpeed = 0.1f;
        public float joystickRotationSpeed = 1f;
        public float rotationSharpness = 12f;
        public bool rotateWithPhysicsMover = false;

        [Header("Obstruction")]
        public float obstructionCheckRadius = 0.2f;
        public LayerMask obstructionLayers = -1;
        public float obstructionSharpness = 12f;
        public Collider[] ignoredColliders;

        [Header("Refs")]
        public Transform followTransform;       // Player/CameraPivot (o el player)
        public InputReader inputReader;         // tu lector de inputs (opcional)

        // --- internos ---
        private Transform _transform;
        private CameraRotationHandler _rotationHandler;
        private CameraDistanceHandler _distanceHandler;
        private CameraFramingHandler _framingHandler;
        private CameraObstructionHandler _obstructionHandler;

        private Vector3 _currentFollowPosition;

        // buffers de input
        private Vector2 _look;
        private float _zoom;                    // acumulado por frame (scroll)
        private InputDevice _lastDevice;

        public Vector3 PlanarDirection => _rotationHandler.PlanarDirection;

        private void Awake()
        {
            _transform = transform;
            _rotationHandler = new CameraRotationHandler(this);
            _distanceHandler = new CameraDistanceHandler(this);
            _framingHandler = new CameraFramingHandler(this);
            _obstructionHandler = new CameraObstructionHandler(this, _distanceHandler);

            Cursor.lockState = CursorLockMode.Locked;
            
            if (followTransform != null)
                _currentFollowPosition = followTransform.position;
        }

        private void OnEnable()
        {
            if (inputReader != null)
            {
                inputReader.OnLook += OnLook;
                // Si tu InputReader expone un evento de zoom, podés suscribirlo aquí.
            }
        }

        private void OnDisable()
        {
            if (inputReader != null)
            {
                inputReader.OnLook -= OnLook;
            }
        }

        // recolecta input si no hay InputReader (fallback a scroll del mouse)
        private void CollectFallbackInput()
        {
            // Zoom por scroll (Input System)
            if (Mouse.current != null)
            {
                float scrollY = Mouse.current.scroll.ReadValue().y; // típico ±120
                _zoom += scrollY * 0.01f; // escala razonable
            }
        }

        private void OnLook(Vector2 delta, InputDevice device)
        {
            _look = delta;
            _lastDevice = device;
        }

        private void LateUpdate()
        {
            if (followTransform == null)
                return;

            // fallback de zoom si no hay evento dedicado
            if (inputReader == null) CollectFallbackInput();

            // llamar al motor de cámara cada frame
            UpdateCamera(Time.deltaTime, _zoom, new Vector3(_look.x, _look.y, 0f), _lastDevice ?? Mouse.current);

            // limpiar buffers que son "por frame"
            _zoom = 0f;
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
