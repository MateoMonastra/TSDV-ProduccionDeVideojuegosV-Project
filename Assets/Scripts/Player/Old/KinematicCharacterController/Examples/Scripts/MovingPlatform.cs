using KinematicCharacterController;
using UnityEngine;
using UnityEngine.Serialization;

namespace Player.Old.KinematicCharacterController.Examples.Scripts
{
    public class MovingPlatform : MonoBehaviour, IMoverController
    {
        [SerializeField] private PhysicsMover mover;

        [Header("Platform Movement")]
        [SerializeField] private Vector3 translationAxis = Vector3.right;
        [SerializeField] private float translationDistance = 10f;
        [SerializeField] private float translationSpeed = 1f;

        [Header("Platform Rotation")]
        [SerializeField] private Vector3 rotationAxis = Vector3.up;
        [SerializeField] private float rotSpeed = 10f;

        [Header("Platform Oscillation")]
        [SerializeField] private Vector3 oscillationAxis = Vector3.zero;
        [SerializeField] private float oscillationAmplitude = 10f;
        [SerializeField] private float oscillationSpeed = 10f;

        private Vector3 _originalPosition;
        private Quaternion _originalRotation;

        private void Start()
        {
            _originalPosition = mover.Rigidbody.position;
            _originalRotation = mover.Rigidbody.rotation;

            mover.MoverController = this;
        }

        public void UpdateMovement(out Vector3 goalPosition, out Quaternion goalRotation, float deltaTime)
        {
            float moveAmount = Mathf.PingPong(Time.time * translationSpeed, translationDistance);
            goalPosition = _originalPosition + translationAxis.normalized * moveAmount;

            Quaternion targetOscillation = Quaternion.Euler(
                oscillationAxis.normalized * 
                (Mathf.Sin(Time.time * oscillationSpeed) * oscillationAmplitude)
            );

            goalRotation = Quaternion.Euler(rotationAxis * (rotSpeed * Time.time)) * _originalRotation * targetOscillation;
        }
    }
}