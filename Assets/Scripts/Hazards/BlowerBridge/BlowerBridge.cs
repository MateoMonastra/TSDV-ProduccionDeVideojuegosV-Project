using System;
using System.Collections;
using UnityEngine;

namespace Hazards.BlowerBridge
{
    public class BlowerBridge : MonoBehaviour
    {
        private enum ActivationMode
        {
            Interval,
            Button
        }

        [SerializeField] private ActivationMode mode = ActivationMode.Interval;

        [SerializeField] private float extendTime = 5f;
        [SerializeField] private float interval = 5f;
        [SerializeField] private float activeTime = 2f;
        [SerializeField] private float extendValue = 0.3f;

        private bool _isActive = false;
        private Coroutine _recursiveCoroutine;
        private Coroutine _activateCoroutine;
        private Coroutine _extendCoroutine;
        private Coroutine _retractCoroutine;
        private float _pivotCorrectionFactor = 0.43f; // valor descubierto mediante pruebas

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
            _extendCoroutine = StartCoroutine(Extend());

            yield return new WaitForSeconds(activeTime + extendTime);

            _retractCoroutine = StartCoroutine(Retract());
        }

        private IEnumerator Extend()
        {
            _isActive = true;
            float elapsedTime = 0f;

            Vector3 initialScale = transform.localScale;
            Vector3 targetScale = initialScale;
            targetScale.z += extendValue;

            Vector3 initialPosition = transform.position;

            float deltaZ = targetScale.z - initialScale.z;
            Vector3 targetPosition = initialPosition - transform.forward * (deltaZ * _pivotCorrectionFactor);

            while (elapsedTime < extendTime)
            {
                float t = elapsedTime / extendTime;

                transform.localScale = Vector3.Lerp(initialScale, targetScale, t);
                transform.position = Vector3.Lerp(initialPosition, targetPosition, t);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.localScale = targetScale;
            transform.position = targetPosition;
        }

        private IEnumerator Retract()
        {
            float elapsedTime = 0f;

            Vector3 initialScale = transform.localScale;
            Vector3 targetScale = initialScale;
            targetScale.z -= extendValue;

            Vector3 initialPosition = transform.position;

            float deltaZ = initialScale.z - targetScale.z;
            Vector3 targetPosition = initialPosition + transform.forward * (deltaZ * _pivotCorrectionFactor);

            while (elapsedTime < extendTime)
            {
                float t = elapsedTime / extendTime;

                transform.localScale = Vector3.Lerp(initialScale, targetScale, t);
                transform.position = Vector3.Lerp(initialPosition, targetPosition, t);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            transform.localScale = targetScale;
            transform.position = targetPosition;
            _isActive = false;
        }
    }
}