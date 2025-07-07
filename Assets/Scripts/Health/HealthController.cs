using System;
using UnityEngine;

namespace Health
{
    public class HealthController : MonoBehaviour
    {
        [SerializeField] private int maxHealth;
        private int _currentHealth;

        public Action OnHeal;
        public Action<DamageInfo> OnTakeDamage;
        public Action<DamageInfo> OnDeath;

        private void Awake()
        {
            _currentHealth = maxHealth;
        }

        public void Heal(int healing)
        {
            _currentHealth += healing;
            OnHeal?.Invoke();
        }

        public void Damage(DamageInfo damageInfo)
        {
            _currentHealth -= damageInfo.Damage;

            if (_currentHealth > 0)
                OnTakeDamage?.Invoke(damageInfo);
            else
                OnDeath?.Invoke(damageInfo);
        }

        public void InstaKill(DamageInfo damageInfo)
        {
            OnDeath?.Invoke(damageInfo);
        }

        public void ResetHealth()
        {
            _currentHealth = maxHealth;
            OnHeal?.Invoke();
        }

        public int GetCurrentHealth()
        {
            return _currentHealth;
        }
    }

    public struct DamageInfo
    {
        public int Damage;
        public Vector3 DamageOrigin;
        public (int, int) Knockback;

        public DamageInfo(int damage, Vector3 transformPosition, (int, int) knockback)
        {
            Damage = damage;
            DamageOrigin = transformPosition;
            Knockback = (knockback.Item1, knockback.Item2);
        }
    }
    
}