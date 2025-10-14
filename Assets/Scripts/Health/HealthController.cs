using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace Health
{
     [Serializable]
    public struct Knockback
    {
        [Min(0)] public float horizontal;
        [Min(0)] public float vertical;
    }
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
        public Knockback Knockback;

        public DamageInfo(int damage, Vector3 transformPosition, (float, float) knockback)
        {
            Damage = damage;
            DamageOrigin = transformPosition;
            Knockback.horizontal = knockback.Item1;
            Knockback.vertical = knockback.Item2;
        }
        
        public DamageInfo(int damage, Vector3 transformPosition, Knockback knockback)
        {
            Damage = damage;
            DamageOrigin = transformPosition;
            Knockback = knockback;
        }
    }
    
}