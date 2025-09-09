using FSM;
using UnityEngine;

namespace Player.New.States
{
/// <summary>
/// Estado de “golpeado”: aplica knockback + stun y bloquea input por un tiempo.
/// Se alimenta con PlayerModel.LastDamage (escrito por PlayerAgent cuando HealthController dispara OnTakeDamage).
/// </summary>
    public class PlayerHit : FinishableState
    {
        public const string ToWalkIdle = "Hit->ToWalkIdle";

        private readonly MyKinematicMotor _m;
        private readonly PlayerModel _model;
        private readonly System.Action<string> _req;
        private readonly PlayerAnimationController _anim;

        private float _t;
        private bool _impulseApplied;

        public PlayerHit(MyKinematicMotor m, PlayerModel model, System.Action<string> req, PlayerAnimationController anim = null)
        {
            _m = m; _model = model; _req = req; _anim = anim;
        }

        public override void Enter()
        {
            base.Enter();
            _t = 0f;
            _impulseApplied = false;

            _model.LocomotionBlocked = true;
            _anim.TriggerHit();
        }

        public override void Exit()
        {
            base.Exit();
            _model.LocomotionBlocked = false;
            _anim?.SetCombatActive(false);
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
            _t += dt;
            
            if (!_impulseApplied)
            {
                ApplyImpulseFromLastDamage();
                _impulseApplied = true;
            }

            if (_t >= _model.HitStunTime)
            {
                _req?.Invoke(ToWalkIdle);
                Finish();
            }
        }

        private void ApplyImpulseFromLastDamage()
        {
            Vector3 fromAttacker = _model.LastDamage.HasValue
                ? (_m.transform.position - _model.LastDamage.Value.DamageOrigin).WithY(0f)
                : (-_m.transform.forward).WithY(0f);

            Vector3 up = _m.CharacterUp;
            Vector3 horiz = Vector3.ProjectOnPlane(fromAttacker, up);
            if (horiz.sqrMagnitude < 1e-6f) horiz = -_m.transform.forward;
            horiz.Normalize();

            Vector3 v = _m.Velocity;
            v = horiz * _model.HitKnockbackHorizontal + up * _model.HitKnockbackUp;
            _m.SetVelocity(v);
        }
    }

    static class VecExt
    {
        public static Vector3 WithY(this Vector3 v, float y) { v.y = y; return v; }
    }
}
