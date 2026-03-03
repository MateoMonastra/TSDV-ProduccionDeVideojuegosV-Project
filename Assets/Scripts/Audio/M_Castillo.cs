using UnityEngine;

public class M_Castillo : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField]
    AK.Wwise.Event m_Castillo;

    private bool _entered;
    
    void Start()
    {
        _entered = false;
    }
    
    void OnTriggerEnter(Collider collider)
    {
        if (_entered == false)
        {
            m_Castillo.Post(this.gameObject);
            _entered = true;
            //Debug.Log("AUDIO: Entered Castillo");
        }
    }

}
