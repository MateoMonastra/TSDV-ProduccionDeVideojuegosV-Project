using System;
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
        [SerializeField] private InputReader inputReader;

        [Header("Settings")] 
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float jumpVelocity = 7f;
        [SerializeField] private float gravity = -25f;

        private Fsm _fsm;

        private const string ToWalkIdleID = "ToWalkIdle";
        private const string ToJumpID = "ToJump";
        private const string ToFallID = "ToFall";


        private void OnEnable()
        {
            inputReader.OnMove += OnHandleMove;
            inputReader.OnJump += TransitionToJump;
        }

        private void OnDisable()
        {
            inputReader.OnMove -= OnHandleMove;
            inputReader.OnJump -= TransitionToJump;
        }

        private void Start()
        {
            State walk = new WalkIdle(motor, moveSpeed, gravity, TransitionToFall);
            State jump = new Jump(motor, jumpVelocity, gravity, moveSpeed, TransitionToWalkIdle, TransitionToFall);
            State fall = new Fall(motor, gravity, moveSpeed, TransitionToWalkIdle);

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
            _fsm.GetCurrentState().HandleInput(input);
        }

        private void Update()
        {
            _fsm?.Update();
        }

        private void FixedUpdate()
        {
            _fsm?.FixedUpdate();
        }
    }
}