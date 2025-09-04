using System;
using UnityEngine;

public interface IInteractable
{
    public bool IsBeingInteracted();
    public InteractData Interact();
    public bool TryInteractionRange(Vector3 interactor);
    public Vector3 GetInteractionPoint();
    public void SetIndicator(bool value);
}

[Serializable]
public struct InteractData
{
    public Vector3 interactPos;
    public bool successInteraction;
    public string animHash;
}