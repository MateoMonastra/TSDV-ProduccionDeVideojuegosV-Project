using UnityEngine;

namespace Enemies.BaseEnemy.States
{
    /// <summary>
    /// Estado de daño/knockback con control de gravedad y ground-check.
    /// </summary>
    public sealed class Impulse : BaseEnemyState
    {
        private readonly System.Action _onEnd;
        private readonly float _rayMaxDistance = 0.6f;

        public Impulse(EnemyContext ctx, System.Action onEnd) : base(ctx)
        {
            _onEnd = onEnd;
        }

        public override void Enter()
        {
            Ctx.Anims?.SetDamagedAnimation(true);

            if (Ctx.Agent) Ctx.Agent.enabled = false;
            if (Ctx.Rb)
            {
                Ctx.Rb.isKinematic = false;

                // Empuje desde el jugador (puede ajustarse según tu juego)
                Vector3 dir = (Ctx.Self.position - Ctx.Target.position);
                dir.y = 0f;
                dir = dir.sqrMagnitude > 0.0001f ? dir.normalized : Ctx.Self.forward;

                Vector3 impulse = dir * Ctx.Model.HorizontalImpulseForce + Vector3.up * Ctx.Model.VerticalImpulseForce;
                Ctx.Rb.AddForce(impulse, ForceMode.Impulse);
            }
        }

        public override void Tick(float dt)
        {
            if (!Ctx.Rb) return;
            
            var v = Ctx.Rb.linearVelocity;

            if (v.y < 0)
                v += Vector3.up * (Physics.gravity.y * (Ctx.Model.FallMultiplier - 1f) * dt);
            else if (v.y > 0)
                v += Vector3.up * (Physics.gravity.y * (Ctx.Model.LowJumpMultiplier - 1f) * dt);

            Ctx.Rb.linearVelocity = v;
            
            bool grounded = Physics.Raycast(
                Ctx.Self.position + Vector3.up * 0.5f,
                Vector3.down,
                _rayMaxDistance,
                Ctx.Model.GroundLayer);

            if (grounded)
                _onEnd?.Invoke();
        }

        public override void Exit()
        {
            if (Ctx.Agent) { Ctx.Agent.enabled = true; Ctx.Agent.ResetPath(); }
            if (Ctx.Rb)    { Ctx.Rb.isKinematic = true; }
            Ctx.Anims?.SetDamagedAnimation(false);
        }
    }
}
