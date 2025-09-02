using UnityEngine;
using UnityEngine.Events;

namespace Interactable
{
    public class InteractableButton : MonoBehaviour, IInteractable
    {
        [SerializeField] private UnityEvent onInteract;
        [SerializeField] private GameObject indicator;
        private bool interacting;

        public bool IsInteractable()
        {
            return interacting;
        }

        public void Interact()
        {
            onInteract?.Invoke();
        }

        public void ToggleIndicator(bool value)
        {
            indicator.SetActive(value);
        }
    }
}