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

        protected Coroutine CooldownCoroutine;
        protected Collider Collider;
        protected Rigidbody Rb;

        protected virtual void Awake()
        {
            Collider = GetComponent<Collider>();
            Collider.isTrigger = true;

            Rb = GetComponent<Rigidbody>();
            Rb.isKinematic = true;
            Rb.useGravity  = false;
        }

        protected void RefreshCooldown()
        {
            CooldownCoroutine ??= StartCoroutine(CooldownRoutine());
        }

        private IEnumerator CooldownRoutine()
        {
            Collider.enabled = false;
            if (visuals) visuals.SetActive(false);
            if (activateLogs) Debug.Log("Pickup Off");

            yield return new WaitForSeconds(cooldownTime);

            OnCooldown?.Invoke();

            Collider.enabled = true;
            if (visuals) visuals.SetActive(true);
            if (activateLogs) Debug.Log("Pickup On");

            CooldownCoroutine = null;
        }
    }
}