using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using KinematicCharacterController;
using KinematicCharacterController.Examples;
using UnityEngine.InputSystem;

namespace KinematicCharacterController.Examples
{
    public class ExamplePlayer : MonoBehaviour
    {
        private InputSystem_Actions inputs;
        public ExampleCharacterController Character;
        public ExampleCharacterCamera CharacterCamera;

        private void Start()
        {
            inputs = new InputSystem_Actions();
            inputs.Enable();
            Cursor.lockState = CursorLockMode.Locked;

            // Tell camera to follow transform
            CharacterCamera.SetFollowTransform(Character.CameraFollowPoint);

            // Ignore the character's collider(s) for camera obstruction checks
            CharacterCamera.IgnoredColliders.Clear();
            CharacterCamera.IgnoredColliders.AddRange(Character.GetComponentsInChildren<Collider>());
        }

        private void Update()
        {
            // if (Input.GetMouseButtonDown(0))
            // {
            //     Cursor.lockState = CursorLockMode.Locked;
            // }

            HandleCharacterInput();
        }

        private void LateUpdate()
        {
            // Handle rotating the camera along with physics movers
            if (CharacterCamera.RotateWithPhysicsMover && Character.Motor.AttachedRigidbody != null)
            {
                CharacterCamera.PlanarDirection = Character.Motor.AttachedRigidbody.GetComponent<PhysicsMover>().RotationDeltaFromInterpolation * CharacterCamera.PlanarDirection;
                CharacterCamera.PlanarDirection = Vector3.ProjectOnPlane(CharacterCamera.PlanarDirection, Character.Motor.CharacterUp).normalized;
            }

            HandleCameraInput();
        }

        private void HandleCameraInput()
        {
            // Create the look input vector for the camera
            float mouseLookAxisUp = inputs.Player.Look.ReadValue<Vector2>().y;
            float mouseLookAxisRight = inputs.Player.Look.ReadValue<Vector2>().x;
            Vector3 lookInputVector = new Vector3(mouseLookAxisRight, mouseLookAxisUp, 0f);
            
            // Prevent moving the camera while the cursor isn't locked
            if (Cursor.lockState != CursorLockMode.Locked)
            {
                lookInputVector = Vector3.zero;
            }

            // Input for zooming the camera (disabled in WebGL because it can cause problems)
            float scrollInput = 0f;
            
            // Apply inputs to the camera
            CharacterCamera.UpdateWithInput(Time.deltaTime, scrollInput, lookInputVector,inputs.Player.Look.activeControl?.device);

            // Handle toggling zoom level
            // if (Input.GetMouseButtonDown(1))
            // {
            //     CharacterCamera.TargetDistance = (CharacterCamera.TargetDistance == 0f) ? CharacterCamera.DefaultDistance : 0f;
            // }
        }

        private void HandleCharacterInput()
        {
            PlayerCharacterInputs characterInputs = new PlayerCharacterInputs();

            // Build the CharacterInputs struct
            characterInputs.MoveAxisForward = inputs.Player.Move.ReadValue<Vector2>().y;
            characterInputs.MoveAxisRight = inputs.Player.Move.ReadValue<Vector2>().x;
            characterInputs.CameraRotation = CharacterCamera.Transform.rotation;
            characterInputs.JumpDown = inputs.Player.Jump.WasPerformedThisFrame();
            characterInputs.CrouchDown = inputs.Player.Crouch.WasPerformedThisFrame();
            characterInputs.CrouchUp = inputs.Player.Crouch.WasReleasedThisFrame();
            characterInputs.DashDown = inputs.Player.Sprint.WasPerformedThisFrame();
            
            //characterInputs.MoveAxisForward = Input.GetAxisRaw(VerticalInput);
            //characterInputs.MoveAxisRight = Input.GetAxisRaw(HorizontalInput);
            //characterInputs.CameraRotation = CharacterCamera.Transform.rotation;
            //characterInputs.JumpDown = Input.GetKeyDown(KeyCode.Space);
            //characterInputs.CrouchDown = Input.GetKeyDown(KeyCode.C);
            //characterInputs.CrouchUp = Input.GetKeyUp(KeyCode.C);
            //characterInputs.DashDown = Input.GetKeyDown(KeyCode.LeftShift);
            //characterInputs.MoveAxisForward = 
            // Apply inputs to character
            Character.SetInputs(ref characterInputs);
        }
    }
}