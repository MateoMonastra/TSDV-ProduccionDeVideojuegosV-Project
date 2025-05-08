using System.Collections;
using UnityEngine;

namespace PickUps
{
    [RequireComponent(typeof(BoxCollider))]
    public class Pickup : MonoBehaviour
    {
        [SerializeField] protected float cooldownTime = 5f;
        [SerializeField] private GameObject visuals;
        [SerializeField] bool activateLogs;

        private Coroutine _cooldownCoroutine;
        private Collider _collider;

        protected virtual void Awake()
        {
            _collider = GetComponent<Collider>();
        }

        protected void RefreshCooldown()
        {
            _cooldownCoroutine ??= StartCoroutine(CooldownRoutine());
        }

        private IEnumerator CooldownRoutine()
        {
            _collider.enabled = false;

            if (visuals)
                visuals.SetActive(false);
            if (activateLogs)
                Debug.Log("Pickup Off");

            yield return new WaitForSeconds(cooldownTime);

            _collider.enabled = true;
            
            if (visuals)
                visuals.SetActive(true);
            if (activateLogs)
                Debug.Log("Pickup On");

            _cooldownCoroutine = null;
        }
    }
}