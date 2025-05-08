using FSM;
using UnityEngine;

namespace Enemies.Enemy
{
    public abstract class BaseEnemyState : State
    {
        protected Transform enemy;
        protected Transform player;
        protected BaseEnemyModel model;
        protected BaseEnemyState(Transform enemy, Transform player, BaseEnemyModel model)
        {
            this.enemy = enemy;
            this.player = player;
            this.model = model;
        }
    }
}