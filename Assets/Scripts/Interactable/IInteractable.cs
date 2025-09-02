using UnityEngine;

public interface IInteractable
{
    public bool IsInteractable();
    public void Interact();
    public void ToggleIndicator(bool value);
}
