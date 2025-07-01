using Health;
using Platforms;
using System.Collections.Generic;
using UnityEngine;

public class KillEnemy : MonoBehaviour
{
    [SerializeField] private HammerController hammerController;
    private List<Collider> attacked = new();

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Enemy"))
        {
            if (other.gameObject.TryGetComponent(out HealthController enemy))
            {
                enemy.Damage(1);
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