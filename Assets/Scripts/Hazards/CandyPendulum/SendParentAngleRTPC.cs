using UnityEngine;
using Hazards.CandyPendulum;

public class SendAngleRTPC : MonoBehaviour
{
    
    private CandyPendulum _pendulum;

    [SerializeField]
    private AK.Wwise.RTPC angleRTPC;
    
    void Start()
    {
        _pendulum = GetComponentInParent<CandyPendulum>();
        if (!_pendulum)
        {
            Debug.LogWarning("SendParentAngleRTPC: No CandyPendulum found on parent object.");
        }
    }

    
    void Update()
    {
        if (_pendulum)
        {
            float angle = _pendulum.transform.localRotation.eulerAngles.z;
            if (angle > 180f)
            {
                angle -= 360f;
            }
            angleRTPC.SetValue(gameObject, angle);
        }
    }
}
