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

        [FormerlySerializedAs("interactorTargetPosition")] [SerializeField]
        private Transform interactorTargetTransform;

        [SerializeField] private float interactionRange;
        private bool interacting;

        public bool IsBeingInteracted()
        {
            return interacting;
        }

        public void Interact()
        {
            onInteract?.Invoke();
        }

        public bool TryInteractionRange(Vector3 interactor)
        {
            return Vector3.Distance(interactorTargetTransform.position, interactor) <= interactionRange;
        }

        public void SetIndicator(bool value)
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