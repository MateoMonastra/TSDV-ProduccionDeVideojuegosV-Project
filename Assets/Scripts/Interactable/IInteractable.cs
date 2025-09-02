using UnityEngine;

public interface IInteractable
{
    public bool IsInteractable();
    public void Interact();
    public bool TryInteractionRange(Vector3 interactor);
    public Vector3 GetInteractionPoint();
    public void ToggleIndicator(bool value);
}
