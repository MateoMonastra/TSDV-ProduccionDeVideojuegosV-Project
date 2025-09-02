using UnityEngine;

public interface IInteractable
{
    public bool IsBeingInteracted();
    public void Interact();
    public bool TryInteractionRange(Vector3 interactor);
    public Vector3 GetInteractionPoint();
    public void SetIndicator(bool value);
}
