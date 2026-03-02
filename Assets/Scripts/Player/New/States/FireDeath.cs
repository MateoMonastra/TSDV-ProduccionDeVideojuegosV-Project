using Art.VFX.Script_VFX;
using FSM;
using UnityEngine;

namespace Player.New.States
{
    public class FireDeath : FinishableState
    {
        public const string ToWalkIdle = "Death->WalkIdle";

        private readonly MyKinematicMotor _motor;
        private readonly PlayerModel _model;
        private readonly PlayerAnimationController _anim;
        private readonly System.Action<string> _req;
        private readonly System.Action _doRespawn;
        private readonly DissolvingController[] _dissolvingController;

        public FireDeath(MyKinematicMotor motor,
            PlayerModel model,
            System.Action<string> request,
            PlayerAnimationController anim,
            System.Action doRespawn,
            DissolvingController[] dissolvingController)
        {
            _motor = motor;
            _model = model;
            _req = request;
            _anim = anim;
            _doRespawn = doRespawn;
            _dissolvingController = dissolvingController;
        }

        public override void Enter()
        {
            base.Enter();

            _model.LocomotionBlocked = true;
            _model.IsDead = true;

            _motor.SetVelocity(Vector3.zero);
            _motor.Frozen = true;

            _anim?.SetCombatActive(false);
            _anim?.TriggerFireDeath();
            
            foreach (var controller in _dissolvingController)
            {
                if (controller.isActiveAndEnabled)
                {
                    controller.StartDissolve();
                }
            }

            if (_anim != null) _anim.OnAnim_FireDeathFinished += OnDeathFinished;
        }

        public override void Exit()
        {
            base.Exit();
            if (_anim != null) _anim.OnAnim_FireDeathFinished -= OnDeathFinished;
            
            _motor.Frozen = false;

            _model.IsDead = false;
            _model.LocomotionBlocked = false;
            _model.ResetJumps();
            _model.ClearActionLocks();
        }

        public override void Tick(float dt)
        {
        }

        private void OnDeathFinished()
        {
            foreach (var controller in _dissolvingController)
            {
                if (controller.isActiveAndEnabled)
                {
                    controller.Reset();
                }
            }

            _doRespawn?.Invoke();
            Finish();
        }
    }
}