using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace PickUps
{
    [RequireComponent(typeof(Collider))]
    [RequireComponent(typeof(Rigidbody))]
    public class Pickup : MonoBehaviour
    {
        [SerializeField] protected UnityEvent OnCooldown;
        [SerializeField] protected float cooldownTime = 5f;
        [SerializeField] private GameObject visuals;
        [SerializeField] bool activateLogs;

        private Coroutine _cooldownCoroutine;
        private Collider _collider;
        private Rigidbody _rb;

        protected virtual void Awake()
        {
            _collider = GetComponent<Collider>();
            _collider.isTrigger = true;

            _rb = GetComponent<Rigidbody>();
            _rb.isKinematic = true;
            _rb.useGravity  = false;
        }

        protected void RefreshCooldown()
        {
            _cooldownCoroutine ??= StartCoroutine(CooldownRoutine());
        }

        private IEnumerator CooldownRoutine()
        {
            _collider.enabled = false;
            if (visuals) visuals.SetActive(false);
            if (activateLogs) Debug.Log("Pickup Off");

            yield return new WaitForSeconds(cooldownTime);

            OnCooldown?.Invoke();

            _collider.enabled = true;
            if (visuals) visuals.SetActive(true);
            if (activateLogs) Debug.Log("Pickup On");

            _cooldownCoroutine = null;
        }
    }
}