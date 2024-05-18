using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAnimator : MonoBehaviour
{
    private Animator animator;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            animator.Play("Tarras_idle_FBX");
        }
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            animator.Play("Tarras_HighAttack_003_FBX");
        }
    }
}
