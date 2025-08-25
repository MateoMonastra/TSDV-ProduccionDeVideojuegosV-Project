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
        private readonly PlayerAnimationController _anim;

        private float _t;
        private bool _impactDone;

        private const float ImpactProximity = 0.20f; 
        private const float MaxAirTime = 3.0f;

        public AttackVertical(MyKinematicMotor m, PlayerModel mdl, System.Action<string> req, PlayerAnimationController anim = null)
        { _m = m; _model = mdl; _req = req; _anim = anim; }

        public override void Enter()
        {
            base.Enter();
            
            if (_m.IsGrounded || !_model.jumpWasPureVertical || _model.verticalOnCooldown)
            { _req?.Invoke(ToIdle); Finish(); return; }

            _t = 0f; _impactDone = false;
            
            _model.locomotionBlocked = true;
            _model.actionMoveSpeedMultiplier = 0f;
            _model.aimLockActive = false;
            
            var v = _m.Velocity;
            v.x = 0f; v.z = 0f;
            if (_model.verticalSlamStartDownSpeed > 0f && v.y > -_model.verticalSlamStartDownSpeed)
                v.y = -_model.verticalSlamStartDownSpeed;
            _m.SetVelocity(v);

            _anim?.SetCombatActive(true);
            _anim?.TriggerVerticalStart();
            if (_anim != null) _anim.OnAnim_VerticalImpact += OnAnimVerticalImpact;
        }

        public override void Exit()
        {
            base.Exit();
            
            _model.verticalOnCooldown   = true;
            _model.verticalCooldownLeft = _model.verticalAttackCooldown;

            _model.ClearActionLocks();
            _anim?.SetCombatActive(false);
            if (_anim != null) _anim.OnAnim_VerticalImpact -= OnAnimVerticalImpact;
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            
            var v = _m.Velocity;
            
            v.x = 0f; v.z = 0f;
            
            float newVy = v.y - _model.verticalSlamExtraAccel * dt;
            
            newVy = Mathf.Max(newVy, -_model.verticalSlamMaxDownSpeed);
            
            v.y = newVy;

            _m.SetVelocity(v);
            
            if (!_impactDone && v.y <= 0f && IsGroundClose(_m.transform.position, GetUp(), ImpactProximity))
            {
                DoVerticalImpact();
            }
            
            _t += dt;
            if (!_impactDone && _t >= MaxAirTime)
            {
                DoVerticalImpact();
            }
            
            if (_impactDone && _t >= _model.verticalAttackPostStun)
            {
                _req?.Invoke(ToIdle);
                Finish();
            }
        }

        private void OnAnimVerticalImpact() => DoVerticalImpact();

        private void DoVerticalImpact()
        {
            if (_impactDone) return;
            _impactDone = true;
            _t = 0f; 

            _anim?.TriggerVerticalImpact();
            
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

        private Vector3 GetUp() => _m != null ? _m.CharacterUp : Vector3.up;

        private bool IsGroundClose(Vector3 origin, Vector3 up, float maxDist)
        {
            var ray = new Ray(origin + up * 0.05f, -up);
            return Physics.Raycast(ray, maxDist, ~0, QueryTriggerInteraction.Ignore);
        }

        public static bool CanUse(MyKinematicMotor m, PlayerModel mdl)
            => !m.IsGrounded && !mdl.verticalOnCooldown;
    }
}
