using Health;
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
        [SerializeField] private int damage = 1;
        [SerializeField] private (int, int) knockback = (20,35);

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

            transform.localRotation = _initialRotation * Quaternion.AngleAxis(angle, Vector3.forward);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            var controller = other.GetComponent<HealthController>();
            if (!controller) return;
            controller.Damage(new DamageInfo(damage, transform.position,knockback));

            // esto quedaría si seria solo empuje
            // characterController.Motor.ForceUnground();
            // characterController.Motor.BaseVelocity =
            //     (((transform.position - characterController.transform.position).normalized * -knockbackForce) +
            //      Vector3.up * knockbackForce);
        }
    }
}