using Interactable;
using UnityEngine;

public class InteractableLeverAudio : MonoBehaviour
{
    [SerializeField]
    private AK.Wwise.Event _leverInteracted;
    [SerializeField]
    private InteractableLever _interactableLever; 
    
    void OnEnable()
    {
        _interactableLever.onInteract.AddListener(LeverInteracted);
    }
    void OnDisable()
    {
        _interactableLever.onInteract.RemoveListener(LeverInteracted);
    }
    
    void LeverInteracted()
    {
        _leverInteracted.Post(this.gameObject);
    }
    
}
