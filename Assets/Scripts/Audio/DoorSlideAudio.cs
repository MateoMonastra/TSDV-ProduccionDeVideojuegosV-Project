using UnityEngine;

public class DoorSlideAudio : MonoBehaviour
{
    [SerializeField]
    private AK.Wwise.Event _doorSlide;
    [SerializeField]
    private SlidingDoor _slidingDoor; 
    
    void OnEnable()
    {
        _slidingDoor.SubePorton.AddListener(DoorSlide);
    }
    void OnDisable()
    {
        _slidingDoor.SubePorton.RemoveListener(DoorSlide);
    }
    
    void DoorSlide()
    {
        _doorSlide.Post(this.gameObject);
        Debug.Log("AUDIO: DoorSlide");
    }
    
}
