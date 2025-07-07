using KinematicCharacterController;
using KinematicCharacterController.Examples;
using Player.New;
using UnityEngine;

public class HammerController : MonoBehaviour
{
    [SerializeField] private ParticleSystem[] holdAttackParticles;
    [SerializeField] private ParticleSystem[] normalAttackParticles;
    [SerializeField] private ParticleSystem groundSlamParticles;
    [SerializeField] private PlayerAnimationEvents animationEvents;
    [SerializeField] private Animator animator;
    [SerializeField] private Animator hammerAnimator;
    [SerializeField] private BoxCollider collider;
    [SerializeField] private KillEnemy killEnemy;
    private InputSystem_Actions inputs;
    private bool isAnimating = false;
    private float holdTimeThreshold = 0.2f; // Time in seconds to consider it a hold
    private float mouseDownTime;
    private bool isGroundSlamming = false;
    private KinematicCharacterMotor motor; // Reference to the character motor
    private ExampleCharacterController characterController; // Reference to the character controller

    // Public property to check ground slam state
    public bool IsGroundSlamming => isGroundSlamming;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        inputs = new InputSystem_Actions();
        inputs.Enable();
        // Get the Animator component attached to this GameObject
        motor = GetComponentInParent<KinematicCharacterMotor>();
        characterController = GetComponentInParent<ExampleCharacterController>();

        animationEvents.AnimationComplete += OnAnimationComplete;
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


        if (characterController.CurrentCharacterState == CharacterState.Stunned)
            return;

        // Don't process new attack inputs if already animating
        if (isAnimating)
            return;

        // Handle mouse button down
        if (inputs.Player.Attack.WasPressedThisFrame())
        {
            mouseDownTime = Time.time;
        }

        // Handle mouse button up
        if (inputs.Player.Attack.WasReleasedThisFrame())
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
        else if (inputs.Player.Attack.WasPressedThisFrame() && !motor.GroundingStatus.IsStableOnGround)
        {
            GroundSlamAttack();
        }
    }

    private void StartAttack()
    {
        isAnimating = true;
        ToggleAttackCollider(true);

        animator.SetBool("IsAttacking", true);
    }

    void NormalAttack()
    {
        StartAttack();

        for (int i = 0; i < normalAttackParticles.Length; i++)
        {
            normalAttackParticles[i].Play();
        }

        animator.SetTrigger("NormalAttack");
        hammerAnimator.SetTrigger("NormalAttack");
    }

    void HoldAttack()
    {
        StartAttack();

        for (int i = 0; i < holdAttackParticles.Length; i++)
        {
            holdAttackParticles[i].Play();
        }

        killEnemy.ToggleMultipleHits(true);
        
        hammerAnimator.SetTrigger("HoldAttack");
        animator.SetTrigger("HoldAttack");
    }

    private void GroundSlamAttack()
    {
        StartAttack();

        if (animator != null)
        {
            animator.ResetTrigger("GroundSlamEnd");
            hammerAnimator.ResetTrigger("GroundSlamEnd");

            isGroundSlamming = true;
            animator.SetTrigger("GroundSlam");
            hammerAnimator.SetTrigger("GroundSlam");

            // Add downward force to the character
            if (motor != null)
            {
                motor.BaseVelocity = Vector3.down * 20f; // Adjust the force as needed
            }
        }
    }

    void EndGroundSlam()
    {
        isAnimating = false;
        ToggleAttackCollider(true);

        if (groundSlamParticles.isPlaying)
            groundSlamParticles.Stop();

        groundSlamParticles.Play();
        animator.SetTrigger("GroundSlamEnd");
        hammerAnimator.SetTrigger("GroundSlamEnd");
    }

    private void OnAnimationComplete()
    {
        if (!isGroundSlamming) // Only end normal animations if not ground slamming
        {
            isAnimating = false;
            animator.ResetTrigger("GroundSlamEnd");
            hammerAnimator.ResetTrigger("GroundSlamEnd");
            killEnemy.ToggleMultipleHits(false);
        }

        animator.SetBool("IsAttacking", false);
        ToggleAttackCollider(false);
        isGroundSlamming = false;
    }

    public void InterruptGroundSlam()
    {
        if (isGroundSlamming)
        {
            animator.SetBool("IsAttacking", false);
            animator.SetTrigger("InterruptGroundSlam");
            hammerAnimator.SetTrigger("InterruptGroundSlam");
            isGroundSlamming = false;
            isAnimating = false;
            ToggleAttackCollider(false);
        }
    }

    public void ToggleAttackCollider(bool value)
    {
        collider.enabled = value;
        killEnemy.StartAttack(true);
    }
}