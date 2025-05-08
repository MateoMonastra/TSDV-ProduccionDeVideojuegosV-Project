using KinematicCharacterController.Examples;
using UnityEngine;

namespace Enemies.Enemy
{
    public class BaseEnemyHitBox : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private float damage;

        [Header("Debug")]
        [SerializeField] private bool activateLogs;

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("Player"))
            {
                //TODO: colocar daño al jugador
                if (!other.GetComponent<ExampleCharacterController>()) return;
                if(activateLogs)
                    Debug.Log("Player hit");
                
                GameEvents.PlayerDied(other.gameObject);
            }
        }
    }
}