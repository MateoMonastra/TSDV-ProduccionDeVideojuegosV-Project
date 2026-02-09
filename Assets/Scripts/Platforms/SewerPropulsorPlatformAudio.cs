using UnityEngine;
using Platforms;
public class SewerPropulsorPlatformAudio : MonoBehaviour
{
    [SerializeField]
    private AK.Wwise.Event _akPropulsorPlatform;

    [SerializeField]
    private PropulsorPlatform _propulsorPlatform;

    void OnEnable()
    {
        _propulsorPlatform.onBounce += PlayPropulsorPlatformAudio;
    }

    void OnDisable()
    {
        _propulsorPlatform.onBounce -= PlayPropulsorPlatformAudio;
    }

    void PlayPropulsorPlatformAudio()
    {
        _akPropulsorPlatform.Post(this.gameObject);
    }
    
}
