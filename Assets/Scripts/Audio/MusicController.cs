using Unity.VisualScripting;
using UnityEngine;

public class MusicController : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField]
    AK.Wwise.Event m_Alcantarilla;
    [SerializeField]
    AK.Wwise.Event m_Castillo;
    
    
    void Start()
    {
        m_Alcantarilla.Post(this.gameObject);
    }

    void OnTriggerEnter(Collider collider)
    {
        m_Castillo.Post(this.gameObject);
        Debug.Log("AUDIO: Entered Castillo");
    }
}
