using System;
using System.Collections.Generic;
using UnityEngine;

namespace UI
{
    public enum PickupId { ExtraJump, DashBuff }

    [Serializable]
    public struct PickupBinding
    {
        public PickupId id;
        public PickupWidget widget;
    }

    /// <summary>Administra múltiples widgets: dispara Get/Use según el id.</summary>
    public class PickupsUIManager : MonoBehaviour
    {
        [SerializeField] private PickupBinding[] bindings;
        private readonly Dictionary<PickupId, PickupWidget> _map = new();

        private void Awake()
        {
            _map.Clear();
            foreach (var b in bindings)
            {
                if (b.widget && !_map.ContainsKey(b.id))
                    _map.Add(b.id, b.widget);
            }
        }

        public void OnPickupGet(PickupId id)
        {
            if (_map.TryGetValue(id, out var w) && w) w.PlayGet();
        }

        public void OnPickupUse(PickupId id)
        {
            if (_map.TryGetValue(id, out var w) && w) w.PlayUse();
        }
    }
}