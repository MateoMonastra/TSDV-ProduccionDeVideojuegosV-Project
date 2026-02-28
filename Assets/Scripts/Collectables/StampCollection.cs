using System;
using System.Collections.Generic;
using UnityEngine;

namespace Collectables
{
    public class StampCollection : MonoBehaviour
    {
        public static StampCollection Instance { get; private set; }
        
        private readonly HashSet<string> _unlocked = new HashSet<string>();

        public event Action<string> OnStampUnlocked;

        private void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
        }

        public bool IsUnlocked(string stampId)
        {
            if (_unlocked.Contains(stampId)) return true;
            
            return false;
        }

        public bool Unlock(string stampId)
        {
            if (IsUnlocked(stampId)) return false;

            _unlocked.Add(stampId);

            OnStampUnlocked?.Invoke(stampId);
            return true;
        }

        public void ResetAllStamps()
        {
            _unlocked.Clear();
        }
    }
}