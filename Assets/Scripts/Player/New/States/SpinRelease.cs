using FSM;
using UnityEngine;

namespace Player.New
{
    public class SpinRelease : FinishableState
    {
        public const string ToIdle = "SpinRelease->AttackIdle";

        private readonly MyKinematicMotor _m;
        private readonly PlayerModel _model;
        private readonly System.Action<string> _req;

        private float _t;

        public System.Action<float> OnSpinCooldownUI;

        private readonly PlayerAnimationController _anim;
        public SpinRelease(MyKinematicMotor m, PlayerModel model, System.Action<string> request, PlayerAnimationController anim = null)
        { _m = m; _model = model; _req = request; _anim = anim; }

        public override void Enter()
        {
            base.Enter();
            _t = 0f;
            _model.locomotionBlocked = true;
            _model.spinOnCooldown = true;
            _model.spinCooldownLeft = _model.spinCooldown;

            _anim?.SetCombatActive(true);
            _anim?.TriggerSpinRelease();
            if (_anim != null) _anim.OnAnim_SpinDamage += OnSpinDamageEvent;
        }

        public override void Exit()
        {
            base.Exit();
            _anim?.SetCombatActive(false);
            if (_anim != null) _anim.OnAnim_SpinDamage -= OnSpinDamageEvent;
            _model.ClearActionLocks();
        }


        public override void Tick(float dt)
        {
            base.Tick(dt);
            _t += dt;

            // Fin de anim + post-stun
            if (_t >= _model.spinDuration + _model.spinPostStun)
            {
                _req?.Invoke(ToIdle);
                Finish();
            }
        }

        private void OnSpinDamageEvent()
        {
            // Daño 360° + push radial
            Vector3 center = _m.transform.position;
            Collider[] hits = Physics.OverlapSphere(center, _model.spinRadius, _model.enemyMask, QueryTriggerInteraction.Ignore);
            foreach (var c in hits)
            {
                var d = c.GetComponentInParent<IDamageable>();
                if (d == null) continue;
                Vector3 dir = (c.bounds.center - center); dir.y = 0f;
                if (dir.sqrMagnitude > 1e-6f) dir.Normalize();
                d.TakeDamage(_model.spinDamage);
                d.ApplyKnockback(dir, _model.spinPushDistance);
                d.ApplyStagger(_model.spinStaggerTime);
            }
        }
    }
}
