using FSM;
using UnityEngine;

namespace Player.New
{
    /// <summary>
    /// Ejecución del ataque 360° tras la carga. Permite moverse/saltar con multiplicadores,
    /// aplica daño circular una vez (por evento de anim o fallback temporal) y al finalizar
    /// decide si pasa a SelfStun o vuelve a Idle.
    /// </summary>
    public class SpinRelease : FinishableState
    {
        // ──────────────────────────────────────────────────────────────────────
        public const string ToIdle     = "ToIdle";
        public const string ToSelfStun = "ToSelfStun";
        // ──────────────────────────────────────────────────────────────────────

        private readonly MyKinematicMotor _motor;
        private readonly PlayerModel _model;
        private readonly System.Action<string> _requestTransition;
        private readonly PlayerAnimationController _anim;

        private float _t;
        private bool  _damageTicked;
        private bool  _nextIsSelfStun;

        private float _execDuration;
        private float _postStun;
        private float _damageMoment;

        public System.Action<float> OnSpinCooldownUI;

        public SpinRelease(MyKinematicMotor motor,
                           PlayerModel model,
                           System.Action<string> requestTransition,
                           PlayerAnimationController anim = null)
        {
            _motor = motor;
            _model = model;
            _requestTransition = requestTransition;
            _anim = anim;
        }

        /// <summary>Entrar al release: setea multiplicadores, cooldown y calcula duraciones.</summary>
        public override void Enter()
        {
            base.Enter();
            _t = 0f;
            _damageTicked = false;
            _nextIsSelfStun = false;

            // Permitir moverse/saltar durante el giro con multiplicadores configurables
            _model.LocomotionBlocked        = false;
            _model.ActionMoveSpeedMultiplier = Mathf.Max(0.01f, _model.SpinMoveSpeedMultiplierWhileExecuting);
            _model.ActionJumpSpeedMultiplier = Mathf.Max(0.01f, _model.SpinJumpSpeedMultiplier);

            // Flags de acción
            _model.InvulnerableToEnemies = false;
            _model.AimLockActive = false;

            // Cooldown del spin
            _model.SpinOnCooldown   = true;
            _model.SpinCooldownLeft = _model.SpinCooldown;
            OnSpinCooldownUI?.Invoke(_model.SpinCooldownLeft);

            // Duraciones en función de la carga
            float r = Mathf.Clamp01(_model.SpinChargeRatio);
            _execDuration = Mathf.Lerp(_model.SpinMinDuration, _model.SpinMaxDuration, r);
            _postStun     = _model.SpinPostStun;
            _model.SelfStunDuration = Mathf.Lerp(_model.SelfStunMinDuration, _model.SelfStunMaxDuration, r);

            // Animación
            _anim?.SetCombatActive(true);
            _anim?.TriggerSpinRelease();
            if (_anim != null) _anim.OnAnim_SpinDamage += OnSpinDamageEvent;
        }

        /// <summary>Salir del release: desuscribe evento y limpia locks si corresponde.</summary>
        public override void Exit()
        {
            base.Exit();
            if (_anim != null) _anim.OnAnim_SpinDamage -= OnSpinDamageEvent;

            // Si vamos a SelfStun, no limpiamos aquí.
            if (!_nextIsSelfStun)
                _model.ClearActionLocks();

            _anim?.SetCombatActive(false);
        }

        /// <summary>Avanza el tiempo, aplica daño si corresponde y resuelve transición final.</summary>
        public override void Tick(float dt)
        {
            base.Tick(dt);
            _t += dt;

            // Fallback de daño si no llegó evento de anim
            if (!_damageTicked && _t >= _damageMoment)
            {
                DoSpinDamage();
                _damageTicked = true;
            }

            // Fin del giro + post-stun → decidir destino
            if (_t >= _execDuration + _postStun)
            {
                if (_model.SpinCausesSelfStun)
                {
                    _nextIsSelfStun = true;
                    _requestTransition?.Invoke(ToSelfStun);
                }
                else
                {
                    _requestTransition?.Invoke(ToIdle);
                }
                Finish();
            }
        }

        // ──────────────────────────────────────────────────────────────────────
        #region Anim Events & Damage
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>Llamado por Animation Event para sincronizar el impacto exacto.</summary>
        private void OnSpinDamageEvent()
        {
            DoSpinDamage();
            _damageTicked = true;
        }

        /// <summary>Aplica daño/knockback/stagger en un radio alrededor del jugador.</summary>
        private void DoSpinDamage()
        {
            Vector3 center = _motor.transform.position;
            Collider[] hits = Physics.OverlapSphere(
                center,
                _model.SpinRadius,
                _model.EnemyMask,
                QueryTriggerInteraction.Ignore
            );

            foreach (var c in hits)
            {
                var dmg = c.GetComponentInParent<IDamageable>();
                if (dmg == null) continue;

                // Dirección horizontal desde el centro del jugador hacia el objetivo
                Vector3 dir = (c.bounds.center - center);
                dir.y = 0f;
                if (dir.sqrMagnitude > 1e-6f) dir.Normalize();

                dmg.TakeDamage(_model.SpinDamage);
                dmg.ApplyKnockback(dir, _model.SpinPushDistance);
                dmg.ApplyStagger(_model.SpinStaggerTime);
            }
        }

        #endregion
    }
}
