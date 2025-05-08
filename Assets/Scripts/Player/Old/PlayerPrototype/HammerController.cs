using UnityEngine;

public class HammerController : MonoBehaviour
{
    private Animator animator;
    private Collider collider;
    private bool isAnimating = false;

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
        // Check for left mouse button click and if we're not currently animating
        if (Input.GetMouseButtonDown(0) && !isAnimating)
        {
            PlayHammerAnimation();
            collider.enabled = true;
        }
    }

    void PlayHammerAnimation()
    {
        if (animator != null)
        {
            isAnimating = true;
            animator.SetTrigger("Attack");
        }
    }

    // This method should be called by an Animation Event at the end of your animation
    public void OnAnimationComplete()
    {
        isAnimating = false;
        collider.enabled = false;
    }
}
