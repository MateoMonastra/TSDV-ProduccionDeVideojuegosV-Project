using FSM;
using UnityEngine;

namespace Player.New
{
    public class AttackVertical : FinishableState
    {
        public const string ToIdle = "AttackVertical->AttackIdle";

        private readonly MyKinematicMotor _m;
        private readonly PlayerModel _model;
        private readonly System.Action<string> _req;

        private float _t;
        private bool _impactDone;
        private readonly PlayerAnimationController _anim;

        public AttackVertical(MyKinematicMotor m, PlayerModel mdl, System.Action<string> req, PlayerAnimationController anim = null)
        { _m = m; _model = mdl; _req = req; _anim = anim; }

        public override void Enter()
        {
            base.Enter();

            if (_m.IsGrounded || !_model.jumpWasPureVertical || _model.verticalOnCooldown)
            { _req?.Invoke(ToIdle); Finish(); return; }

            _t = 0f; _impactDone = false;
            _model.locomotionBlocked = true;

            _anim?.TriggerVerticalStart();
            if (_anim != null) _anim.OnAnim_VerticalImpact += OnAnimVerticalImpact;
        }

        public override void Exit()
        {
            base.Exit();
            _model.locomotionBlocked = false;
            _model.verticalOnCooldown = true;
            _model.verticalCooldownLeft = _model.verticalAttackCooldown;
            if (_anim != null) _anim.OnAnim_VerticalImpact -= OnAnimVerticalImpact;
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            _t += dt;

            // Evitar desplazamiento horizontal mientras cae/impacta
            var v = _m.Velocity;
            v.x = 0f; v.z = 0f;
            _m.SetVelocity(v);

            // Fallback: si no usás Animation Event, detectá impacto por grounded
            if (!_impactDone && _m.IsGrounded)
            {
                DoVerticalImpact();
            }

            // Post-stun
            if (_impactDone)
            {
                if (_t >= _model.verticalAttackPostStun)
                {
                    _req?.Invoke(ToIdle);
                    Finish();
                }
            }

            // CD
            if (_model.verticalOnCooldown)
            {
                _model.verticalCooldownLeft = Mathf.Max(0f, _model.verticalCooldownLeft - dt);
                if (_model.verticalCooldownLeft <= 0f) _model.verticalOnCooldown = false;
            }
        }

        private void OnAnimVerticalImpact() => DoVerticalImpact();

        private void DoVerticalImpact()
        {
            if (_impactDone) return;
            _impactDone = true;
            _t = 0f;

            // Daño en área
            Vector3 center = _m.transform.position;
            Collider[] hits = Physics.OverlapSphere(center, _model.verticalAttackRadius, _model.enemyMask, QueryTriggerInteraction.Ignore);
            foreach (var c in hits)
            {
                var d = c.GetComponentInParent<IDamageable>();
                if (d == null) continue;
                Vector3 dir = (c.bounds.center - center); dir.y = 0f;
                if (dir.sqrMagnitude > 1e-6f) dir.Normalize();
                d.TakeDamage(_model.verticalDamage);
                d.ApplyKnockback(dir, _model.verticalKnockbackDistance);
                d.ApplyStagger(_model.verticalStaggerTime);
            }
        }

        public static bool CanUse(MyKinematicMotor m, PlayerModel mdl)
            => !m.IsGrounded && mdl.jumpWasPureVertical && !mdl.verticalOnCooldown;
    }
}
