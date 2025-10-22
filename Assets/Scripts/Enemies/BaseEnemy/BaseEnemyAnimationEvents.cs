using UnityEngine;
using UnityEngine.Serialization;

namespace Enemies.BaseEnemy
{
    public class BaseEnemyAnimationEvents : MonoBehaviour
    {
        [SerializeField] private EnemyAnimationController anim;
        
        public void OnAttackDamage()
        {
            anim.AnimEvent_AttackDamage();
        }
    }
}