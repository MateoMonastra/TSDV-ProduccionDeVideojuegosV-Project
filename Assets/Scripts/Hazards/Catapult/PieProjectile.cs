using System.Collections;
using KinematicCharacterController.Examples;
using UnityEngine;

namespace Hazards.Catapult
{
    public class PieProjectile : MonoBehaviour
    {
        [SerializeField] private LayerMask environmentLayer;
        
        private Rigidbody _rb;
        private bool _hasCollided = false;

        private void OnEnable()
        {
            _rb = GetComponent<Rigidbody>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_hasCollided) return;
            _hasCollided = true;

            if (other.CompareTag("Player"))
            {
                if (!other.GetComponent(typeof(ExampleCharacterController))) return;

                GameEvents.GameEvents.PlayerBlinded();
            }
            else if (other.gameObject.layer == environmentLayer)
            {
                Debug.Log("Environment Hit");
            }

            Destroy(gameObject);
        }
    }
}