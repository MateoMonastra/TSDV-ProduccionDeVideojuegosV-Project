using FSM;
using UnityEngine;

namespace Enemies.BaseEnemy.States
{
    public class Death : State
    {
        private GameObject _enemy;
        private BaseEnemyModel _model;

        private float _deathTimer;
        public Death(GameObject enemy, BaseEnemyModel model)
        {
            this._enemy = enemy;
            this._model = model;
        }
        public override void Enter()
        {
            base.Enter();
        }

        public override void Tick(float delta)
        {
            base.Tick(delta);
            if (_deathTimer < _model.DeathTime)
            {
                _deathTimer += delta;
            }
            else
            {
                _enemy.SetActive(false);
            }
            
            
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
