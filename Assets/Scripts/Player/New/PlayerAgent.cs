using FSM;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

namespace Player.New
{
    public class PlayerAgent : MonoBehaviour
    {
        public UnityEvent onFall;
        public UnityEvent onWalkIdle;
        public UnityEvent onJump;

        [Header("References")] 
        [SerializeField] private MyKinematicMotor motor;
        [SerializeField] private MyCharacterCamera camera;
        [SerializeField] private PlayerAnimationController animationController;
        [SerializeField] private InputReader inputReader;
        [SerializeField] private PlayerModel model;

        private Fsm _fsm;

        private const string ToWalkIdleID = "ToWalkIdle";
        private const string ToJumpID = "ToJump";
        private const string ToFallID = "ToFall";

        private InputDevice _lastInputDevice;

        private void OnEnable()
        {
            inputReader.OnMove += OnHandleMove;
            inputReader.OnLook += HandleCameraLook;
            inputReader.OnJump += TransitionToJump;
            inputReader.OnClick += SetGameCameraLock;
        }

        private void OnDisable()
        {
            inputReader.OnMove -= OnHandleMove;
            inputReader.OnLook -= HandleCameraLook;
            inputReader.OnJump -= TransitionToJump;
            inputReader.OnClick -= SetGameCameraLock;
        }

        private void Start()
        {
            State fall = new Fall(motor, model, TransitionToWalkIdle);
            State jump = new Jump(motor, model, TransitionToFall);
            State walk = new WalkIdle(motor, model, TransitionToFall);

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
            model.MoveInput = new Vector3(input.x, 0, input.y).normalized;
            SendInputToState();
        }

        private void UpdateCamera(float deltaTime)
        {
            if (camera == null) return;

            Vector3 rotationInput = new Vector3(model.LookInput.x, model.LookInput.y, 0f);

            camera.UpdateCamera(
                deltaTime,
                0,
                rotationInput,
                _lastInputDevice
            );
        }

        private void HandleCameraLook(Vector2 input, InputDevice device)
        {
            model.LookInput = input;

            _lastInputDevice = device;
        }

        private void SendInputToState()
        {
            _fsm?.GetCurrentState()?.HandleInput(model.MoveInput, model.LookInput);
        }

        private void SetGameCameraLock()
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        private void AlignAgentToCamera()
        {
            if (camera == null) return;

            Vector3 planarDir = camera.PlanarDirection;
            planarDir.y = 0f;

            if (planarDir.sqrMagnitude > 0.001f)
            {
                Quaternion lookRot = Quaternion.LookRotation(planarDir, Vector3.up);
                transform.rotation = Quaternion.Slerp(transform.rotation, lookRot, Time.deltaTime * 10f);
            }
        }
        
        private void Update()
        {
            _fsm?.Update();
            animationController.SetMovementVelocity(motor.Velocity.x, motor.Velocity.z);
            UpdateCamera(Time.deltaTime);

            if (model.MoveInput.sqrMagnitude > 0.01f)
            {
                AlignAgentToCamera();
            }
        }
        
        private void FixedUpdate()
        {
            _fsm?.FixedUpdate();
        }
    }
}
