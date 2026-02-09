using UnityEngine;

public class PlayerFollowerListener : MonoBehaviour
{
    [SerializeField]
    private GameObject player;
    [SerializeField]
    private GameObject characterCamera;

    
    
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        transform.position = player.transform.position;
        transform.eulerAngles = new Vector3(0, characterCamera.transform.rotation.eulerAngles.y, 0);
    }
}
