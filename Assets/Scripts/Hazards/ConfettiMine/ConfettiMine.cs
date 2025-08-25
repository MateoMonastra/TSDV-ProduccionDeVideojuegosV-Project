using System;
using System.Collections;
using KinematicCharacterController.Examples;
using UnityEngine;
using UnityEngine.Serialization;

namespace Hazards.ConfettiMine
{
    public class ConfettiMine : MonoBehaviour
    {
        [SerializeField] private float mineRadius;
        [SerializeField] private float knockbackForce = 20f;
        [SerializeField] private float activationDelay = 2f;
        [SerializeField] private ParticleSystem[] explosionEffects;
        [SerializeField] private Renderer mineRenderer;
        [SerializeField] private Material baseMaterial;
        [SerializeField] private Material warningMaterial;
        [SerializeField] private AnimationCurve blinkCurve;
        [SerializeField] private Collider collider;

        private bool _isTriggered = false;
        private Coroutine _warningCoroutine;
        private Coroutine _explodeCoroutine;

        private void Awake()
        {
            GameEvents.GameEvents.OnPlayerDied += Respawn;
        }

        private void OnDestroy()
        {
            GameEvents.GameEvents.OnPlayerDied -= Respawn;
        }

        private void Respawn(GameObject player)
        {
            if (mineRenderer)
                mineRenderer.material = baseMaterial;

            if (_explodeCoroutine != null)
                StopCoroutine(_explodeCoroutine);

            if (_warningCoroutine != null)
                StopCoroutine(_warningCoroutine);

            mineRenderer.enabled = true;
            collider.enabled = true;
            _isTriggered = false;
            //gameObject.SetActive(true);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_isTriggered) return;

            if (!other.CompareTag("Player")) return;
            _isTriggered = true;
            _warningCoroutine = StartCoroutine(ActivateMine(other));
        }

        private IEnumerator ActivateMine(Collider player)
        {
            float elapsed = 0f;
            bool usingBase = false;

            while (elapsed < activationDelay)
            {
                if (mineRenderer)
                    mineRenderer.material = usingBase ? baseMaterial : warningMaterial;

                usingBase = !usingBase;

                float normalizedTime = Mathf.Clamp01(elapsed / activationDelay);
                float blinkInterval = blinkCurve.Evaluate(normalizedTime);

                yield return new WaitForSeconds(blinkInterval);
                elapsed += blinkInterval;
            }

            _explodeCoroutine = StartCoroutine(Explode());
        }

        private IEnumerator Explode()
        {
            for (int i = 0; i < explosionEffects.Length; i++)
            {
                explosionEffects[i].Play();
            }

            collider.enabled = false;
            mineRenderer.enabled = false;
            
            // hecho asi por si quieren integrar que los enemigos les afecte tambien 
            Collider[] colliders = Physics.OverlapSphere(transform.position, mineRadius);
            foreach (var entity in colliders)
            {
                var characterController = entity.GetComponent<ExampleCharacterController>();

                if (!characterController) continue;
                //TODO: AGREGAR DAÑO 
                characterController.Motor.ForceUnground();
                characterController.Motor.BaseVelocity =
                    (((transform.position - characterController.transform.position).normalized * -knockbackForce) +
                     Vector3.up * knockbackForce);
            }

            //gameObject.SetActive(false);
            yield break;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            if (_isTriggered)
                Gizmos.DrawWireSphere(transform.position, mineRadius);
        }
    }
}