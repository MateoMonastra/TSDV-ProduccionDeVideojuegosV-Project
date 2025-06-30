using FSM;
using UnityEngine;
using UnityEngine.Events;

namespace Player.New
{
    public class PlayerAgent : MonoBehaviour
    {
        public UnityEvent onFall;
        public UnityEvent onWalkIdle;
        public UnityEvent onJump;

        [Header("References")]
        [SerializeField] private MyKinematicMotor motor;
        [SerializeField] private PlayerAnimationController animationController;
        [SerializeField] private InputReader inputReader;

        [Header("Settings")]
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float jumpVelocity = 7f;
        [SerializeField] private float gravity = -25f;
        [SerializeField] private float airAcceleration = 15f;
        [SerializeField] private float maxAirSpeed = 10f;
        [SerializeField] private float rotationSharpness = 15f;
        [SerializeField] private float airControlSharpness = 5f;

        private Fsm _fsm;

        private Vector3 _moveInput;
        private Vector3 _lookInput;

        private const string ToWalkIdleID = "ToWalkIdle";
        private const string ToJumpID = "ToJump";
        private const string ToFallID = "ToFall";


        private void OnEnable()
        {
            inputReader.OnMove += OnHandleMove;
            inputReader.OnLook += OnHandleLook;
            inputReader.OnJump += TransitionToJump;
        }

        private void OnDisable()
        {
            inputReader.OnMove -= OnHandleMove;
            inputReader.OnLook -= OnHandleLook;
            inputReader.OnJump -= TransitionToJump;
        }

        private void Start()
        {
            State fall = new Fall(motor, gravity, airAcceleration, maxAirSpeed, rotationSharpness,
                TransitionToWalkIdle);
            State jump = new Jump(motor, jumpVelocity, moveSpeed, gravity, rotationSharpness, airControlSharpness, TransitionToFall);
            State walk = new WalkIdle(motor, moveSpeed, rotationSharpness, gravity, TransitionToFall);

            walk.AddTransition(new Transition { From = walk, To = jump, ID = ToJumpID });
            walk.AddTransition(new Transition { From = walk, To = fall, ID = ToFallID });

            jump.AddTransition(new Transition { From = jump, To = fall, ID = ToFallID });
            jump.AddTransition(new Transition { From = jump, To = walk, ID = ToWalkIdleID });

            fall.AddTransition(new Transition { From = fall, To = walk, ID = ToWalkIdleID });

            _fsm = new Fsm(walk);
        }

        private void TransitionToWalkIdle()
        {
            onWalkIdle?.Invoke();
            _fsm?.TryTransitionTo(ToWalkIdleID);
        }

        private void TransitionToJump()
        {
            onJump?.Invoke();
            Debug.Log(motor.IsGrounded.ToString());
            _fsm?.TryTransitionTo(ToJumpID);
        }

        private void TransitionToFall()
        {
            onFall?.Invoke();
            _fsm?.TryTransitionTo(ToFallID);
        }

        private void OnHandleMove(Vector2 input)
        {
            _moveInput = new Vector3(input.x, 0, input.y).normalized;
            SendInputToState();
        }

        private void OnHandleLook(Vector2 input)
        {
            _lookInput = new Vector3(input.x, 0, input.y).normalized;
            SendInputToState();
        }

        private void SendInputToState()
        {
            _fsm?.GetCurrentState()?.HandleInput(_moveInput, _lookInput);
        }

        private void Update()
        {
            _fsm?.Update();
            animationController.SetMovementVelocity(motor.Velocity.x, motor.Velocity.z);
        }
        
        private void FixedUpdate()
        {
            _fsm?.FixedUpdate();
        }
    }
}