using Health;
using Platforms;
using System.Collections.Generic;
using UnityEngine;

public class KillEnemy : MonoBehaviour
{
    [SerializeField] private HammerController hammerController;
    [SerializeField] private int damage;
    [SerializeField] private (int, int) knockback = (20,35);
    private bool _dealingDamage = false;
    private void OnTriggerEnter(Collider other)
    {
        if (_dealingDamage)
        {

            if (other.CompareTag("Enemy"))
            {
                if (other.gameObject.TryGetComponent(out HealthController enemy))
                {
                    enemy.Damage(new DamageInfo(damage,transform.position,knockback));
                    hammerController.ToggleAttackCollider(false);
                    _dealingDamage = false;
                }
            }

            //TODO: YA HAY QUE SACAR ESTE BODRIO
            if (other.gameObject.TryGetComponent(out IBreakable breakable))
            {

                if (hammerController.IsGroundSlamming)
                {
                    breakable.Break();
                }
            }
        }
    }

    public void StartAttack(bool value)
    {
        _dealingDamage = value;
    }
}