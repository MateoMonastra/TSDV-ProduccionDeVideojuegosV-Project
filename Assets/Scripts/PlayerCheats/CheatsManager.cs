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
        [SerializeField] private float flySpeed;

        private ExampleCharacterController _playerCharacterController;
        private KinematicCharacterMotor _kinematicCharacterMotor;

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
                inputReader.OnFlyDown -= PlayerFlyDown;
                inputReader.OnFlyUp -= PlayerFlyUp;
                inputReader.OnFlyMove -= PlayerMovement;

                _playerCharacterController.enabled = true;
                _kinematicCharacterMotor.enabled = true;
                hammer.SetActive(true);

                _playerCharacterController.Motor.SetPositionAndRotation(
                    _playerCharacterController.gameObject.transform.position, Quaternion.identity);

                _isGodModeActive = false;
            }
            else
            {
                _isGodModeActive = true;
                _playerCharacterController.enabled = false;
                _kinematicCharacterMotor.enabled = false;
                hammer.SetActive(false);

                inputReader.OnFlyDown += PlayerFlyDown;
                inputReader.OnFlyUp += PlayerFlyUp;
                inputReader.OnFlyMove += PlayerMovement;
            }
        }

        private void PlayerFlyUp()
        {
            character.gameObject.transform.position += new Vector3(0, flySpeed, 0);
        }

        private void PlayerFlyDown()
        {
            character.gameObject.transform.position += new Vector3(0, -flySpeed, 0);
        }

        private void PlayerMovement(Vector2 direction)
        {
            character.gameObject.transform.position += new Vector3(direction.x * flySpeed, 0, direction.y * flySpeed);
        }
    }
}