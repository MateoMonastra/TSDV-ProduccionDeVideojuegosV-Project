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
        private readonly PlayerAnimationController _anim;

        private float _t;
        private bool  _damageTicked;

        public System.Action<float> OnSpinCooldownUI;

        public SpinRelease(MyKinematicMotor m, PlayerModel model, System.Action<string> request, PlayerAnimationController anim = null)
        { _m = m; _model = model; _req = request; _anim = anim; }

        public override void Enter()
        {
            base.Enter();
            
            _model.locomotionBlocked = true;
            _model.actionMoveSpeedMultiplier = 0f;
            _model.invulnerableToEnemies = true;
            _model.aimLockActive = false;
            
            _model.SpinOnCooldown   = true;
            _model.SpinCooldownLeft = _model.SpinCooldown;
            OnSpinCooldownUI?.Invoke(_model.SpinCooldownLeft);

            _t = 0f;
            _damageTicked = false;

            _anim?.SetCombatActive(true);
            _anim?.TriggerSpinRelease();
            if (_anim != null) _anim.OnAnim_SpinDamage += OnSpinDamageEvent;
        }

        public override void Exit()
        {
            base.Exit();
            if (_anim != null) _anim.OnAnim_SpinDamage -= OnSpinDamageEvent;

            _model.ClearActionLocks();
            _anim?.SetCombatActive(false);
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            _t += dt;
            
            if (!_damageTicked && _t >= _model.SpinDuration * 0.4f)
            {
                DoSpinDamage();
                _damageTicked = true;
            }
            
            if (_t >= _model.SpinDuration + _model.SpinPostStun)
            {
                _req?.Invoke(ToIdle);
                Finish();
            }
        }

        private void OnSpinDamageEvent()
        {
            DoSpinDamage();
        }

        private void DoSpinDamage()
        {
            Vector3 center = _m.transform.position;
            Collider[] hits = Physics.OverlapSphere(center, _model.SpinRadius, _model.enemyMask, QueryTriggerInteraction.Ignore);

            foreach (var c in hits)
            {
                var d = c.GetComponentInParent<IDamageable>();
                if (d == null) continue;

                Vector3 dir = (c.bounds.center - center); dir.y = 0f;
                if (dir.sqrMagnitude > 1e-6f) dir.Normalize();

                d.TakeDamage(_model.SpinDamage);
                d.ApplyKnockback(dir, _model.SpinPushDistance);
                d.ApplyStagger(_model.SpinStaggerTime);
            }
        }
    }
}
