using System;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

namespace Interactable
{
    public class InteractableButton : MonoBehaviour, IInteractable
    {
        [SerializeField] private UnityEvent onInteract;
        [SerializeField] private GameObject indicator;
        [FormerlySerializedAs("interactorTargetPosition")] [SerializeField] private Transform interactorTargetTransform;
        [SerializeField] private float interactionRange;
        private bool interacting;

        public bool IsInteractable()
        {
            return interacting;
        }

        public void Interact()
        {
            Debug.Log("success");
            onInteract?.Invoke();
        }

        public bool TryInteractionRange(Vector3 interactor)
        {
            if(Vector3.Distance(interactorTargetTransform.position,interactor) <= interactionRange)
            {
                return true;
            }

            return false;
        }

        public void ToggleIndicator(bool value)
        {
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