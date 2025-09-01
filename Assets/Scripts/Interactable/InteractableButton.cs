using UnityEngine;
using UnityEngine.Events;

namespace Interactable
{
    public class InteractableButton : IInteractable
    {
        [SerializeField] private UnityEvent onInteract;
        private bool interacting;

        public bool IsInteractable()
        {
            return interacting;
        }

        public void Interact()
        {
            onInteract?.Invoke();
        }
    }
}