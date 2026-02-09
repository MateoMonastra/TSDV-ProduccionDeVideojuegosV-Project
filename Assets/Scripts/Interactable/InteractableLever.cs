using Player;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Interactable
{
    public class InteractableLever : MonoBehaviour, IInteractable
    {
        [Header("Settings")] [SerializeField] private InteractData interactData;
        [SerializeField] private UnityEvent onInteract;
        [SerializeField] private InputReader inputReader;
        [SerializeField] private Image pcIndicator;
        [SerializeField] private Image joystickIndicator;

        [Header("Timer Settings")] [SerializeField]
        private bool resetTimerEnabled;

        [SerializeField] private UnityEvent onReset;

        [SerializeField] private float resetTime;

        [SerializeField] private Transform interactorTargetTransform;
        [SerializeField] private Transform playerInteractTargetPosition;

        [SerializeField] private float interactionRange;
        private bool interacting;
        private bool isOnTimer;
        private float _currentExitTime;
        private InputDevice _lastDevice;


        public bool IsBeingInteracted()
        {
            return interacting;
        }

        private void OnEnable()
        {
            inputReader.OnInputPressed += UpdateInputDeviceCanvas;
        }

        private void Update()
        {
            if (isOnTimer)
            {
                _currentExitTime += Time.deltaTime;

                if (_currentExitTime >= resetTime)
                {
                    _currentExitTime = 0;
                    interacting = false;
                    onReset?.Invoke();
                }
            }
        }

        public InteractData Interact(bool hammer)
        {
            interactData.successInteraction = false;

            if (hammer)
                return interactData;


            if (interacting)
                return interactData;

            interacting = true;


            SetIndicator(false);

            interactData.interactPos = interactorTargetTransform.position;
            interactData.interactPlayerPos = playerInteractTargetPosition.position;
            interactData.successInteraction = true;
            return interactData;
        }

        public void FinishInteraction()
        {
            onInteract?.Invoke();

            if (resetTimerEnabled)
                isOnTimer = true;
        }

        public void InterruptInteraction()
        {
            interacting = false;
        }

        public bool TryInteractionRange(Vector3 interactor)
        {
            return Vector3.Distance(interactorTargetTransform.position, interactor) <= interactionRange;
        }

        public void SetIndicator(bool value)
        {
            if (interacting)
            {
                pcIndicator.gameObject.SetActive(false);
                joystickIndicator.gameObject.SetActive(false);
            }
            else
            {
                if (_lastDevice is Gamepad)
                {
                    joystickIndicator.gameObject.SetActive(value);
                    pcIndicator.gameObject.SetActive(false);
                }
                else
                {
                    pcIndicator.gameObject.SetActive(value);
                    joystickIndicator.gameObject.SetActive(false);
                }
            }
        }

        public Vector3 GetInteractionPoint()
        {
            return interactorTargetTransform.position;
        }

        private void UpdateInputDeviceCanvas(InputDevice device)
        {
            _lastDevice = device;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(interactorTargetTransform.position, interactionRange);
        }
    }
}