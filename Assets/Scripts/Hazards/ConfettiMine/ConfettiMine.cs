using System;
using System.Collections;
using KinematicCharacterController.Examples;
using UnityEngine;

namespace Hazards.ConfettiMine
{
    public class ConfettiMine : MonoBehaviour
    {
        [SerializeField] private float mineRadius;
        [SerializeField] private float knockbackForce = 8f;
        [SerializeField] private float activationDelay = 2f;
        [SerializeField] private ParticleSystem explosionEffect;
        [SerializeField] private ParticleSystem confettiEffect;
        [SerializeField] private Renderer mineRenderer;

        private bool _isTriggered = false;
        private Color _warningColor = Color.red;
        private Coroutine _warningCoroutine;

        private void OnTriggerEnter(Collider other)
        {
            if (_isTriggered) return;

            if (!other.CompareTag("Player")) return;
            _isTriggered = true;
            _warningCoroutine = StartCoroutine(ActivateMine(other));
        }

        private IEnumerator ActivateMine(Collider player)
        {
            if (mineRenderer)
                mineRenderer.material.color = _warningColor;
            
            yield return new WaitForSeconds(activationDelay);

            Explode();
        }

        private void Explode()
        {
            if (explosionEffect)
                explosionEffect.Play();

            if (confettiEffect)
                confettiEffect.Play();
            
            // hecho asi por si quieren integrar que los enemigos les afecte tambien 
            Collider[] colliders = Physics.OverlapSphere(transform.position, mineRadius);
            foreach (var entity in colliders)
            {
                var characterController = entity.GetComponent<ExampleCharacterController>();
                
                if (!characterController) continue;
                //TODO: AGREGAR DAÑO 
                characterController.Motor.ForceUnground();
                characterController.Motor.BaseVelocity = (((transform.position - characterController.transform.position).normalized * -knockbackForce) + Vector3.up * knockbackForce);
            }
            
            Destroy(gameObject);
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            if (_isTriggered)
                Gizmos.DrawWireSphere(transform.position, mineRadius);
        }
    }
}