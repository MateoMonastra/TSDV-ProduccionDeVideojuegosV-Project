using System;
using System.Collections.Generic;
using Coins;
using FSM;
using Health;
using UnityEngine;


namespace Player.New
{
    /// <summary>
    /// Base para ataques del combo: lleva el tiempo, ejecuta el hit frontal
    /// y da soporte de buffer para encadenar el siguiente golpe.
    /// </summary>
    public abstract class AttackBase : FinishableState
    {
        protected readonly MyKinematicMotor M;
        protected readonly PlayerModel Model;
        protected readonly Action<string> Req;
        private readonly MyCharacterCamera _characterCamera;
        
        protected float Duration;
        protected float t;
        
        private readonly HashSet<HealthController> _enemiesHit = new HashSet<HealthController>();
        private readonly HashSet<BreakableLoot> _breakablesHit = new HashSet<BreakableLoot>();
        
        protected bool ChainBuffered;

        protected Vector2 knockbackDistance;
        protected float stunDuration;

        protected AttackBase(MyKinematicMotor m, PlayerModel mdl, MyCharacterCamera characterCamera, Action<string> req)
        {
            M = m;
            Model = mdl;
            _characterCamera = characterCamera;
            Req = req;
        }

        public override void Enter()
        {
            base.Enter();
            t = 0f;
            ChainBuffered = false;
            _enemiesHit.Clear();

            Model.DashBlocked = true;
            Model.JumpBlocked = true;
            _breakablesHit.Clear();
        }

        public override void Exit()
        {
            base.Exit();
            ChainBuffered = false;
            _enemiesHit.Clear();

            Model.DashBlocked = false;
            _breakablesHit.Clear();
        }

        /// <summary>Marca que el jugador pidió encadenar el siguiente golpe.</summary>
        protected void BufferChain() => ChainBuffered = true;

        protected void TryDoHitFrontal(float normalizedTime)
        {
            float halfAngle = (Model != null) ? Model.AttackHalfAngleDegrees : 45f;
            TryDoHitFrontal(normalizedTime, halfAngle);
        }

        protected void TryDoHitFrontal(float normalizedTime, float halfAngleDeg)
        {
            Vector3 origin = M.transform.position;
            Vector3 up = M.CharacterUp;
            Vector3 forward = Vector3.ProjectOnPlane(M.transform.forward, up).normalized;
            float range = Model.AttackRange;
            int mask = Model.EnemyMask.value;

            Collider[] cols = Physics.OverlapSphere(origin, range, mask, QueryTriggerInteraction.Collide);

            List<HealthController> enemiesToHit = new List<HealthController>();

            foreach (Collider col in cols)
            {
                if (col.TryGetComponent(out HealthController health))
                {
                    enemiesToHit.Add(health);
                }
                
                if (col.TryGetComponent(out BreakableLoot breakable))
                {
                    if (!_breakablesHit.Contains(breakable))
                    {
                        _breakablesHit.Add(breakable);
                        breakable.Hit();
                    }
                }
                
            }

            foreach (HealthController health in enemiesToHit)
            {
                if (!_enemiesHit.Contains(health))
                {
                    _enemiesHit.Add(health);
                    health.Damage(new DamageInfo(Model.AttackDamage, origin, knockbackDistance, "PlayerBaseAttack",
                        stunDuration));

                    _characterCamera.TriggerCameraShake(0.15f, 3f, 0.35f);
                }
            }
        }
    }
}