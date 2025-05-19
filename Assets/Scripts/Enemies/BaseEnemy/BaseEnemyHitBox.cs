using KinematicCharacterController.Examples;
using UnityEngine;

namespace Enemies.BaseEnemy
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
                if (!other.TryGetComponent(out ExampleCharacterController controller)) return;
                if(activateLogs)
                    Debug.Log("Player hit");
                controller.DeathSequence(transform.position);
            }
        }
    }
}