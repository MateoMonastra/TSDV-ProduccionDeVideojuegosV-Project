using FSM;
using UnityEngine;

namespace Enemies
{
    public abstract class BaseState : State
    {
        protected Transform enemy;
        protected Transform player;
        protected BaseEnemyModel model;
        protected BaseState(Transform enemy, Transform player, BaseEnemyModel model)
        {
            this.enemy = enemy;
            this.player = player;
            this.model = model;
        }
    }
}