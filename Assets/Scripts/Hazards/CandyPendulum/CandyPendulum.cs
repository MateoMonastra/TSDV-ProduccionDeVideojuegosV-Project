using KinematicCharacterController.Examples;
using Unity.Cinemachine;
using UnityEngine;

namespace Hazards.CandyPendulum
{
    public class CandyPendulum : MonoBehaviour
    {
        [SerializeField] private float swingAngle = 45f;
        [SerializeField] private float swingSpeed = 1.5f;
        [SerializeField] private float knockbackForce = 20f;

        private float _time;
        private Quaternion _initialRotation;
        private void OnEnable()
        {
            _initialRotation = transform.localRotation;
        }
        private void Update()
        {
            _time += Time.deltaTime;
            float angle = swingAngle * Mathf.Sin(_time * swingSpeed * Mathf.PI * 2);
            
            transform.localRotation = _initialRotation * Quaternion.AngleAxis(angle, Vector3.right);
        }

        private void OnTriggerEnter(Collider other)
        {
            //TODO: AGREGAR DAÑO
            // if (!other.TryGetComponent(out ExampleCharacterController characterController)) return;

            if (!other.CompareTag("Player")) return;
            var characterController = other.GetComponent<ExampleCharacterController>();
            if (!characterController) return;
                characterController.DeathSequence(transform.position);

            // esto quedaría si seria solo empuje
            // characterController.Motor.ForceUnground();
            // characterController.Motor.BaseVelocity =
            //     (((transform.position - characterController.transform.position).normalized * -knockbackForce) +
            //      Vector3.up * knockbackForce);
        }
    }
}