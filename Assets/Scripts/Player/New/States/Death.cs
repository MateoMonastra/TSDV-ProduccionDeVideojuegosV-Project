using FSM;
using UnityEngine;

namespace Player.New
{
    public class Death : FinishableState
    {
        public const string ToWalkIdle = "Death->WalkIdle";

        private readonly MyKinematicMotor _motor;
        private readonly PlayerModel _model;
        private readonly Camera _deathCamera;
        private readonly Camera _cameraRef;
        private readonly PlayerAnimationController _anim;
        private readonly System.Action<string> _req;
        private readonly System.Action _doRespawn;

        public Death(MyKinematicMotor motor,
            PlayerModel model,
            Camera deathCamera,
            Camera cameraRef,
            System.Action<string> request,
            PlayerAnimationController anim,
            System.Action doRespawn)
        {
            _motor = motor;
            _model = model;
            _deathCamera = deathCamera;
            _cameraRef = cameraRef;
            _req = request;
            _anim = anim;
            _doRespawn = doRespawn;
        }

        public override void Enter()
        {
            base.Enter();

            _model.LocomotionBlocked = true;
            _model.IsDead = true;

            _motor.SetVelocity(Vector3.zero);
            _motor.Frozen = true;

            _deathCamera.gameObject.SetActive(true);
            _cameraRef.gameObject.SetActive(false);

            _anim?.TriggerDeath();
            if (_anim != null) _anim.OnAnim_DeathFinished += OnDeathFinished;
        }

        public override void Exit()
        {
            base.Exit();
            if (_anim != null) _anim.OnAnim_DeathFinished -= OnDeathFinished;

            _motor.Frozen = false;

            _model.IsDead = false;
            _model.LocomotionBlocked = false;
            _model.ResetJumps();
            _model.ClearActionLocks();

            _cameraRef.gameObject.SetActive(true);
            _deathCamera.gameObject.SetActive(false);
        }

        public override void Tick(float dt)
        {
        }

        private void OnDeathFinished()
        {
            _doRespawn?.Invoke();
            _req?.Invoke(ToWalkIdle);
            Finish();
        }
    }
}