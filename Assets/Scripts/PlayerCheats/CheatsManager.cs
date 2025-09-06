using Player;
using Player.New;
using Player.New.UI;
using UnityEngine;

namespace PlayerCheats
{
    /// <summary>
    /// Cheats / Herramientas de Dev:
    /// - Toggle God Mode: desactiva el motor y permite volar con WASD + cámara.
    /// - Dash PickUp Cheat: activa el buff de dash y resetea cooldown si estaba en CD.
    /// - Jump PickUp Cheat: otorga un salto extra (HasExtraJump = true).
    /// Depende de InputReader para los eventos de entrada de cheats y vuelo.
    /// </summary>
    public class CheatsManager : MonoBehaviour
    {
        [Header("Refs")]
        [SerializeField] private InputReader inputReader;
        [SerializeField] private GameObject character;
        [SerializeField] private Transform cameraTransform;
        [SerializeField] private GameObject godModeInstructions;
        [SerializeField] private HUDManager hud;

        [Header("Fly (God Mode)")]
        [SerializeField, Tooltip("Velocidad de vuelo en God Mode (m/s).")]
        private float flySpeed = 12f;

        private PlayerAgent _agent;
        private MyKinematicMotor _motor;
        private PlayerModel _model;

      
        private Vector2 _flyInput;      
        private float _flyVerticalInput;

        private bool _godMode;

        private void Reset()
        {
            if (!character) character = gameObject;
        }

        private void Awake()
        {
            if (!character) character = gameObject;
            _agent = character.GetComponent<PlayerAgent>();
            _motor = character.GetComponent<MyKinematicMotor>();
            _model = character.GetComponent<PlayerAgent>().GetPlayerModel();

            if (!cameraTransform && Camera.main) cameraTransform = Camera.main.transform;
        }

        private void OnEnable()
        {
            inputReader.OnDashPickUpCheat += OnDashPickUpCheat;
            inputReader.OnJumpPickUpCheat += OnJumpPickUpCheat;
            inputReader.OnGodModeCheat    += ToggleGodMode;
            
            inputReader.OnFlyUp          += FlyUpPressed;
            inputReader.OnFlyDown        += FlyDownPressed;
            inputReader.OnFlyUpCanceled  += FlyVerticalCanceled;
            inputReader.OnFlyDownCanceled+= FlyVerticalCanceled;
            inputReader.OnFlyMove        += FlyMove;
        }

        private void OnDisable()
        {
            if (inputReader)
            {
                inputReader.OnDashPickUpCheat -= OnDashPickUpCheat;
                inputReader.OnJumpPickUpCheat -= OnJumpPickUpCheat;
                inputReader.OnGodModeCheat    -= ToggleGodMode;

                inputReader.OnFlyUp           -= FlyUpPressed;
                inputReader.OnFlyDown         -= FlyDownPressed;
                inputReader.OnFlyUpCanceled   -= FlyVerticalCanceled;
                inputReader.OnFlyDownCanceled -= FlyVerticalCanceled;
                inputReader.OnFlyMove         -= FlyMove;
            }
        }
        
        /// <summary>Cheat: activa el buff de dash; si está en cooldown, lo resetea.</summary>
        private void OnDashPickUpCheat()
        {
            if (!_model) return;
            
            _model.DashBuffPending = true;
            if (_model.DashOnCooldown)
            {
                _model.DashOnCooldown  = false;
                _model.DashCooldownLeft = 0f;
            }
            
            hud?.SetPickupDashBuffActive(true);
        }

        /// <summary>Cheat: otorga un salto extra (tercer salto).</summary>
        private void OnJumpPickUpCheat()
        {
            if (!_model) return;
            _model.HasExtraJump = true;
            hud?.SetPickupExtraJumpActive(true);
        }
        
        private void ToggleGodMode()
        {
            if (_godMode) DisableGodMode();
            else          EnableGodMode();
        }

        private void EnableGodMode()
        {
            _godMode = true;
            GameEvents.GameEvents.PlayerGodMode(true);

            if (godModeInstructions) godModeInstructions.SetActive(true);
            
            if (_agent) _agent.enabled = false;
            if (_motor) _motor.enabled = false;
            
            _flyInput = Vector2.zero;
            _flyVerticalInput = 0f;
        }

        private void DisableGodMode()
        {
            _godMode = false;
            GameEvents.GameEvents.PlayerGodMode(false);

            if (godModeInstructions) godModeInstructions.SetActive(false);
            
            if (_motor)
            {
                _motor.WarpTo(character.transform.position, character.transform.rotation);
                _motor.SetVelocity(Vector3.zero);
                _motor.enabled = true;
            }
            if (_agent) _agent.enabled = true;
            
            _flyInput = Vector2.zero;
            _flyVerticalInput = 0f;
        }

        private void FlyUpPressed()    => _flyVerticalInput =  1f;
        private void FlyDownPressed()  => _flyVerticalInput = -1f;
        private void FlyVerticalCanceled() => _flyVerticalInput = 0f;
        private void FlyMove(Vector2 dir)  => _flyInput = dir; 
        

        private void FixedUpdate()
        {
            if (!_godMode) return;
            if (!character || !cameraTransform) return;
        
            Vector3 camFwd = cameraTransform.forward; camFwd.y = 0f; camFwd.Normalize();
            Vector3 camRight = cameraTransform.right; camRight.y = 0f; camRight.Normalize();
           
            Vector3 hor = (camRight * _flyInput.x + camFwd * _flyInput.y) * flySpeed;
            
            Vector3 ver = Vector3.up * (_flyVerticalInput * flySpeed);

            Vector3 delta = (hor + ver) * Time.fixedDeltaTime;
            character.transform.position += delta;
        }
    }
}
