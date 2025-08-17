using FSM;
using UnityEngine;

namespace Player.New
{
    public abstract class AttackBase : FinishableState
    {
        protected readonly MyKinematicMotor M;
        protected readonly PlayerModel Model;
        protected readonly System.Action<string> Req;

        protected float Duration;
        protected float t;
        private   bool _didHit;

        protected AttackBase(MyKinematicMotor m, PlayerModel mdl, System.Action<string> req)
        { M = m; Model = mdl; Req = req; }

        public override void Enter()
        {
            base.Enter();
            _didHit = false;
        }

        /// <summary> Ejecuta el golpe frontal (un objetivo) una sola vez por estado. </summary>
        protected void TryDoHitFrontal(float hitTimeNormalized = 0.5f, float halfAngleDeg = 55f)
        {
            if (_didHit) return;
            if (t < Duration * hitTimeNormalized) return;
            _didHit = true;

            // Origen y forward del atacante (plano XZ)
            Vector3 origin = M.transform.position;
            Vector3 forward = M.transform.forward;

            if (HitboxUtils.TryGetClosestInFront(origin, forward, Model.attackRange, halfAngleDeg,
                    Model.enemyMask, out var target, out var targetTf))
            {
                // Direccion horizontal hacia el objetivo para el empuje
                Vector3 dir = (targetTf.position - origin); dir.y = 0f;
                dir = dir.sqrMagnitude > 1e-6f ? dir.normalized : forward;

                target.TakeDamage(Model.attackDamage);
                target.ApplyKnockback(dir, Model.attackKnockbackDistance);
                target.ApplyStagger(Model.attackStaggerTime);
            }
            // TODO: SFX de impacto, VFX, hitstop…
        }
    }
}