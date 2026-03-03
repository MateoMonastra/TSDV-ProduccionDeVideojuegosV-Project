using UnityEngine;

public class StepSwitch : MonoBehaviour
{
    [SerializeField]
    AK.Wwise.Switch akSwitchStepTo;
    void OnTriggerEnter(Collider collider)
    {
        akSwitchStepTo.SetValue(collider.gameObject);
        Debug.Log("AUDIO: Step Switch" + akSwitchStepTo.ToString());
    }
}
