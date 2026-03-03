using System;
using Hazards.Cannon;
using UnityEngine;

public class CannonAudio : MonoBehaviour
{
    [SerializeField] 
    private CannonAgent cannonAgent;
    [SerializeField]
    private AK.Wwise.Event akCannonShoot;
    [SerializeField]
    private AK.Wwise.Event akCannonMovementStart ;
    [SerializeField]
    private AK.Wwise.Event akCannonMovementStop;

    void OnEnable()
    {
        cannonAgent.onAttack.AddListener(CannonShoot);
        cannonAgent.onRotate.AddListener(CannonMoving);
        cannonAgent.onRotate.AddListener(CannonStopped);
    }


    void OnDisable()
    {
        cannonAgent.onAttack.RemoveListener(CannonShoot);
        cannonAgent.onRotate.RemoveListener(CannonMoving);
        cannonAgent.onRotate.RemoveListener(CannonStopped);
    }

    private void CannonShoot()
    {
        akCannonShoot.Post(this.gameObject);
    }
    private void CannonMoving()
    {
        akCannonMovementStart.Post(this.gameObject);
        Debug.Log("AUDIO: Cannon Moving");
    }
    private void CannonStopped()
    {
        akCannonMovementStop.Post(this.gameObject);
        Debug.Log("AUDIO: Cannon Stopped");
    }
    
}

