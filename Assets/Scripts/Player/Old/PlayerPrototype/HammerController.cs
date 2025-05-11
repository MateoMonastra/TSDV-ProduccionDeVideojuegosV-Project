using KinematicCharacterController;
using UnityEngine;

public class HammerController : MonoBehaviour
{
    [SerializeField] private ParticleSystem groundSlamParticles;
    private Animator animator;
    private Collider collider;
    private bool isAnimating = false;
    private float holdTimeThreshold = 0.2f; // Time in seconds to consider it a hold
    private float mouseDownTime;
    private bool isGroundSlamming = false;
    private KinematicCharacterMotor motor; // Reference to the character motor

    // Public property to check ground slam state
    public bool IsGroundSlamming => isGroundSlamming;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Get the Animator component attached to this GameObject
        collider = GetComponent<BoxCollider>();
        animator = GetComponent<Animator>();
        motor = GetComponentInParent<KinematicCharacterMotor>();
        if (animator == null)
        {
            Debug.LogError("No Animator component found on this GameObject!");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Don't process inputs if ground slamming
        if (isGroundSlamming)
        {
            // Check if ground slam should end
            if (motor.GroundingStatus.IsStableOnGround)
            {
                EndGroundSlam();
            }
            return;
        }

        // Handle mouse button down
        if (Input.GetMouseButtonDown(0) && !isAnimating)
        {
            mouseDownTime = Time.time;
        }

        // Handle mouse button up
        if (Input.GetMouseButtonUp(0) && !isAnimating)
        {
            float holdDuration = Time.time - mouseDownTime;
            
            if (holdDuration >= holdTimeThreshold)
            {
                HoldAttack();
            }
            else
            {
                NormalAttack();
            }
        }

        // Check for ground slam input
        if (Input.GetMouseButtonDown(0) && !isAnimating && !motor.GroundingStatus.IsStableOnGround)
        {
            GroundSlamAttack();
        }
    }

    void NormalAttack()
    {
        if (animator != null)
        {
            isAnimating = true;
            animator.SetTrigger("Attack");
            collider.enabled = true;
        }
    }

    void HoldAttack()
    {
        if (animator != null)
        {
            isAnimating = true;
            animator.SetTrigger("HoldAttack");
            collider.enabled = true;
        }
    }

    void GroundSlamAttack()
    {
        if (animator != null)
        {
            isAnimating = true;
            isGroundSlamming = true;
            animator.SetTrigger("GroundSlam");
            collider.enabled = true;

            // Add downward force to the character
            if (motor != null)
            {
                motor.BaseVelocity = Vector3.down * 20f; // Adjust the force as needed
            }
        }
    }

    void EndGroundSlam()
    {
        isGroundSlamming = false;
        isAnimating = false;
        collider.enabled = true;

        if (groundSlamParticles.isPlaying)
            groundSlamParticles.Stop();

        groundSlamParticles.Play();
        animator.SetTrigger("GroundSlamEnd");
    }

    // This method should be called by an Animation Event at the end of your animation
    public void OnAnimationComplete()
    {
        if (!isGroundSlamming) // Only end normal animations if not ground slamming
        {
            isAnimating = false;
            collider.enabled = false;
        }
    }
}