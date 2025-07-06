using Health;
using Platforms;
using System.Collections.Generic;
using UnityEngine;

public class KillEnemy : MonoBehaviour
{
    [SerializeField] private HammerController hammerController;
    private bool _dealingDamage = false;
    private void OnTriggerEnter(Collider other)
    {
        if (_dealingDamage)
        {
            if (other.CompareTag("Enemy"))
            {
                if (other.gameObject.TryGetComponent(out HealthController enemy))
                {
                    enemy.Damage(1);
                    hammerController.ToggleAttackCollider(false);
                    _dealingDamage = false;
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

    public void StartAttack(bool value)
    {
        _dealingDamage = value;
    }
}