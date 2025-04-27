using FSM;
using UnityEngine;

namespace Enemy
{
    public abstract class BaseState : State
    {
        protected Transform enemy;
        protected Transform player;
        protected EnemyModel model;
        protected BaseState(Transform enemy, Transform player, EnemyModel model)
        {
            this.enemy = enemy;
            this.player = player;
            this.model = model;
        }
    }
}