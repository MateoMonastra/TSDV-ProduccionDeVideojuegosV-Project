using FSM;
using UnityEngine;

namespace Enemies.BaseEnemy.States
{
    public class Death : State
    {
        private GameObject _enemy;
        public Death(GameObject enemy)
        {
            this._enemy = enemy;
        }
        public override void Enter()
        {
            base.Enter();
            Debug.Log("muelto confirmed confirmed");
            _enemy.SetActive(false);
        }

        public override void Tick(float delta)
        {
            base.Tick(delta);
        }

        public override void FixedTick(float delta)
        {
            base.FixedTick(delta);
        }

        public override void Exit()
        {
            base.Exit();
        }
    }
}
