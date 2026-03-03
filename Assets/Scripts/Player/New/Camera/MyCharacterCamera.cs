using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Player.New
{
    [RequireComponent(typeof(Camera))]
    public class MyCharacterCamera : MonoBehaviour
    {
        [Header("Framing")] public Vector2 followPointFraming = new Vector2(0f, 0f);
        public float followingSharpness = 12f;

        [Header("Distance")] public float defaultDistance = 6f;
        public float minDistance = 0f;
        public float maxDistance = 10f;
        public float distanceMovementSpeed = 5f;
        public float distanceMovementSharpness = 10f;

        [Header("Rotation")] public bool invertX = false;
        public bool invertY = false;
        [Range(-90f, 90f)] public float defaultVerticalAngle = 20f;
        [Range(-90f, 90f)] public float minVerticalAngle = -60f;
        [Range(-90f, 90f)] public float maxVerticalAngle = 80f;
        public float mouseRotationSpeed = 0.1f;
        public float joystickRotationSpeed = 1f;

        [Header("Obstruction")] public float obstructionCheckRadius = 0.2f;
        public LayerMask obstructionLayers = -1;
        public float obstructionSharpness = 12f;
        public Collider[] ignoredColliders;

        [Header("Refs")] public Transform followTransform;
        public InputReader inputReader;

        private Transform _transform;
        private CameraRotationHandler _rotationHandler;
        private CameraDistanceHandler _distanceHandler;
        private CameraFramingHandler _framingHandler;
        private CameraObstructionHandler _obstructionHandler;

        private Vector3 _currentFollowPosition;

        private Vector3 _startingPosition;
        private Quaternion _startingRotation;

        private Vector2 _look;
        private float _zoom;
        private InputDevice _lastDevice;

        public Vector3 PlanarDirection => _rotationHandler.PlanarDirection;

        private void Awake()
        {
            _transform = transform;
            _rotationHandler = new CameraRotationHandler(this);
            _distanceHandler = new CameraDistanceHandler(this);
            _framingHandler = new CameraFramingHandler(this);
            _obstructionHandler = new CameraObstructionHandler(this, _distanceHandler);

            _startingPosition = transform.localPosition;
            _startingRotation = transform.localRotation;

            Cursor.lockState = CursorLockMode.Locked;

            if (followTransform != null)
                _currentFollowPosition = followTransform.position;

            GameEvents.GameEvents.OnPlayerRevived += ResetLook;
        }

        private void OnEnable()
        {
            InputSubscription(true);

            GameEvents.GameEvents.OnGamePaused += PauseTheCamera;
        }

        private void OnDisable()
        {
            InputSubscription(false);
            GameEvents.GameEvents.OnGamePaused -= PauseTheCamera;
        }

        private void OnDestroy()
        {
            GameEvents.GameEvents.OnPlayerRevived -= ResetLook;
        }

        private void CollectFallbackInput()
        {
            if (Mouse.current != null)
            {
                float scrollY = Mouse.current.scroll.ReadValue().y;
                _zoom += scrollY * 0.01f;
            }
        }

        private void PauseTheCamera(bool isGamePaused)
        {
            if (isGamePaused)
            {
                _look = Vector2.zero;
                InputSubscription(false);
            }
            else
            {
                InputSubscription(true);
            }
        }

        private void OnLook(Vector2 delta, InputDevice device)
        {
            _look = delta;
            _lastDevice = device;
        }

        private void ResetLook()
        {
            _look = Vector2.zero;

            if (followTransform != null)
            {
                _currentFollowPosition = followTransform.position;
            }
            
            _rotationHandler.SetCameraRotation(followTransform.rotation);

            UpdateCamera(0f, 0f, Vector3.zero, _lastDevice ?? Mouse.current);
        }

        public void SetCameraRotation(Quaternion rotation)
        {
            _rotationHandler.SetCameraRotation(rotation);
        }

        private void LateUpdate()
        {
            if (followTransform == null)
                return;

            if (inputReader == null) CollectFallbackInput();

            UpdateCamera(Time.deltaTime, _zoom, new Vector3(_look.x, _look.y, 0f), _lastDevice ?? Mouse.current);
            _zoom = 0f;
        }

        private void UpdateCamera(float deltaTime, float zoomInput, Vector3 rotationInput, InputDevice inputDevice)
        {
            if (followTransform == null) return;

            _rotationHandler.ProcessRotationInput(deltaTime, rotationInput, inputDevice);
            _distanceHandler.ProcessZoomInput(zoomInput);

            _currentFollowPosition = Vector3.Lerp(
                _currentFollowPosition,
                followTransform.position,
                1f - Mathf.Exp(-followingSharpness * deltaTime));

            Quaternion camRot = _rotationHandler.GetCameraRotation();

            Vector3 desiredPosition =
                _currentFollowPosition - (camRot * Vector3.forward * _distanceHandler.TargetDistance);
            desiredPosition = _framingHandler.ApplyFramingOffset(desiredPosition, _transform);

            float currentDistance = _obstructionHandler.GetAdjustedDistance(
                _currentFollowPosition, desiredPosition, deltaTime);

            Vector3 dir = (desiredPosition - _currentFollowPosition);
            if (dir.sqrMagnitude > 0.000001f)
            {
                dir.Normalize();
                desiredPosition = _currentFollowPosition + dir * currentDistance;
            }

            _transform.position = desiredPosition;
            _transform.rotation = _rotationHandler.GetCameraRotation();
        }

        public void TriggerCameraShake(float duration = 0.25f, float maxShakeDistance = 8f, float shakeMagnitude = 0.8f)
        {
            StartCoroutine(CameraShakeCoroutine(duration, maxShakeDistance, shakeMagnitude));
        }

        public IEnumerator CameraShakeCoroutine(float duration = 0.25f, float maxShakeDistance = 8f,
            float shakeMagnitude = 0.8f)
        {
            Vector3 originalPos = transform.localPosition;
            float elapsed = 0.0f;

            while (elapsed < duration)
            {
                float x = (Mathf.PerlinNoise(Time.time * maxShakeDistance, 0) - 0.5f) * shakeMagnitude;
                float y = (Mathf.PerlinNoise(0, Time.time * maxShakeDistance) - 0.5f) * shakeMagnitude;

                _framingHandler.SetCameraFollowPointFraming(new Vector2(_framingHandler.DefaultFraming.x + x,
                    _framingHandler.DefaultFraming.y + y));
                elapsed += Time.deltaTime;
                yield return null;
            }

            _framingHandler.ResetCameraFollowPointFraming();
        }
        
        public void InputSubscription(bool subscribe)
        {
            if (subscribe)
            {
                if (inputReader != null)
                    inputReader.OnLook += OnLook;
            }
            else
            {
                if (inputReader != null)
                    inputReader.OnLook -= OnLook;
            }
        }
    }
}