using FSM;
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
        protected readonly System.Action<string> Req;

        /// <summary>Duración total del ataque actual.</summary>
        protected float Duration;

        /// <summary>Acumulador de tiempo del ataque.</summary>
        protected float t;

        /// <summary>Señal de que ya aplicamos el hit (evita multi-hit).</summary>
        private bool _didHit;

        /// <summary>Si el jugador pidió encadenar (presionó Attack) en cualquier momento.</summary>
        protected bool ChainBuffered;

        protected AttackBase(MyKinematicMotor m, PlayerModel mdl, System.Action<string> req)
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

        /// <summary> Ejecuta el golpe frontal (único objetivo) una sola vez por estado. </summary>
        protected void TryDoHitFrontal(float hitTimeNormalized = 0.5f, float halfAngleDeg = 55f)
        {
            if (_didHit) return;
            if (t < Duration * hitTimeNormalized) return;
            _didHit = true;

            Vector3 origin = M.transform.position;
            Vector3 forward = M.transform.forward;

            if (HitboxUtils.TryGetClosestInFront(origin, forward, Model.AttackRange, halfAngleDeg,
                    Model.EnemyMask, out var target, out var targetTf))
            {
                Vector3 dir = (targetTf.position - origin); dir.y = 0f;
                dir = dir.sqrMagnitude > 1e-6f ? dir.normalized : forward;

                target.TakeDamage(Model.AttackDamage);
                target.ApplyKnockback(dir, Model.AttackKnockbackDistance);
                target.ApplyStagger(Model.AttackStaggerTime);
            }
        }
    }
}
