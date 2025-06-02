using System;
using System.Collections;
using UnityEngine;

namespace Hazards.BlowerBridge
{
    public class BlowerBridge : MonoBehaviour
    {
        private static readonly int IsExtended = Animator.StringToHash("IsExtended");

        private enum ActivationMode
        {
            Interval,
            Button
        }

        [SerializeField] private ActivationMode mode = ActivationMode.Interval;

        [SerializeField] private float interval = 5f;
        [SerializeField] private float activeTime = 2f;
        [SerializeField] private Animator animator;

        private bool _isActive = false;
        private Coroutine _recursiveCoroutine;
        private Coroutine _activateCoroutine;
        
        private void Start()
        {
            if (mode == ActivationMode.Interval)
                StartCoroutine(AutoActivate());
        }

        public void ActivateBridge()
        {
            if (!_isActive)
                StartCoroutine(ActivateSequence());
        }

        private IEnumerator AutoActivate()
        {
            yield return new WaitForSeconds(interval);

            if (!_isActive)
                yield return _activateCoroutine = StartCoroutine(ActivateSequence());

            _recursiveCoroutine = StartCoroutine(AutoActivate());
        }

        private IEnumerator ActivateSequence()
        {
            _isActive = true;

            if (animator)
                animator.SetBool(IsExtended, true);

            yield return new WaitForSeconds(activeTime);

            if (animator)
                animator.SetBool(IsExtended, false);

            _isActive = false;
        }
    }
}