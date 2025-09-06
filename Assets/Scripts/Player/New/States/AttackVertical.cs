using FSM;
using UnityEngine;
using Platforms; // IBreakable
using Player;    // IDamageable

namespace Player.New
{
    /// <summary>
    /// Ataque vertical aéreo (slam). Acelera hacia el piso y, al impactar,
    /// aplica daño/knockback/stagger a enemigos, rompe IBreakable y empuja rigidbodies.
    /// El impacto se sincroniza por Animation Event si existe; si no, hay fallbacks.
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
        private float _postTimer;

        // Fallbacks
        private const float ImpactProximity = 0.20f; 
        private const float MaxAirTime      = 3.0f;

        public AttackVertical(MyKinematicMotor m, PlayerModel mdl, System.Action<string> req, PlayerAnimationController anim = null)
        { _m = m; _model = mdl; _req = req; _anim = anim; }

        /// <summary>Puede usarse si está en aire, no hay cooldown.</summary>
        public static bool CanUse(MyKinematicMotor m, PlayerModel model)
            => !m.IsGrounded && !model.VerticalOnCooldown;

        public override void Enter()
        {
            base.Enter();
            _t = 0f; 
            _impactDone = false;
            _postTimer = -1f;

            // Bloquear locomoción durante la bajada
            _model.LocomotionBlocked = true;
            _model.AimLockActive = false;

            // Impulso inicial hacia abajo opcional
            var v = _m.Velocity;
            v.y = Mathf.Min(v.y, -_model.VerticalSlamStartDownSpeed);
            _m.SetVelocity(v);

            // Anim y evento
            _anim?.SetCombatActive(true);
            _anim?.TriggerVerticalStart();
            if (_anim != null) _anim.OnAnim_VerticalImpact += OnAnimVerticalImpact;
        }

        public override void Exit()
        {
            base.Exit();
            if (_anim != null) _anim.OnAnim_VerticalImpact -= OnAnimVerticalImpact;

            _model.ClearActionLocks();
            _anim?.SetCombatActive(false);
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            _t += dt;

            // Si ya impactó, solo contar "post-stun" y salir
            if (_impactDone)
            {
                if (_postTimer > 0f)
                {
                    _postTimer -= dt;
                    if (_postTimer <= 0f)
                    {
                        _model.LocomotionBlocked = false;
                        _req?.Invoke(ToIdle);
                        Finish();
                    }
                }
                return;
            }

            // Acelerar hacia abajo con límite de velocidad
            var v = _m.Velocity;
            v.y = Mathf.Max(v.y - _model.VerticalSlamExtraAccel * dt, -_model.VerticalSlamMaxDownSpeed);
            _m.SetVelocity(v);

            // Impacto por proximidad a suelo si no hay evento
            if (_m.IsGrounded && _t > 0.05f)
            {
                DoImpact();
            }
            else
            {
                // raycast corto hacia abajo para anticipar
                if (Physics.Raycast(_m.transform.position, Vector3.down, out var hit, ImpactProximity, ~0, QueryTriggerInteraction.Ignore))
                    DoImpact();
            }

            // Seguridad por si nunca colisiona
            if (_t >= MaxAirTime && !_impactDone)
                DoImpact();
        }

        private void OnAnimVerticalImpact() => DoImpact();

        private void DoImpact()
        {
            if (_impactDone) return;
            _impactDone = true;

            Vector3 center = _m.transform.position;

            // Objeto(s) a afectar: usamos máscara dedicada de vertical
            // Permitimos triggers (Interactuables) y colliders normales.
            Collider[] hits = Physics.OverlapSphere(
                center, 
                _model.VerticalAttackRadius, 
                _model.VerticalHitMask, 
                QueryTriggerInteraction.Collide
            );

            // Procesar cada hit: Enemigos (IDamageable) > Rompibles (IBreakable) > Rigidbodies
            foreach (var c in hits)
            {
                // 1) Enemigos / dummies que implementan IDamageable
                var dmg = c.GetComponentInParent<IDamageable>();
                if (dmg != null)
                {
                    Vector3 dir = (c.bounds.center - center);
                    dir.y = 0f;
                    if (dir.sqrMagnitude > 1e-6f) dir.Normalize();

                    dmg.TakeDamage(_model.VerticalDamage);
                    dmg.ApplyKnockback(dir, _model.VerticalKnockbackDistance);
                    dmg.ApplyStagger(_model.VerticalStaggerTime);
                    continue;
                }

                // 2) Rompibles
                var br = c.GetComponentInParent<IBreakable>();
                if (br != null)
                {
                    br.Break();
                    continue;
                }

                // 3) Rigidbody(s) sueltos
                if (_model.VerticalAffectsRigidbodies)
                {
                    var rb = c.attachedRigidbody ?? c.GetComponentInParent<Rigidbody>();
                    if (rb != null && !rb.isKinematic)
                    {
                        Vector3 to = (c.bounds.center - center);
                        if (to.sqrMagnitude < 1e-6f) to = Vector3.up;

                        Vector3 horiz = to; horiz.y = 0f;
                        if (horiz.sqrMagnitude > 1e-6f) horiz.Normalize();

                        Vector3 pushDir = (horiz + Vector3.up * _model.VerticalRigidbodyUpFactor).normalized;
                        rb.AddForce(pushDir * _model.VerticalRigidbodyImpulse, ForceMode.VelocityChange);
                    }
                }
            }

            // Cooldown + anim de impacto
            _model.VerticalOnCooldown   = true;
            _model.VerticalCooldownLeft = _model.VerticalAttackCooldown;

            _anim?.TriggerVerticalImpact();

            // Mantener inmóvil durante el post-stun configurado
            _model.LocomotionBlocked = true;
            _postTimer = Mathf.Max(0f, _model.VerticalAttackPostStun);
        }
    }
}
