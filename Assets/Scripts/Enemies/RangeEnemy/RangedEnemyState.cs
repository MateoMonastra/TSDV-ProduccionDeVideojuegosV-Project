using FSM;
using UnityEngine;

namespace Enemies.RangeEnemy
{
    public abstract class RangedEnemyState : State
    {
        protected Transform enemy;
        protected Transform player;
        protected RangedEnemyModel model;

        protected RangedEnemyState(Transform enemy, Transform player, RangedEnemyModel model)
        {
            this.enemy = enemy;
            this.player = player;
            this.model = model;
        }
    }
}