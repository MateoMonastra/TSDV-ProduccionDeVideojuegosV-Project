using UnityEngine;

namespace Platforms
{
    public class BreakablePlatform : MonoBehaviour, IBreakable
    {
        public void Break()
        {
            gameObject.SetActive(false);
        }
    }
}
