using PickUps;
using UnityEngine;

namespace Platforms
{
    public class BreakablePlatform : Pickup, IBreakable
    {
        public void Break()
        {
            RefreshCooldown();
        }
    }
}
