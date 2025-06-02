using System.Collections;
using KinematicCharacterController.Examples;
using UnityEngine;

namespace Hazards.ConfettiMine
{
    public class ConfettiMine : MonoBehaviour
    {
        [SerializeField] private float knockbackForce = 8f;
        [SerializeField] private float activationDelay = 2f;
        [SerializeField] private ParticleSystem explosionEffect;
        [SerializeField] private ParticleSystem confettiEffect;
        [SerializeField] private Renderer mineRenderer;

        private bool _isTriggered = false;
        private Color _warningColor = Color.red;

        private void OnTriggerEnter(Collider other)
        {
            if (_isTriggered) return;

            if (!other.CompareTag("Player")) return;
            _isTriggered = true;
            StartCoroutine(ActivateMine(other));
        }

        private IEnumerator ActivateMine(Collider player)
        {
            if (mineRenderer)
                mineRenderer.material.color = _warningColor;
            
            yield return new WaitForSeconds(activationDelay);

            Explode(player);
        }

        private void Explode(Collider player)
        {
            if (explosionEffect)
                explosionEffect.Play();

            if (confettiEffect)
                confettiEffect.Play();

            //TODO: AGREGAR DAÑO 

            var characterController = player.GetComponent<ExampleCharacterController>();
            if (characterController)
            {
                characterController.Motor.ForceUnground();
                characterController.Motor.BaseVelocity = (((transform.position - characterController.transform.position).normalized * knockbackForce) + Vector3.up * knockbackForce);
            }
            
            Destroy(gameObject);
        }
    }
}