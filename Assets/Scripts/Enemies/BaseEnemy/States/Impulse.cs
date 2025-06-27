using UnityEngine;

namespace Enemies.BaseEnemy.States
{
    public class Impulse : BaseEnemyState
    {
        public Impulse(Transform enemy, Transform player, BaseEnemyModel model) : base(enemy, player, model)
        {
        }
    }
}