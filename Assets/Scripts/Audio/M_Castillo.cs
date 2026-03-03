using UnityEngine;

public class M_Castillo : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField]
    AK.Wwise.Event m_Castillo;
    
    void OnTriggerEnter(Collider collider)
    {
        m_Castillo.Post(this.gameObject);
        Debug.Log("AUDIO: Entered Castillo");
    }

}
