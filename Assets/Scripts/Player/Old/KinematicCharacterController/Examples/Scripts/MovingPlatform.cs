using KinematicCharacterController;
using UnityEngine;
using UnityEngine.Serialization;

namespace Player.Old.KinematicCharacterController.Examples.Scripts
{
    public class MovingPlatform : MonoBehaviour, IMoverController
    {
        [SerializeField] private PhysicsMover mover;

        [Header( "Platform Movement" )]
        [SerializeField] private Vector3 translationAxis = Vector3.right;
        [SerializeField] private float translationPeriod = 10;
        [SerializeField] private float translationSpeed = 1;
        [Header( "Platform Rotation" )]
        [SerializeField] private Vector3 rotationAxis = Vector3.up;
        [SerializeField] private float rotSpeed = 10;
        [Header( "Platform Oscillation" )]
        [SerializeField] private Vector3 oscillationAxis = Vector3.zero;
        [SerializeField] private float oscillationPeriod = 10;
        [SerializeField] private float oscillationSpeed = 10;

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
            goalPosition = (_originalPosition + (translationAxis.normalized *
                                                 (Mathf.Sin(Time.time * translationSpeed) * translationPeriod)));

            Quaternion targetRotForOscillation =
                Quaternion.Euler(oscillationAxis.normalized *
                                 (Mathf.Sin(Time.time * oscillationSpeed) * oscillationPeriod)) * _originalRotation;
            goalRotation = Quaternion.Euler(rotationAxis * (rotSpeed * Time.time)) * targetRotForOscillation;
        }
    }
}