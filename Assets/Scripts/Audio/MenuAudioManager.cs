using UnityEngine;

public class MenuAudioManager : MonoBehaviour
{

    void OnDisable()
    {
        AkUnitySoundEngine.StopAll(this.gameObject);
    }

}

