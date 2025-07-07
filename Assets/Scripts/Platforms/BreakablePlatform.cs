using PickUps;
using UnityEngine;
using UnityEngine.Events;

namespace Platforms
{
    public class BreakablePlatform : Pickup, IBreakable
    {
        [SerializeField] private UnityEvent OnBreak;

        public void Break()
        {
            RefreshCooldown();
        }
    }
}