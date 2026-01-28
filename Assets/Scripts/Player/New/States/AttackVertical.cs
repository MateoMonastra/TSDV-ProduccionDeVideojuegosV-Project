using System.Collections;
using FSM;
using Health;
using UnityEngine;
using Platforms;
using Player.New.VFX;
using Player.New.Audio;
using Unity.Mathematics;

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
        private readonly PlayerVfxController _vfxController;
        private readonly PlayerAudioController _audioController;

        private float _t;
        private bool _impactDone;
        private bool _impactStarted;
        private float _postTimer;

        private const float MaxAirTime = 3.0f;

        public AttackVertical(MyKinematicMotor m, PlayerModel mdl, System.Action<string> req,
            PlayerAnimationController anim = null, PlayerVfxController vfxController = null,
            PlayerAudioController audioController = null)
        {
            _m = m;
            _model = mdl;
            _req = req;
            _anim = anim;
            _vfxController = vfxController;
            _audioController = audioController;
        }

        /// <summary>Puede usarse si está en aire, no hay cooldown.</summary>
        public static bool CanUse(MyKinematicMotor m, PlayerModel model)
        {
            if (m.IsGrounded || model.VerticalOnCooldown)
                return false;

            Vector3 up = m.CharacterUp;
            Vector3 down = -up;

            return !Physics.Raycast(m.transform.position, down, out var hit,
                model.MinimalGroundDistance, ~0, QueryTriggerInteraction.Ignore);
        }


        public override void Enter()
        {
            base.Enter();
            _t = 0f;
            _impactDone = false;
            _impactStarted = false;

            _postTimer = -1f;

            _model.LocomotionBlocked = true;
            _model.AimLockActive = false;

            var v = _m.Velocity;
            v.y = Mathf.Min(v.y, -_model.VerticalSlamStartDownSpeed);
            _m.SetVelocity(v);

            _anim?.SetVerticalStart(true);
            _anim?.SetFalling(false);

            if (_anim != null) _anim.OnAnim_VerticalImpact += OnAnimVerticalImpact;

            _audioController.PlayPlayerAttackSmash();
        }

        public override void Exit()
        {
            base.Exit();
            if (_anim != null) _anim.OnAnim_VerticalImpact -= OnAnimVerticalImpact;

            _anim.SetVerticalStart(false);
            _anim.CleanVerticalImpact();

            _model.ClearActionLocks();
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            _t += dt;
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

            var v = _m.Velocity;
            v.y = Mathf.Max(v.y - _model.VerticalSlamExtraAccel * dt, -_model.VerticalSlamMaxDownSpeed);
            _m.SetVelocity(v);

            bool hasHit = Physics.Raycast(_m.transform.position, Vector3.down, out RaycastHit hit, 3.0f, _m.groundMask);
            Debug.DrawRay(_m.transform.position, Vector3.down * 3.0f, hasHit ? Color.red : Color.green);
            if (hasHit)
            {
                if (!_impactStarted)
                {
                    _anim?.TriggerVerticalImpact();
                    _anim?.SetVerticalStart(false);
                    _impactStarted = true;
                }


                DoImpact();
            }
        }

        private void OnAnimVerticalImpact() => DoImpact();

        private void DoImpact()
        {
            if (_impactDone) return;
            _impactDone = true;
            _impactStarted = true;


            _audioController.PlayPlayerAttackSmashHitFloor();

            Vector3 center;

            if (Physics.Raycast(_m.transform.position + _m.transform.forward * 3.0f, Vector3.down, out RaycastHit hit,
                    3.0f, _m.groundMask))
            {
                center = hit.point + Vector3.up * 0.5f;
            }
            else
            {
                center = _m.transform.position;
            }

            Collider[] hits = Physics.OverlapSphere(
                center,
                _model.VerticalAttackRadius,
                _model.VerticalHitMask,
                QueryTriggerInteraction.Collide
            );

            _vfxController?.PlayAt(VfxEvent.VerticalAttackLand, center);

            var processedEnemies = new System.Collections.Generic.HashSet<object>();
            var processedBreakable = new System.Collections.Generic.HashSet<object>();

            foreach (var c in hits)
            {
                if (!c) continue;
                if (c.gameObject.layer == _model.PlayerLayer) continue;

                var enemyHealth = c.GetComponentInParent<HealthController>();
                if (enemyHealth != null)
                {
                    var key = (object)enemyHealth;
                    if (!processedEnemies.Contains(key))
                    {
                        processedEnemies.Add(key);

                        enemyHealth.Damage(new DamageInfo(_model.VerticalDamage, center, (8, 60)));
                    }

                    continue;
                }

                var br = c.GetComponentInParent<IBreakable>();
                if (br != null)
                {
                    var key = (object)br;
                    if (!processedBreakable.Contains(key))
                    {
                        processedBreakable.Add(key);
                        br.Break();
                    }

                    continue;
                }

                if (!_model.VerticalAffectsRigidbodies) continue;

                var rb = c.attachedRigidbody ?? c.GetComponentInParent<Rigidbody>();

                if (rb == null || rb.isKinematic) continue;

                Vector3 to = (c.bounds.center - center);
                if (to.sqrMagnitude < _model.MinInputSqr) to = Vector3.up;

                Vector3 horiz = to;
                horiz.y = 0f;
                if (horiz.sqrMagnitude > _model.MinInputSqr) horiz.Normalize();

                Vector3 pushDir = (horiz + Vector3.up * _model.VerticalRigidbodyUpFactor).normalized;
                rb.AddForce(pushDir * _model.VerticalRigidbodyImpulse, ForceMode.VelocityChange);
            }


            _model.VerticalOnCooldown = true;
            _model.VerticalCooldownLeft = _model.VerticalAttackCooldown;

            _model.LocomotionBlocked = true;
            _postTimer = Mathf.Max(0f, _model.VerticalAttackPostStun);
        }
    }
}