using UnityEngine;

namespace Player
{
    public interface IDamageable
    {
        void TakeDamage(float amount);
        void ApplyKnockback(Vector3 direction, float distance);
        void ApplyStagger(float seconds);
    }
}