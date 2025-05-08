using System.Collections;
using UnityEngine;

namespace PickUps
{
    public class Pickup : MonoBehaviour
    {
        [SerializeField] protected float cooldownTime = 5f;
        private Coroutine _cooldownCoroutine;

        protected void RefreshCooldown()
        {
            _cooldownCoroutine ??= StartCoroutine(CooldownRoutine());
        }

        private IEnumerator CooldownRoutine()
        {
            gameObject.SetActive(false);
            yield return new WaitForSeconds(cooldownTime);
            gameObject.SetActive(true);
            _cooldownCoroutine = null;
        }
    }
}