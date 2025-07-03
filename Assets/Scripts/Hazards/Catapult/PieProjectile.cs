using System.Collections;
using System.Collections.Generic;
using KinematicCharacterController.Examples;
using UnityEngine;

namespace Hazards.Catapult
{
    public class PieProjectile : MonoBehaviour
    {
        [SerializeField] private LayerMask environmentLayer;
        [SerializeField] private ParticleSystem environmentHit;
        [SerializeField] private List<ParticleSystem> OnHit;
        [SerializeField] private GameObject model;

        private bool _hasCollided = false;
        private float destroyOffset = 3.0f;

        private void OnTriggerEnter(Collider other)
        {
            if (_hasCollided) return;
            _hasCollided = true;

            if (other.CompareTag("Player"))
            {
                if (!other.GetComponent(typeof(ExampleCharacterController))) return;

                GameEvents.GameEvents.PlayerBlinded();
                PlayOnHit();
            }
            else if (other.gameObject.layer == environmentLayer)
            {
                environmentHit.Play();
                PlayOnHit();
            }

            model.SetActive(false);
            Destroy(gameObject, destroyOffset);
        }

        private void PlayOnHit()
        {
            foreach (var effect in OnHit)
            {
                effect.Play();
            }
        }
    }
}