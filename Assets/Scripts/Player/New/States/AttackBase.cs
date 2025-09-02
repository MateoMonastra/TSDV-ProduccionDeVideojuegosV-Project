using FSM;
using UnityEngine;

namespace Player.New
{
    /// <summary>
    /// Base para ataques básicos (A1/A2/A3). Maneja el “tick” de duración y utilidades
    /// para aplicar daño frontal a un único objetivo.
    /// </summary>
    public abstract class AttackBase : FinishableState
    {
        protected readonly MyKinematicMotor M;
        protected readonly PlayerModel Model;
        protected readonly System.Action<string> Req;

        protected float Duration; // duración del ataque actual
        protected float t;        // acumulador de tiempo
        private   bool _didHit;

        protected AttackBase(MyKinematicMotor m, PlayerModel mdl, System.Action<string> req)
        { M = m; Model = mdl; Req = req; }

        public override void Exit()
        {
            base.Exit();
            _didHit = false;
        }

        /// <summary> Ejecuta el golpe frontal (único objetivo) una sola vez por estado. </summary>
        protected void TryDoHitFrontal(float hitTimeNormalized = 0.5f, float halfAngleDeg = 55f)
        {
            if (_didHit) return;
            if (t < Duration * hitTimeNormalized) return;
            _didHit = true;

            // Origen y forward del atacante (plano XZ)
            Vector3 origin = M.transform.position;
            Vector3 forward = M.transform.forward;

            if (HitboxUtils.TryGetClosestInFront(origin, forward, Model.AttackRange, halfAngleDeg,
                    Model.EnemyMask, out var target, out var targetTf))
            {
                // Dirección horizontal hacia el objetivo para el empuje
                Vector3 dir = (targetTf.position - origin); dir.y = 0f;
                dir = dir.sqrMagnitude > 1e-6f ? dir.normalized : forward;

                target.TakeDamage(Model.AttackDamage);
                target.ApplyKnockback(dir, Model.AttackKnockbackDistance);
                target.ApplyStagger(Model.AttackStaggerTime);
            }
            // TODO: SFX/VFX, hitstop…
        }
    }
}
