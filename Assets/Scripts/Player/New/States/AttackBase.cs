using System;
using System.Collections.Generic;
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

        /// <summary>Duración total del ataque actual.</summary>
        protected float Duration;

        /// <summary>Acumulador de tiempo del ataque.</summary>
        protected float t;

        /// <summary>Enemigos ya golpeados durante este ataque.</summary>
        private readonly HashSet<HealthController> _enemiesHit = new HashSet<HealthController>();

        /// <summary>Si el jugador pidió encadenar (presionó Attack) en cualquier momento.</summary>
        protected bool ChainBuffered;

        protected Vector2 knockbackDistance;
        protected float stunDuration;

        protected AttackBase(MyKinematicMotor m, PlayerModel mdl, Action<string> req)
        { M = m; Model = mdl; Req = req; }

        public override void Enter()
        {
            base.Enter();
            t = 0f;
            ChainBuffered = false;
            _enemiesHit.Clear();

            Model.DashBlocked = true;
            Model.JumpBlocked = true;
        }

        public override void Exit()
        {
            base.Exit();
            ChainBuffered = false;
            _enemiesHit.Clear();

            Model.DashBlocked = false;
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
            Vector3 origin  = M.transform.position;
            Vector3 up      = M.CharacterUp;
            Vector3 forward = Vector3.ProjectOnPlane(M.transform.forward, up).normalized;
            float   range   = Model.AttackRange;
            int     mask    = Model.EnemyMask.value;

            var cols = Physics.OverlapSphere(origin, range, mask, QueryTriggerInteraction.Collide);

            float bestDot = -1f;
            Transform bestTf = null;

            for (int i = 0; i < cols.Length; i++)
            {
                var t = cols[i].transform;
                Vector3 to = Vector3.ProjectOnPlane(t.position - origin, up);
                if (to.sqrMagnitude <= Model.MinInputSqr) continue;

                float dist = to.magnitude;
                if (dist > range + 0.001f) continue;

                to /= dist;
                float ang = Vector3.Angle(forward, to);
                if (ang > halfAngleDeg) continue;

                float d = Vector3.Dot(forward, to);
                if (d > bestDot) { bestDot = d; bestTf = t; }
            }

            if (!bestTf) return;

            var enemyHealth = bestTf.GetComponentInParent<HealthController>();
            if (enemyHealth == null) return;

            // Evitar golpear dos veces al mismo enemigo en este ataque
            if (_enemiesHit.Contains(enemyHealth))
                return;

            _enemiesHit.Add(enemyHealth);
            enemyHealth.Damage(new DamageInfo(Model.AttackDamage, origin, knockbackDistance,"PlayerBaseAttack", stunDuration));
        }
    }
}
