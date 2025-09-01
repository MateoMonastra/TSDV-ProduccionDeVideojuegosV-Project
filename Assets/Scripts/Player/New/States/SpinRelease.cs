using FSM;
using UnityEngine;
using Player; // IDamageable

namespace Player.New
{
    public class SpinRelease : FinishableState
    {
        public const string ToIdle     = "SpinRelease->AttackIdle";
        public const string ToSelfStun = "SpinRelease->SelfStun";

        private readonly MyKinematicMotor _m;
        private readonly PlayerModel _model;
        private readonly System.Action<string> _req;
        private readonly PlayerAnimationController _anim;

        private float _t;
        private bool  _damageTicked;
        private bool  _nextIsSelfStun;

        private float _execDuration;   // duración efectiva del giro
        private float _postStun;       // post-stun (sigue saliendo del model)
        private float _damageMoment;   // momento del daño (si no hay evento)

        public System.Action<float> OnSpinCooldownUI;

        public SpinRelease(MyKinematicMotor m, PlayerModel model, System.Action<string> request, PlayerAnimationController anim = null)
        { _m = m; _model = model; _req = request; _anim = anim; }

        public override void Enter()
        {
            base.Enter();
            _t = 0f; _damageTicked = false; _nextIsSelfStun = false;

            // Permitir moverse/saltar durante el giro con multiplicadores
            _model.locomotionBlocked = false;
            _model.actionMoveSpeedMultiplier = Mathf.Max(0.01f, _model.spinMoveSpeedMultiplierWhileExecuting);
            _model.actionJumpSpeedMultiplier = Mathf.Max(0.01f, _model.spinJumpSpeedMultiplier);

            _model.invulnerableToEnemies = false;
            _model.aimLockActive = false;

            // Cooldown
            _model.SpinOnCooldown   = true;
            _model.SpinCooldownLeft = _model.SpinCooldown;
            OnSpinCooldownUI?.Invoke(_model.SpinCooldownLeft);

            // Duraciones por carga
            float r = Mathf.Clamp01(_model.spinChargeRatio);
            _execDuration = Mathf.Lerp(_model.spinMinDuration, _model.spinMaxDuration, r);
            _postStun     = _model.SpinPostStun;
            _model.selfStunDuration = Mathf.Lerp(_model.selfStunMinDuration, _model.selfStunMaxDuration, r);

            _damageMoment = Mathf.Clamp01(0.4f) * _execDuration;

            _anim?.SetCombatActive(true);
            _anim?.TriggerSpinRelease();
            if (_anim != null) _anim.OnAnim_SpinDamage += OnSpinDamageEvent;
        }

        public override void Exit()
        {
            base.Exit();
            if (_anim != null) _anim.OnAnim_SpinDamage -= OnSpinDamageEvent;
        
            if (!_nextIsSelfStun)
                _model.ClearActionLocks(); // también resetea actionJumpSpeedMultiplier
        
            _anim?.SetCombatActive(false);
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            _t += dt;

            // Fallback de daño si no hay Animation Event
            if (!_damageTicked && _t >= _damageMoment)
            {
                DoSpinDamage();
                _damageTicked = true;
            }

            // Termina giro + post-stun → decidir destino
            if (_t >= _execDuration + _postStun)
            {
                if (_model.spinCausesSelfStun)
                {
                    _nextIsSelfStun = true;
                    _req?.Invoke(ToSelfStun);
                }
                else
                {
                    _req?.Invoke(ToIdle);
                }
                Finish();
            }
        }

        private void OnSpinDamageEvent()
        {
            DoSpinDamage(); // impact exacto si existe el evento
            _damageTicked = true;
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
