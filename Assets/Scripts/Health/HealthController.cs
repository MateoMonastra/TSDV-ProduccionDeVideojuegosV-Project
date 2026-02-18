using System;
using UnityEngine;

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
            else if (_currentHealth == 0)
                OnDeath?.Invoke(damageInfo);

        }

        public void InstaKill()
        {
            DamageInfo instakillDamage = new DamageInfo(999999, Vector3.zero, Vector3.up, "InstaKill");
            OnDeath?.Invoke(instakillDamage);
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
        public Vector2 Knockback;
        public string DamageName;
        public float StunDuration;

        public DamageInfo(int damage, Vector3 transformPosition, Vector2 knockback, string damageName, float stunDuration = 0.5f)
        {
            Damage = damage;
            DamageOrigin = transformPosition;
            Knockback = knockback;
            DamageName = damageName;
            StunDuration = stunDuration;
        }
    }
}