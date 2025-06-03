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
        [SerializeField] private Transform pendulumArm;
        // [SerializeField] private float knockbackForce;

        private float _time;

        private void Update()
        {
            _time += Time.deltaTime;
            float angle = swingAngle * Mathf.Sin(_time * swingSpeed * Mathf.PI * 2);
            pendulumArm.localRotation = Quaternion.Euler(0f, 0f, angle);
        }

        private void OnTriggerEnter(Collider other)
        {
            //TODO: AGREGAR DAÑO
            // if (!other.TryGetComponent(out ExampleCharacterController characterController)) return;
            // characterController.DeathSequence(transform.position);

            // esto quedaría si seria solo empuje, pero decidí que sea muerte 
            if (!other.CompareTag("Player")) return;
            var characterController = other.GetComponent<ExampleCharacterController>();
            if (!characterController) return;

            characterController.Motor.ForceUnground();
            characterController.Motor.BaseVelocity =
                (((transform.position - characterController.transform.position).normalized * -knockbackForce) +
                 Vector3.up * knockbackForce);
        }
    }
}