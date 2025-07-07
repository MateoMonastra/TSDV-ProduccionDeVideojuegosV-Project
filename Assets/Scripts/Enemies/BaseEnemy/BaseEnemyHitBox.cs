using Health;
using KinematicCharacterController.Examples;
using UnityEngine;

namespace Enemies.BaseEnemy
{
    public class BaseEnemyHitBox : MonoBehaviour
    {
        [Header("Stats")]
        [SerializeField] private int damage = 1;
        [SerializeField] private (int, int) knockback = (20,35);

        [Header("Debug")]
        [SerializeField] private bool activateLogs;

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag("Player")) return;
            
            if (!other.TryGetComponent(out HealthController controller)) return;
                
            if(activateLogs)
                Debug.Log("Player hit");
                
            controller.Damage(new DamageInfo(damage,transform.position,knockback));
        }
    }
}