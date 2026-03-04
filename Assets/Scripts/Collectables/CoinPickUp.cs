using System.Collections;
using UnityEngine;
using Event = AK.Wwise.Event;

namespace Coins
{
    public class CoinPickup : MonoBehaviour
    {
        [Header("Value")] [SerializeField] private int coinValue = 1;
        [SerializeField] private float spinSpeed = 360f;

        [Header("Spawn Burst")] [SerializeField]
        private float burstForce = 5f;
        [SerializeField] private float gravityMultiplier = 2.0f;
        [SerializeField] private float upwardForce = 4f;
        [SerializeField] private float settleTime = 0.35f;

        [Header("Landing")] [SerializeField] private LayerMask groundMask;
        [SerializeField] private float minAirTimeBeforeLandingCheck = 0.08f;
        [SerializeField] private float landedVelocityThreshold = 0.20f;
        [SerializeField] private float extraPickupDelayAfterLanding = 0.05f;

        [Header("Pickup")] [SerializeField] private float pickupDelay = 0.15f;
        [SerializeField] private float magnetRadius = 1.8f;
        [SerializeField] private float magnetSpeed = 9f;

        [Header("Fly To UI")] [SerializeField] private float flyToUiDuration = 0.25f;


        [Header("Refs")] [SerializeField] private Rigidbody rb;
        [Header("Refs")] [SerializeField] private Event pickUpEvent;
        
        [SerializeField] private GameObject pickupParticles;
        [SerializeField] private Collider triggerCollider;
        [SerializeField] private Renderer visualRenderer;

        private bool _hasLanded;
        private bool _canBePicked;
        private bool _isFlyingToUI;
        private float _spawnTime;

        private Transform _playerTarget;
        private Coroutine _flyToUiRoutine;
        private Coroutine _pickupRoutine;
        private Coroutine _enablePickupAfterLandingRoutine;

        private Camera _mainCam;
        private Camera _camera;

        private void Awake()
        {
            if (rb == null) rb = GetComponent<Rigidbody>();
            if (triggerCollider == null) triggerCollider = GetComponent<Collider>();
            _mainCam = Camera.main;
        }

        private void OnEnable()
        {
            _spawnTime = Time.time;

            _hasLanded = false;
            _canBePicked = false;
            _isFlyingToUI = false;
            _playerTarget = null;

            if (rb != null)
            {
                rb.isKinematic = false;
                rb.linearVelocity = Vector3.zero;
                rb.angularVelocity = Vector3.zero;
            }

            if (triggerCollider != null)
                triggerCollider.enabled = true;

            GameEvents.GameEvents.OnPlayerDied += StopCoroutines;
        }

        private void OnDisable()
        {
            StopCoroutines();
            GameEvents.GameEvents.OnPlayerDied -= StopCoroutines;
        }

        public void Launch(Vector3 randomDir, float randomForceMultiplier)
        {
            if (rb == null) return;

            rb.isKinematic = false;
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;

            Vector3 force = randomDir * (burstForce * randomForceMultiplier) + Vector3.up * upwardForce;
            rb.AddForce(force, ForceMode.Impulse);
        }
        
        private void FixedUpdate()
        {
            if (rb == null) return;
            if (rb.isKinematic) return;       
            if (!rb.useGravity) return;
            
            rb.AddForce(Physics.gravity * (gravityMultiplier - 1f), ForceMode.Acceleration);
        }

        private void Update()
        {
            if (!_isFlyingToUI)
            {
                transform.Rotate(0f, spinSpeed * Time.deltaTime, 0f);
            }

            if (!_hasLanded && rb != null)
            {
                bool enoughAirTime = (Time.time - _spawnTime) >= minAirTimeBeforeLandingCheck;
                bool slowEnough = rb.linearVelocity.magnitude <= landedVelocityThreshold;

                if (enoughAirTime && slowEnough)
                {
                    MarkAsLanded();
                }
            }

            if (!_canBePicked || _isFlyingToUI) return;

            if (_playerTarget != null)
            {
                float dist = Vector3.Distance(transform.position, _playerTarget.position);

                if (!(dist <= magnetRadius)) return;

                if (rb != null && !rb.isKinematic)
                {
                    rb.isKinematic = true;
                }

                transform.position = Vector3.MoveTowards(
                    transform.position,
                    _playerTarget.position,
                    magnetSpeed * Time.deltaTime
                );

                if (dist < 0.2f && _flyToUiRoutine == null)
                {
                    CoinsWallet.Instance.RequestCoinFromWorld(transform.position, coinValue);
                    Destroy(gameObject);
                }
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!_canBePicked || _isFlyingToUI) return;

            if (other.CompareTag("Player"))
            {
                _playerTarget = other.transform;

                pickUpEvent?.Post(gameObject);
                CoinsWallet.Instance.RequestCoinFromWorld(transform.position, coinValue);
                Instantiate(pickupParticles, transform.position, Quaternion.identity);
                
                Destroy(gameObject);
            }
        }

        private void OnCollisionEnter(Collision collision)
        {
            if (_hasLanded) return;

            if (((1 << collision.gameObject.layer) & groundMask) != 0)
            {
                MarkAsLanded();
            }
        }

        private void MarkAsLanded()
        {
            if (_hasLanded) return;

            _hasLanded = true;

            if (rb != null)
            {
                rb.isKinematic = true;
            }

            if (_enablePickupAfterLandingRoutine != null)
                StopCoroutine(_enablePickupAfterLandingRoutine);

            _enablePickupAfterLandingRoutine = StartCoroutine(EnablePickupAfterLanding());
        }

        private IEnumerator EnablePickupAfterLanding()
        {
            if (extraPickupDelayAfterLanding > 0f)
                yield return new WaitForSeconds(extraPickupDelayAfterLanding);

            _canBePicked = true;
            _enablePickupAfterLandingRoutine = null;
        }

        private void StopCoroutines()
        {
            if (_flyToUiRoutine != null)
            {
                StopCoroutine(_flyToUiRoutine);
                _flyToUiRoutine = null;
            }

            if (_enablePickupAfterLandingRoutine != null)
            {
                StopCoroutine(_enablePickupAfterLandingRoutine);
                _enablePickupAfterLandingRoutine = null;
            }

            if (_pickupRoutine != null)
            {
                StopCoroutine(_pickupRoutine);
                _pickupRoutine = null;
            }
        }
    }
}