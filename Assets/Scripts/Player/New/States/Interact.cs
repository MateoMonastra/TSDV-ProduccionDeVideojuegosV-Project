using FSM;
using UnityEngine;

namespace Player.New.States
{
    public class Interact : FinishableState
    {
        public const string ToIdle = "ToIdle";

        private readonly MyKinematicMotor _motor;
        private readonly PlayerModel _model;
        private readonly System.Action<string> _requestTransition;
        private readonly PlayerAnimationController _anim;

        private InteractData _interactData;
        private float _t;                 
        private bool  _playedGetUp;      

        public Interact(MyKinematicMotor motor,
                        PlayerModel model,
                        System.Action<string> requestTransition,
                        PlayerAnimationController anim = null)
        {
            _motor = motor;
            _model = model;
            _requestTransition = requestTransition;
            _anim = anim;
        }

        public override void Enter()
        {
            base.Enter();

            _t = 0f;
            _playedGetUp = false;

            _model.LocomotionBlocked         = true;
            _model.ActionMoveSpeedMultiplier = 0f;
            _model.InvulnerableToEnemies     = false;
            _model.AimLockActive             = false;
            
            _model.IsSelfStunned   = true;
            _model.SelfStunTimeLeft = _model.SelfStunDuration;
            
            
            ZeroHorizontalVelocity();
            
            _anim?.SetInteracting(true);
            _motor.WarpTo(_interactData.interactPos, _interactData.interactRot);
        }

        /// <summary>Salir: limpia flag y locks (vía <see cref="PlayerModel.ClearActionLocks"/>).</summary>
        public override void Exit()
        {
            base.Exit();
            _model.IsSelfStunned = false;
            _model.ClearActionLocks();
            _anim?.SetInteracting(false);
        }

        /// <summary>
        /// Mantiene al personaje inmóvil horizontalmente, cuenta el tiempo de stun y
        /// dispara “get up” cerca del final. Al finalizar, vuelve a Idle.
        /// </summary>
        public override void Tick(float dt)
        {
            base.Tick(dt);

            ZeroHorizontalVelocity();
        }
        
        /// <summary>Anula la velocidad horizontal conservando la componente vertical.</summary>
        private void ZeroHorizontalVelocity()
        {
            Vector3 v = _motor.Velocity;
            v.x = 0f; v.z = 0f;
            _motor.SetVelocity(v);
        }

        public void SetData(InteractData data)
        {
            _interactData = data;
        }
    }
}