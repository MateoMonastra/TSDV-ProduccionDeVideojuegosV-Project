using Enemies;
using Platforms;
using UnityEngine;

public class KillEnemy : MonoBehaviour
{
    [SerializeField] private HammerController hammerController;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (other.gameObject.TryGetComponent(out IEnemy enemy))
            {
                enemy.OnBeingAttacked();
            }
        }

        //TODO: YA HAY QUE SACAR ESTE BODRIO
        if (other.gameObject.TryGetComponent(out IBreakable breakable))
        {
            if (hammerController.IsGroundSlamming)
                breakable.Break();
        }
    }
}