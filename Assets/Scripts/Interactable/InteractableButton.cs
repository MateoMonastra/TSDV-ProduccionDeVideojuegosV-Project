using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;
using UnityEngine.UIElements;

namespace Interactable
{
    public class InteractableButton : MonoBehaviour, IInteractable
    {
        [Header("Settings")]
        [SerializeField] private InteractData interactData;
        [SerializeField] private UnityEvent onInteract;
        [SerializeField] private GameObject indicator;

        [Header("Timer Settings")]
        [SerializeField] private UnityEvent onExitTimer;
        [SerializeField] private bool exitTimerEnabled;

        [SerializeField] private float exitTime;

        [SerializeField] private Transform interactorTargetTransform;

        [SerializeField] private float interactionRange;
        private bool interacting;
        private bool isOnTimer;
        private float _currentExitTime;

        public bool IsBeingInteracted()
        {
            return interacting;
        }

        private void Update()
        {
            if (isOnTimer)
            {
                _currentExitTime += Time.deltaTime;

                if (_currentExitTime >= exitTime)
                {
                    _currentExitTime = 0;
                    interacting = false;
                    onExitTimer?.Invoke();
                }
            }
        }

        public InteractData Interact()
        {
            interactData.successInteraction = false;

            if (interacting)
                return interactData;

            onInteract?.Invoke();
            interacting = true;
            isOnTimer = true;

            SetIndicator(false);

            interactData.interactPos = interactorTargetTransform.position;
            interactData.successInteraction = true;
            return interactData;
        }

        public bool TryInteractionRange(Vector3 interactor)
        {
            return Vector3.Distance(interactorTargetTransform.position, interactor) <= interactionRange;
        }

        public void SetIndicator(bool value)
        {
            if (interacting)
                indicator.SetActive(false);
            else
                indicator.SetActive(value);
        }

        public Vector3 GetInteractionPoint()
        {
            return interactorTargetTransform.position;
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(interactorTargetTransform.position, interactionRange);
        }
    }
}