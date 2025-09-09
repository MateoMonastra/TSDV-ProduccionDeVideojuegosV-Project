using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Health
{
    public class HealthController : MonoBehaviour
    {
        [SerializeField] private int maxHealth;
        [SerializeField] private float damageCooldown;
        private int _currentHealth;
        private float _timer;

        public Action OnHeal;
        public Action<DamageInfo> OnTakeDamage;
        public Action OnDeath;

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
            if (_timer <= damageCooldown) return;
            
            _currentHealth -= damageInfo.Damage;

            if (_currentHealth > 0)
                OnTakeDamage?.Invoke(damageInfo);
            else
                OnDeath?.Invoke();

            _timer = 0;
        }

        public void InstaKill()
        {
            OnDeath?.Invoke();
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

        private void Update()
        {
            _timer += Time.deltaTime;
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