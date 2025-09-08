using System;
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

        /// <summary>Señal de que ya aplicamos el hit (evita multi-hit).</summary>
        private bool _didHit;

        /// <summary>Si el jugador pidió encadenar (presionó Attack) en cualquier momento.</summary>
        protected bool ChainBuffered;

        protected AttackBase(MyKinematicMotor m, PlayerModel mdl, Action<string> req)
        { M = m; Model = mdl; Req = req; }

        public override void Enter()
        {
            base.Enter();
            t = 0f;
            _didHit = false;
            ChainBuffered = false;
        }

        public override void Exit()
        {
            base.Exit();
            _didHit = false;
            ChainBuffered = false;
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
                if (to.sqrMagnitude <= 1e-6f) continue;

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
            if (enemyHealth != null)
                enemyHealth.Damage(new DamageInfo(Model.AttackDamage, origin,(0,0)));
        }

    }
}
