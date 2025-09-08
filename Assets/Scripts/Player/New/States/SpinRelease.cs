using FSM;
using Health;
using Player.New.UI;
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
        public const string ToIdle     = "ToIdle";
        public const string ToSelfStun = "ToSelfStun";

        private readonly MyKinematicMotor _motor;
        private readonly PlayerModel _model;
        private readonly System.Action<string> _requestTransition;
        private readonly PlayerAnimationController _anim;
        private readonly HUDManager _hud;

        private float _t;
        private bool  _damageTicked;
        private bool  _nextIsSelfStun;

        private float _execDuration;
        private float _postStun;

        public SpinRelease(MyKinematicMotor motor,
                           PlayerModel model,
                           HUDManager hud,
                           System.Action<string> requestTransition,
                           PlayerAnimationController anim = null)
        {
            _motor = motor;
            _model = model;
            _hud = hud;
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
            
            _model.InvulnerableToEnemies = false;
            _model.AimLockActive = false;

            // Cooldown del spin
            _model.SpinOnCooldown   = true;
            _model.SpinCooldownLeft = _model.SpinCooldown;
            _hud.OnSpinCooldown(_model.SpinCooldownLeft);

            // Duraciones en función de la carga
            float r = Mathf.Clamp01(_model.SpinChargeRatio);
            _execDuration = Mathf.Lerp(_model.SpinMinDuration, _model.SpinMaxDuration, r);
            _postStun     = _model.SpinPostStun;
            _model.SelfStunDuration = Mathf.Lerp(_model.SelfStunMinDuration, _model.SelfStunMaxDuration, r);
            
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

            DoSpinDamage();
            
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
            float radius   = _model.SpinRadius;
            int   mask     = _model.EnemyMask.value;

            var hits = Physics.OverlapSphere(center, radius, mask, QueryTriggerInteraction.Collide);
            for (int i = 0; i < hits.Length; i++)
            {
                var objectiveHealth = hits[i].GetComponentInParent<HealthController>();
                if (objectiveHealth == null) continue;

                objectiveHealth.Damage(new DamageInfo(_model.SpinDamage, center, (0,0)));
            }


        }

        #endregion
    }
}
