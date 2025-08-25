using Health;
using UnityEngine;

namespace Enemies
{
    public interface IEnemy
    {
        public void OnBeingAttacked(DamageInfo damageOrigin);
    }
}