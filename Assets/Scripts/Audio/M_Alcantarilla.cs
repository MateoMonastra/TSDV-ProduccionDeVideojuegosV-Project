using UnityEngine;

public class M_Alcantarilla : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    [SerializeField]
    AK.Wwise.Event m_Alcantarilla;
    [SerializeField]
    AK.Wwise.Event m_StopAlcantarilla;
    void Start()
    {
        m_Alcantarilla.Post(this.gameObject);
    }

    // Update is called once per frame
    void OnTriggerEnter(Collider collider)
    {
        m_StopAlcantarilla.Post(this.gameObject);
    }
}
