using UnityEngine;

public class HammerController : MonoBehaviour
{
    private Animator animator;
    private Collider collider;
    private bool isAnimating = false;
    private float holdTimeThreshold = 0.2f; // Time in seconds to consider it a hold
    private float mouseDownTime;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        // Get the Animator component attached to this GameObject
        collider = GetComponent<BoxCollider>();
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            Debug.LogError("No Animator component found on this GameObject!");
        }
    }

    // Update is called once per frame
    void Update()
    {
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

    // This method should be called by an Animation Event at the end of your animation
    public void OnAnimationComplete()
    {
        isAnimating = false;
        collider.enabled = false;
    }
}
