using System;
using UnityEngine;

namespace Health
{
    public class HealthController : MonoBehaviour
    {
        [SerializeField] private int maxHealth;
        private int _currentHealth;

        public Action OnHeal;
        public Action OnTakeDamage;
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

        public void Damage(int damage)
        {
            _currentHealth -= damage;

            if (_currentHealth <= 0)
                OnTakeDamage?.Invoke();
            else
                OnDeath?.Invoke();
        }
    }
}