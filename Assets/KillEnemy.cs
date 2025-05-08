using Enemies;
using UnityEngine;

public class KillEnemy : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name);
        if (!other.CompareTag("Enemy")) return;
        
        if (other.gameObject.TryGetComponent(out IEnemy enemy))
        {
            enemy.OnBeingAttacked();
        }
    }
}