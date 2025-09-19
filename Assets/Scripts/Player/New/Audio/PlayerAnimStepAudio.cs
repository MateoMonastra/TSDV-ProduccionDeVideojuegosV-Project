using UnityEngine;

public class PlayerAnimStepAudio : MonoBehaviour
{
    [SerializeField]
    private AK.Wwise.Event _akPlayerStep;
    [SerializeField]
    private GameObject PlayerAudio;

    public void OnStepAudio()
    {
        Debug.Log("Step");
        _akPlayerStep.Post(PlayerAudio);
    }
}
