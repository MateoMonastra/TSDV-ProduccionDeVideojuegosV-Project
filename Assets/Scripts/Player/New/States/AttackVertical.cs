using FSM;
using UnityEngine;

namespace Player.New
{
    /// <summary>
    /// Ataque vertical aéreo (slam). Solo si el salto fue “puro vertical”.
    /// Acelera hacia el piso y al impactar hace daño en área, knockback y stagger.
    /// </summary>
    public class AttackVertical : FinishableState
    {
        public const string ToIdle = "ToIdle";

        private readonly MyKinematicMotor _m;
        private readonly PlayerModel _model;
        private readonly System.Action<string> _req;
        private readonly PlayerAnimationController _anim;

        private float _t;
        private bool  _impactDone;

        // Fallback de impacto por proximidad al suelo
        private const float ImpactProximity = 0.20f; 
        private const float MaxAirTime      = 3.0f;

        public AttackVertical(MyKinematicMotor m, PlayerModel mdl, System.Action<string> req, PlayerAnimationController anim = null)
        { _m = m; _model = mdl; _req = req; _anim = anim; }

        /// <summary>Puede usarse si está en aire, no hay cooldown y el salto fue vertical.</summary>
        public static bool CanUse(MyKinematicMotor m, PlayerModel model)
            => !m.IsGrounded && !model.VerticalOnCooldown;

        public override void Enter()
        {
            base.Enter();
            _t = 0f; _impactDone = false;

            // Bloquea locomoción durante el slam
            _model.LocomotionBlocked = true;
            _model.AimLockActive = false;

            // Impulso inicial hacia abajo opcional
            var v = _m.Velocity;
            v.y = Mathf.Min(v.y, -_model.VerticalSlamStartDownSpeed);
            _m.SetVelocity(v);

            _anim?.SetCombatActive(true);
            _anim?.TriggerVerticalStart();
        }

        public override void Exit()
        {
            base.Exit();
            _model.ClearActionLocks();
            _anim?.SetCombatActive(false);
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            _t += dt;

            // Acelerar hacia abajo con límite de velocidad
            var v = _m.Velocity;
            v.y = Mathf.Max(v.y - _model.VerticalSlamExtraAccel * dt, -_model.VerticalSlamMaxDownSpeed);
            _m.SetVelocity(v);

            // Impacto por proximidad a suelo (fallback si no hay evento de anim)
            if (!_impactDone && _m.IsGrounded && _t > 0.05f)
            {
                DoImpact();
            }
            else if (!_impactDone)
            {
                // raycast corto hacia abajo para anticipar
                if (Physics.Raycast(_m.transform.position, Vector3.down, out var hit, ImpactProximity, ~0, QueryTriggerInteraction.Ignore))
                    DoImpact();
            }

            // Seguridad por si nunca colisiona
            if (_t >= MaxAirTime && !_impactDone)
                DoImpact();
        }

        private void DoImpact()
        {
            _impactDone = true;

            // Daño radial
            Collider[] hits = Physics.OverlapSphere(_m.transform.position, _model.VerticalAttackRadius, _model.EnemyMask, QueryTriggerInteraction.Ignore);
            foreach (var c in hits)
            {
                var dmg = c.GetComponentInParent<Player.IDamageable>();
                if (dmg == null) continue;

                Vector3 dir = (c.bounds.center - _m.transform.position); dir.y = 0f;
                if (dir.sqrMagnitude > 1e-6f) dir.Normalize();

                dmg.TakeDamage(_model.VerticalDamage);
                dmg.ApplyKnockback(dir, _model.VerticalKnockbackDistance);
                dmg.ApplyStagger(_model.VerticalStaggerTime);
            }

            // Cooldown + post-stun breve
            _model.VerticalOnCooldown = true;
            _model.VerticalCooldownLeft = _model.VerticalAttackCooldown;

            _anim?.TriggerVerticalImpact();

            // Quitar bloqueo de locomoción tras el “post-stun” del vertical
            // (si querés inmovilizar: usar una bandera/tiempo adicional)
            _model.LocomotionBlocked = false;

            _req?.Invoke(ToIdle);
            Finish();
        }
    }
}
