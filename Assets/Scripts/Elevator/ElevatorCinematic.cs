using System;
using System.Collections;
using UnityEngine;
using Player.New;

namespace Elevator
{
    [RequireComponent(typeof(Animator))]
    public class ElevatorCinematic : MonoBehaviour
    {
        private static readonly int PlayerEntered = Animator.StringToHash("PlayerEntered");

        [Header("Platform")]
        [SerializeField] private Transform playerSnapPoint;

        [Header("Animation")]
        [SerializeField] private Animator animator;
        [Tooltip("Clip que mueve la plataforma hacia arriba (para sacar duración del shake)")]
        [SerializeField] private AnimationClip upClip;

        [Header("Camera Shake")]
        [SerializeField] private float maxShakeDistance = 18f;
        [SerializeField] private float shakeMagnitude = 0.45f;

        [Header("One Shot")]
        [SerializeField] private bool triggerOnce = true;

        private bool _used;
        private bool _running;

        private PlayerAgent _player;
        private MyKinematicMotor _motor;
        private Transform _originalPlayerParent;

        private void Reset()
        {
            animator = GetComponent<Animator>();
        }

        private void Awake()
        {
            if (animator == null) animator = GetComponent<Animator>();
        }

        private void OnTriggerEnter(Collider other)
        {
            if (_running) return;
            if (triggerOnce && _used) return;

            var agent = other.GetComponentInParent<PlayerAgent>();
            if (agent == null) return;

            _used = true;
            StartCinematic(agent);
        }

        private void StartCinematic(PlayerAgent agent)
        {
            _running = true;
            _player = agent;
            
            _player.SetCinematicMode(true);

            _motor = _player.GetMotor();
            if (_motor != null && playerSnapPoint != null)
            {
                _motor.WarpTo(playerSnapPoint.position, playerSnapPoint.rotation);
                _motor.SetVelocity(Vector3.zero);
            }
            
            float duration = (upClip != null) ? Mathf.Max(0.05f, upClip.length) : 0.75f;
            var cam = _player.GetCharacterCamera();
            if (cam != null)
            {
                cam.TriggerCameraShake(duration, maxShakeDistance, shakeMagnitude);
            }
            
            animator.SetTrigger(PlayerEntered);

            // StartCoroutine(WarpPlayer());
        }

        private IEnumerator WarpPlayer()
        {
            while (_running)
            {
                _motor.WarpTo(playerSnapPoint.position, playerSnapPoint.rotation);
                _motor.SetVelocity(Vector3.zero);
                yield return null;
            }
            yield break;
        }

        private void LateUpdate()
        {
            if (_running)
            {
                _motor.WarpTo(playerSnapPoint.position, playerSnapPoint.rotation);
                _motor.SetVelocity(Vector3.zero);
            }
        }


        public void EndCinematic()
        {
            if (!_running) return;
            
            _player.SetCinematicMode(false);

            _player = null;
            _running = false;
        }
    }
}