using FSM;
using UnityEngine;

namespace Enemy
{
    public class Death : State
    {
        private GameObject enemy;
        public Death(GameObject enemy)
        {
            this.enemy = enemy;
        }
        public override void Enter()
        {
            base.Enter();
            enemy.SetActive(false);
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
