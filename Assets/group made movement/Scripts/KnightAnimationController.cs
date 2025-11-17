using UnityEngine;

/// <summary>
/// Controls knight animations based on player movement state.
/// Attach this to the knight model GameObject (child of test_body).
/// </summary>
public class KnightAnimationController : MonoBehaviour
{
    private Animator animator;
    private BoardWalk boardWalk;
    
    // Animation parameter names
    private readonly string isMovingParam = "IsMoving";
    
    void Start()
    {
        // Get Animator component (should be on the knight model or this GameObject)
        animator = GetComponent<Animator>();
        if (animator == null)
        {
            animator = GetComponentInChildren<Animator>();
        }
        
        // Get BoardWalk component (should be on the parent Player object)
        // Try to find it by going up the hierarchy
        Transform current = transform;
        while (current != null && boardWalk == null)
        {
            boardWalk = current.GetComponent<BoardWalk>();
            if (boardWalk == null)
            {
                current = current.parent;
            }
        }
        
        if (animator == null)
        {
            Debug.LogError($"KnightAnimationController: No Animator found on {gameObject.name} or its children. Make sure the knight model has an Animator component with the Knight_Animator controller assigned.");
        }
        
        if (boardWalk == null)
        {
            Debug.LogError($"KnightAnimationController: No BoardWalk component found on parent of {gameObject.name}. Make sure this is a child of a Player object.");
        }
    }
    
    void Update()
    {
        if (animator != null && boardWalk != null)
        {
            // Update animation based on movement state
            animator.SetBool(isMovingParam, boardWalk.isMoving);
        }
    }
}

