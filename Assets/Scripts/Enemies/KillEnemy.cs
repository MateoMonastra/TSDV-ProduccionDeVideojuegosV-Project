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
    private bool _multipleHits = false;

    private List<Collider> hitColliders = new List<Collider>();

    private void OnTriggerEnter(Collider other)
    {
        //run through list to not hit same collider twice
        foreach (Collider collider in hitColliders)
        {
            if (collider == other)
                return;
        }

        if (_dealingDamage)
        {

            if (other.CompareTag("Enemy"))
            {
                if (other.gameObject.TryGetComponent(out HealthController enemy))
                {
                    hitColliders.Add(other);
                    enemy.Damage(new DamageInfo(damage,transform.position,knockback));

                    if (!_multipleHits)
                    {
                        hammerController.ToggleAttackCollider(false);
                        _dealingDamage = false;
                    }
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
            else if (other.gameObject.TryGetComponent(out IInteractable interactable))
            {
                interactable.Interact(true);
            }
        }
    }

    public void StartAttack(bool value)
    {
        _dealingDamage = value;
        hitColliders.Clear();
    }

    public void ToggleMultipleHits(bool value)
    {
        _multipleHits = value;
        hitColliders.Clear();
    }
}