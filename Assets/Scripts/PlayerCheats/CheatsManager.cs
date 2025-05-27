using System;
using KinematicCharacterController;
using KinematicCharacterController.Examples;
using Player;
using UnityEngine;

namespace PlayerCheats
{
    public class CheatsManager : MonoBehaviour
    {
        [SerializeField] private InputReader inputReader;
        [SerializeField] private GameObject character;
        [SerializeField] private GameObject hammer;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private GameObject godModeInstructions;
        [SerializeField] private float flySpeed;

        private ExampleCharacterController _playerCharacterController;
        private KinematicCharacterMotor _kinematicCharacterMotor;
        private Vector2 _currentFlyInput;
        private float _currentVerticalInput;

        private bool _isGodModeActive = false;

        private void OnEnable()
        {
            _playerCharacterController = character.GetComponent<ExampleCharacterController>();
            _kinematicCharacterMotor = character.GetComponent<KinematicCharacterMotor>();

            if (_playerCharacterController == null)
            {
                Debug.Log("player controller null");
            }

            if (_kinematicCharacterMotor == null)
            {
                Debug.Log("kinematic character motor null");
            }

            inputReader.OnDashPickUpCheat += _playerCharacterController.AddExtraDashCharge;
            inputReader.OnJumpPickUpCheat += OnJumpPickUpCheat;
            inputReader.OnGodModeCheat += SwitchGodMode;
        }

        private void OnDisable()
        {
            if (_playerCharacterController != null)
                inputReader.OnDashPickUpCheat -= _playerCharacterController.AddExtraDashCharge;

            inputReader.OnJumpPickUpCheat -= OnJumpPickUpCheat;
            inputReader.OnGodModeCheat -= SwitchGodMode;

            _playerCharacterController = null;
            _kinematicCharacterMotor = null;
        }

        private void OnJumpPickUpCheat()
        {
            _playerCharacterController.AddExtraJumps(1);
        }

        private void SwitchGodMode()
        {
            if (_isGodModeActive)
            {
                GameEvents.GameEvents.PlayerGodMode(false);

                inputReader.OnFlyDown -= PlayerFlyDown;
                inputReader.OnFlyUp -= PlayerFlyUp;
                inputReader.OnFlyDownCanceled -= StopVerticalMovement;
                inputReader.OnFlyUpCanceled -= StopVerticalMovement;
                inputReader.OnFlyMove -= PlayerMovement;

                inputReader.OnDashPickUpCheat += _playerCharacterController.AddExtraDashCharge;
                inputReader.OnJumpPickUpCheat += OnJumpPickUpCheat;

                _playerCharacterController.enabled = true;
                _kinematicCharacterMotor.enabled = true;
                hammer.SetActive(true);
                godModeInstructions.SetActive(false);

                _playerCharacterController.Motor.SetPositionAndRotation(
                    _playerCharacterController.gameObject.transform.position, Quaternion.identity);

                _isGodModeActive = false;
            }
            else
            {
                GameEvents.GameEvents.PlayerGodMode(true);

                _isGodModeActive = true;
                _playerCharacterController.enabled = false;
                _kinematicCharacterMotor.enabled = false;
                hammer.SetActive(false);
                godModeInstructions.SetActive(true);

                inputReader.OnFlyDown += PlayerFlyDown;
                inputReader.OnFlyUp += PlayerFlyUp;
                inputReader.OnFlyDownCanceled += StopVerticalMovement;
                inputReader.OnFlyUpCanceled += StopVerticalMovement;
                inputReader.OnFlyMove += PlayerMovement;

                inputReader.OnDashPickUpCheat -= _playerCharacterController.AddExtraDashCharge;
                inputReader.OnJumpPickUpCheat -= OnJumpPickUpCheat;
            }
        }

        private void PlayerFlyUp()
        {
            _currentVerticalInput = flySpeed;
        }

        private void PlayerFlyDown()
        {
            _currentVerticalInput = -flySpeed;
        }

        private void StopVerticalMovement()
        {
            _currentVerticalInput = 0f;
        }

        private void PlayerMovement(Vector2 direction)
        {
            _currentFlyInput = direction;
        }

        private void FixedUpdate()
        {
            if (_isGodModeActive)
            {
                Vector3 cameraForward = cameraTransform.forward;
                Vector3 cameraRight = cameraTransform.right;
                
                cameraForward.y = 0;
                cameraRight.y = 0;

                cameraForward.Normalize();
                cameraRight.Normalize();
                
                Vector3 horizontalMovement =
                    (_currentFlyInput.x * cameraRight + _currentFlyInput.y * cameraForward) * flySpeed;

                
                Vector3 verticalMovement = Vector3.up * (_currentVerticalInput * flySpeed);

                
                Vector3 movement = horizontalMovement + verticalMovement;

                character.transform.position += movement;
            }
        }
    }
}