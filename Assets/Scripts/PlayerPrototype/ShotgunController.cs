using UnityEngine;
using KinematicCharacterController;
using UnityEngine.Serialization;

public class ShotgunController : MonoBehaviour
{
    [Header("Shotgun Settings")]
    [SerializeField] private float knockbackForce = 10f;
    [SerializeField] private float recoilForce = 5f;
    [SerializeField] private float cooldownTime = 0.5f;
    [SerializeField] private float maxDistance = 100f;
    [SerializeField] private LayerMask shootableLayers;
    [SerializeField] private Transform lookingTransf;
    [SerializeField] private Camera playerCamera;
    [SerializeField] private ParticleSystem muzzleFlashParticles;
    [FormerlySerializedAs("characterMotor")] [SerializeField] private Rigidbody characterRb;

    private float currentCooldown = 0f;
    private bool canShoot = true;

    private void Awake()
    {
        // Get camera reference if not set
        if (playerCamera == null)
        {
            playerCamera = Camera.main;
        }

        // Get particle system reference if not set
        if (muzzleFlashParticles == null)
        {
            muzzleFlashParticles = GetComponentInChildren<ParticleSystem>();
        }

        // Get character motor reference if not set
        if (characterRb == null)
        {
            characterRb = GetComponentInParent<Rigidbody>();
        }

        // Ensure particle system is stopped initially
        if (muzzleFlashParticles != null)
        {
            muzzleFlashParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        }
    }

    private void Update()
    {
        // Update cooldown
        if (!canShoot)
        {
            currentCooldown -= Time.deltaTime;
            if (currentCooldown <= 0f)
            {
                canShoot = true;
            }
        }

        // Handle shooting input
        if (Input.GetMouseButtonDown(0) && canShoot)
        {
            Shoot();
        }
    }

    private void Shoot()
    {
        // Reset cooldown
        canShoot = false;
        currentCooldown = cooldownTime;

        // Play muzzle flash particles
        if (muzzleFlashParticles != null)
        {
            // Stop any existing particles and clear them
            muzzleFlashParticles.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            // Play new particles
            muzzleFlashParticles.Play();
        }

        // Create ray from camera
        Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        RaycastHit hit;

        // Calculate shot direction (horizontal only)
        Vector3 shotDirection = playerCamera.transform.forward;
        shotDirection.y = 0f;
        shotDirection.Normalize();

        // Apply recoil to the character motor
        if (characterRb != null)
        {
            Vector3 recoilDirection = -playerCamera.transform.forward;
            characterRb.AddForce(recoilDirection * recoilForce);
        }

        // Perform raycast
        if (Physics.Raycast(ray, out hit, maxDistance, shootableLayers))
        {
            // Check if hit object has a rigidbody
            Rigidbody hitRigidbody = hit.collider.GetComponent<Rigidbody>();
            if (hitRigidbody != null)
            {
                // Calculate knockback direction (from hit point to camera)
                Vector3 knockbackDirection = (-lookingTransf.transform.forward).normalized;
                
                // Apply force to the rigidbody
                hitRigidbody.AddForce(knockbackDirection * knockbackForce, ForceMode.Impulse);
            }
        }
    }

    // Optional: Draw ray in editor for debugging
    private void OnDrawGizmos()
    {
        if (playerCamera != null)
        {
            Gizmos.color = Color.red;
            Ray ray = playerCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
            Gizmos.DrawRay(ray.origin, ray.direction * maxDistance);
        }
    }
}
