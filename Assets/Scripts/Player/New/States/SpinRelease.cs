using System.Collections.Generic;
using FSM;
using Health;
using Player.New.Audio;
using Player.New.VFX;
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
        public const string ToIdle = "ToIdle";
        public const string ToSelfStun = "ToSelfStun";

        private readonly MyKinematicMotor _motor;
        private readonly PlayerModel _model;
        private readonly System.Action<string> _requestTransition;
        private readonly PlayerAnimationController _anim;
        private readonly PlayerVfxController _vfxController;
        private readonly PlayerAudioController _audioController;

        private List<HealthController> staggeredHits;

        private float _t;
        private float _tickTimer = 1.0f;
        private bool _damageTicked;
        private bool _keepDamaging;
        private bool _nextIsSelfStun;

        private float _execDuration;
        private float _postStun;

        public SpinRelease(MyKinematicMotor motor,
            PlayerModel model,
            System.Action<string> requestTransition,
            PlayerAnimationController anim = null, PlayerVfxController vfxController = null,
            PlayerAudioController audioController = null)
        {
            _vfxController = vfxController;
            _motor = motor;
            _model = model;
            _requestTransition = requestTransition;
            _anim = anim;
            _audioController = audioController;
            staggeredHits = new List<HealthController>();
        }

        /// <summary>Entrar al release: setea multiplicadores, cooldown y calcula duraciones.</summary>
        public override void Enter()
        {
            base.Enter();
            _t = 0f;
            _damageTicked = true;
            _nextIsSelfStun = false;
            _keepDamaging = true;

            staggeredHits.Clear();

            
            _model.ActionMoveSpeedMultiplier = Mathf.Max(0.01f, _model.SpinMoveSpeedMultiplierWhileExecuting);
            _model.ActionJumpSpeedMultiplier = Mathf.Max(0.01f, _model.SpinJumpSpeedMultiplier);

            _model.InvulnerableToEnemies = false;
            _model.AimLockActive = false;

            _model.SpinOnCooldown = true;
            _model.SpinCooldownLeft = _model.SpinCooldown;
            _motor.RotationLocked = true;

            float r = Mathf.Clamp01(_model.SpinChargeRatio);
            _execDuration = Mathf.Lerp(_model.SpinMinDuration, _model.SpinMaxDuration, r);
            _postStun = _model.SpinPostStun;
            _model.SelfStunDuration = Mathf.Lerp(_model.SelfStunMinDuration, _model.SelfStunMaxDuration, r);

            _anim?.TriggerSpinRelease();
            if (_anim != null) _anim.OnAnim_SpinDamage += OnSpinDamageEvent;

            _vfxController?.Play(VfxEvent.SpinAttack);

            _audioController.PlayPlayerChargeAttackStart();
        }

        /// <summary>Salir del release: desuscribe evento y limpia locks si corresponde.</summary>
        public override void Exit()
        {
            base.Exit();
            if (_anim != null) _anim.OnAnim_SpinDamage -= OnSpinDamageEvent;

            if (!_nextIsSelfStun)
                _model.ClearActionLocks();

            _motor.RotationLocked = false;
            _model.JumpBlocked = false;
            _model.DashBlocked = false;
            
            
            _vfxController?.Stop(VfxEvent.SpinAttack);
        }

        /// <summary>Avanza el tiempo, aplica daño si corresponde y resuelve transición final.</summary>
        public override void Tick(float dt)
        {
            base.Tick(dt);
            _t += dt;
            _tickTimer += dt;

            if(_tickTimer >= 0.25f)
            {
                _tickTimer = 0;
                if (_damageTicked) DoSpinDamage();
            }
            


            if (_t >= _execDuration + _postStun)
            {
                _keepDamaging = false;
                DamageLastHit();
                
                if (_model.SpinCausesSelfStun)
                {
                    _nextIsSelfStun = true;
                    _requestTransition?.Invoke(ToSelfStun);
                    _audioController.PlayPlayerChargeAttackStop();
                }
                else
                {
                    _requestTransition?.Invoke(ToIdle);
                    _audioController.PlayPlayerChargeAttackStop();
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
            _damageTicked = true;
        }

        /// <summary>Aplica daño/knockback/stagger en un radio alrededor del jugador.</summary>
        private void DoSpinDamage()
        {
            if (!_keepDamaging) return;

            Vector3 center = _motor.transform.position;
            float radius = _model.SpinRadius;
            int mask = _model.EnemyMask.value;

            var hits = Physics.OverlapSphere(center, radius, mask, QueryTriggerInteraction.Collide);

            for (int i = 0; i < hits.Length; i++)
            {
                HealthController objectiveHealth = hits[i].GetComponentInParent<HealthController>();
                if (objectiveHealth == null) continue;

                if (!staggeredHits.Contains(objectiveHealth))
                {
                    staggeredHits.Add(objectiveHealth);
                    objectiveHealth.transform.SetParent(_motor.transform);
                }
            }

            DamageStaggeredHits();
        }

        private void DamageStaggeredHits()
        {
            Vector3 center = _motor.transform.position;
            foreach (HealthController hit in staggeredHits)
            {
                if (hit.GetCurrentHealth() != 1)
                {
                    hit.Damage(new DamageInfo(_model.SpinDamage, center, Vector2.zero,
                        "PlayerSpinAttack", 1.0f));
                }
                else
                {
                    hit.Damage(new DamageInfo(0, center, Vector2.zero,
                        "PlayerSpinAttack", 1.0f));
                }
            }
        }

        private void DamageLastHit()
        {

            Vector3 center = _motor.transform.position;

            foreach (HealthController hit in staggeredHits)
            {
                hit.transform.parent = null;
                
                hit.Damage(new DamageInfo(1, center, new Vector2(15.0f, 0.0f),
                    "PlayerSpinLastAttack", 1.5f));

            }
        }

        #endregion
    }
}