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

            // Bloqueos durante el giro
            _model.locomotionBlocked = true;
            _model.actionMoveSpeedMultiplier = 0f;
            _model.invulnerableToEnemies = true;
            _model.aimLockActive = false;

            // Cooldown
            _model.SpinOnCooldown   = true;
            _model.SpinCooldownLeft = _model.SpinCooldown;
            OnSpinCooldownUI?.Invoke(_model.SpinCooldownLeft);

            // Duraciones escaladas por carga
            float r = Mathf.Clamp01(_model.spinChargeRatio);
            _execDuration = Mathf.Lerp(_model.spinMinDuration, _model.spinMaxDuration, r);
            _postStun     = _model.SpinPostStun;

            // SelfStun escalable
            _model.selfStunDuration = Mathf.Lerp(_model.selfStunMinDuration, _model.selfStunMaxDuration, r);

            // Momento del daño (fallback) ~40% del giro (podés tunearlo si querés)
            _damageMoment = Mathf.Clamp01(0.4f) * _execDuration;

            _anim?.SetCombatActive(true);
            _anim?.TriggerSpinRelease();
            if (_anim != null) _anim.OnAnim_SpinDamage += OnSpinDamageEvent;
        }

        public override void Exit()
        {
            base.Exit();
            if (_anim != null) _anim.OnAnim_SpinDamage -= OnSpinDamageEvent;
            
            // Si NO voy a SelfStun, libero locks acá.
            if (!_nextIsSelfStun)
                _model.ClearActionLocks();
            
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
