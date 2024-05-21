using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationPlayer : MonoBehaviour
{
    // Reference to the Animator component
    private Animator animator;

    // Name of the animation to play
    public string animationName;

    // Start is called before the first frame update
    void Start()
    {
        // Get the Animator component attached to this GameObject
        animator = GetComponent<Animator>();

        // Check if the Animator component is not null
        if (animator == null)
        {
            Debug.LogError("Animator component not found on GameObject.");
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Check for user input to trigger the animation
        if (Input.GetKeyDown(KeyCode.Space))
        {
            // Play the specified animation
            animator.Play(animationName);
        }
    }
}