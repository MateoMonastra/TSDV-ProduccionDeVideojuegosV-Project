using Enemies;
using Platforms;
using UnityEngine;

public class KillEnemy : MonoBehaviour
{
    [SerializeField] private HammerController hammerController;

    private void OnTriggerEnter(Collider other)
    {
        Debug.Log(other.gameObject.name);
        if (other.CompareTag("Enemy"))
        {
            if (other.gameObject.TryGetComponent(out IEnemy enemy))
            {
                enemy.OnBeingAttacked();
            }
        }

        if (other.gameObject.TryGetComponent(out IBreakable breakable))
        {
            if (hammerController.IsGroundSlamming)
                breakable.Break();
        }
    }
}