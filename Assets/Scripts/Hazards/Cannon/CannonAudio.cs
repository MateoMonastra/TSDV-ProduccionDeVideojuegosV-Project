using System;
using Hazards.Cannon;
using UnityEngine;

public class CannonAudio : MonoBehaviour
{
    [SerializeField] 
    CannonAgent cannonAgent;
    [SerializeField]
    private AK.Wwise.Event akCannonShoot;

    void OnEnable()
    {
        cannonAgent.onAttack.AddListener(CannonShoot);
    }
    void OnDisable()
    {
        cannonAgent.onAttack.RemoveListener(CannonShoot);
    }
    private void CannonShoot()
    {
        akCannonShoot.Post(this.gameObject);
    }
}
