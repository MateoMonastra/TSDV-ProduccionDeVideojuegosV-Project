using System;
using System.Collections.Generic;
using FSM;
using Player.New.VFX;
using UnityEngine;

namespace Player.New.States
{
    public class CastleWaterDeath : FinishableState
    {
        public const string ToWalkIdle = "Death->WalkIdle";

        private readonly MyKinematicMotor _motor;
        private readonly PlayerModel _model;
        private List<GameObject> _playerRig;
        private readonly PlayerVfxController _vfxController;
        private readonly PlayerAnimationController _anim;
        private readonly MyCharacterCamera _myCharacterCamera;
        private readonly Action<string> _req;
        private readonly Action _doRespawn;

        private float _timer;
        private bool _used;
        private float _duration = 0.02f;

        public CastleWaterDeath(MyKinematicMotor motor,
            PlayerModel model,
            PlayerVfxController vfxController,
            Action<string> request,
            PlayerAnimationController anim,
            Action doRespawn,
            List<GameObject> playerRig,
            MyCharacterCamera myCharacterCamera)
        {
            _motor = motor;
            _model = model;
            _vfxController = vfxController;
            _req = request;
            _anim = anim;
            _doRespawn = doRespawn;
            _playerRig = playerRig;
            _myCharacterCamera = myCharacterCamera;
        }

        public override void Enter()
        {
            base.Enter();
            _timer = 0;
            _used = false;
            _model.LocomotionBlocked = true;
            _model.IsDead = true;

            _anim?.SetCombatActive(false);
            _anim?.TriggerDeath();
            _vfxController?.PlayAt(VfxEvent.CastleWaterDeath);
            if (_anim != null) _anim.OnAnim_DeathFinished += OnDeathFinished;
        }

        public override void Exit()
        {
            base.Exit();
            if (_anim != null) _anim.OnAnim_DeathFinished -= OnDeathFinished;

            _motor.Frozen = false;
            
            SetActiveMesh(true);
            _myCharacterCamera.InputSubscription(true);
            _model.IsDead = false;
            _model.LocomotionBlocked = false;
            _model.ResetJumps();
            _model.ClearActionLocks();
        }

        public override void Tick(float dt)
        {
            if (_timer >= _duration)
            {
                if (!_used)
                {
                    _used = true;
                    SetActiveMesh(false);
                    _motor.SetVelocity(Vector3.zero);
                    _motor.Frozen = true;
                }
            }
            else
            {
                _timer += dt;
            }
        }

        private void OnDeathFinished()
        {
            _doRespawn?.Invoke();
            Finish();
        }

        private void SetActiveMesh(bool active)
        {
            foreach (var playerRig in _playerRig)
            {
                playerRig.SetActive(active);
            }
        }
    }
}