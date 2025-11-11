using System;
using Player.New.VFX;
using UnityEngine;

namespace Player.New.States
{
    public class SprintNew : LocomotionState
    {
        private readonly PlayerAnimationController _anim;
        private readonly PlayerVfxController _vfx;

        public SprintNew(MyKinematicMotor m,
            PlayerModel mdl,
            Transform cam,
            System.Action<string> requestTransition,
            PlayerAnimationController anim = null,
            PlayerVfxController vfx = null)
            : base(m, mdl, cam, requestTransition)
        {
            _anim = anim;
            _vfx = vfx;
        }

        public override void Enter()
        {
            base.Enter();

            Model.ActionMoveSpeedMultiplier = Model.SprintSpeedMultiplier;

            _anim?.SetWalking(false);
            _anim?.SetSprinting(true);

            _vfx.Play(VfxEvent.Run);
        }

        public override void Tick(float delta)
        {
            base.Tick(delta);

            
            Debug.LogError("TICKING IN SPRINT NEW");

            UpdateMoveInputWorld();

            ApplyLocomotion(delta, inAir: false);
        }

        private void UpdateMoveInputWorld()
        {
            Vector3 up = Motor.CharacterUp;

            Vector3 camFwd = Vector3.ProjectOnPlane(Cam.forward, up).normalized;
            if (camFwd.sqrMagnitude < Model.MinInputSqr)
                camFwd = Vector3.ProjectOnPlane(Cam.up, up).normalized;

            Vector3 camRight = Vector3.Cross(up, camFwd);

            Model.MoveInputWorld = camFwd * Model.RawMoveInput.y + camRight * Model.RawMoveInput.x;
            if (Model.MoveInputWorld.sqrMagnitude > Model.MinInputSqr)
                Model.MoveInputWorld = Model.MoveInputWorld.normalized;
        }
    }

}