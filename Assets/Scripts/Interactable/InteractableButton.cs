using UnityEngine;
using UnityEngine.Events;

public class InteractableButton : MonoBehaviour, IInteractable
{
    [SerializeField] private UnityEvent onInteract;

    [Header("Timer Settings")] [SerializeField]
    private bool exitTimerEnabled;

    [SerializeField] private UnityEvent onExitTimer;

    [SerializeField] private float exitTime;

    [SerializeField] private BoxCollider interactorCollider;
    [SerializeField] private float interactionRange;

    private InteractData interactData;
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

    public InteractData Interact(bool hammer)
    {
        interactData.successInteraction = false;

        if (interacting)
            return interactData;

        interacting = true;
        
        onInteract?.Invoke();

        interactData.successInteraction = true;
        return interactData;
    }

    public void FinishInteraction()
    {
        onInteract?.Invoke();

        if (exitTimerEnabled)
            isOnTimer = true;
    }

    public void InterruptInteraction()
    {
        interacting = false;
    }

    public bool TryInteractionRange(Vector3 interactor)
    {
        return true;
    }

    public void SetIndicator(bool value)
    {
        
    }

    public Vector3 GetInteractionPoint()
    {
        return Vector3.zero;
    }
}