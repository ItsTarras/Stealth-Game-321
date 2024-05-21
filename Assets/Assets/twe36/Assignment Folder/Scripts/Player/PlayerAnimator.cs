using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace twe36
{

    public class PlayerAnimator : MonoBehaviour
    {
        private Animator animator;
        private Rigidbody rb;
        private BoxCollider box;
        [SerializeField] private BoxCollider shoeCollider;
        private BasicCameraController controller;
        private bool isKilled = false;

        void Start()
        {
            animator = GetComponent<Animator>();
            rb = GetComponent<Rigidbody>();
            box = GetComponent<BoxCollider>();
            controller = GetComponent<BasicCameraController>();
        }

        void Update()
        {
            if (!controller.dead)
            {
                jumpCheck();
                walkingCheck();
                idleFight();
                backJogCheck();
                highCheck();
                lowCheck();
                flipCheck();
                butterflyCheck();
            }
            else
            {
                animator.SetBool("Jumping", false);
                animator.SetBool("ForwardWalk", false);
                animator.SetBool("ForwardWalk", false);
                animator.SetBool("FightingStance", false);
                animator.SetBool("BackJog", false);
                if (!isKilled)
                {
                    animator.SetTrigger("Dead");
                    isKilled = true;
                }
            }
        }


        private void jumpCheck()
        {
            if (Input.GetKey(KeyCode.Space))
            {
                //Start a jump, then reset parameters.
                animator.SetBool("Jumping", true);
                rb.useGravity = false; // Disable gravity while jumping
            }
            else
            {
                AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
                if ((stateInfo.IsName("Jump_Still") || stateInfo.IsName("Jump_Run")) && stateInfo.normalizedTime > 0.8f)
                {
                    rb.useGravity = true; // Re-enable gravity after jump
                    animator.SetBool("Jumping", false);
                }
                else
                {
                    // Jumping animation is completedd
                    animator.SetBool("Jumping", false);
                }
            }


            //While a jumping animation is playing, remove any and all gravity on the object.

        }



        private void walkingCheck()
        {
            float currentX = animator.GetFloat("BlendX");
            float currentY = animator.GetFloat("BlendY");

            if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
            {
                //Start the walking movement.
                animator.SetBool("Walking", true);
                animator.SetBool("FightingStance", false);

                //Vertical Movement
                if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.S))
                {
                    if (Input.GetKey(KeyCode.W) && currentY < 1)
                    {
                        animator.SetFloat("BlendY", Mathf.Lerp(currentY, 1, 1f * Time.deltaTime));
                    }

                    if (Input.GetKey(KeyCode.S) && currentY > -1)
                    {
                        animator.SetFloat("BlendY", Mathf.Lerp(currentY, -1, 1f * Time.deltaTime));
                    }
                }
                else
                {
                    {
                        animator.SetFloat("BlendY", Mathf.Lerp(currentY, 0, 1f * Time.deltaTime));
                    }
                }


                //Horizontal Movement
                if (Input.GetKey(KeyCode.A) || Input.GetKey(KeyCode.D))
                {
                    if (Input.GetKey(KeyCode.A) && currentX > -1)
                    {
                        animator.SetFloat("BlendX", Mathf.Lerp(currentX, -1, 1f * Time.deltaTime));
                    }

                    if (Input.GetKey(KeyCode.D) && currentX < 1)
                    {
                        animator.SetFloat("BlendX", Mathf.Lerp(currentX, 1, 1f * Time.deltaTime));
                    }
                }
                else
                {

                    // Reset to neutral if we aren't touching any horizontal key.
                    animator.SetFloat("BlendX", Mathf.Lerp(currentX, 0, 1f * Time.deltaTime));

                }

            }
            else
            {
                animator.SetBool("Walking", false);
            }
        }

        private void flipCheck()
        {
            if (Input.GetKey(KeyCode.L))
            {
                animator.SetBool("Flip", true);
            }
            else
            {
                animator.SetBool("Flip", false);
            }
        }


        private void idleFight()
        {
            if (Input.GetKeyDown(KeyCode.F))
            {
                if (!animator.GetBool("FightingStance"))
                {
                    animator.SetBool("FightingStance", true);
                }
                else
                {
                    animator.SetBool("FightingStance", false);
                }
            }
        }

        private void butterflyCheck()
        {
            if (Input.GetKey(KeyCode.H))
            {
                animator.SetBool("Butterfly", true);
            }
            else
            {
                animator.SetBool("Butterfly", false);
            }
        }


        private void backJogCheck()
        {
            if (Input.GetKey(KeyCode.S))
            {
                animator.SetBool("BackJog", true);
            }
            else
            {
                animator.SetBool("BackJog", false);
            }
        }

        private void highCheck()
        {
            if (Input.GetKey(KeyCode.J))
            {
                animator.SetBool("High", true);
            }
            else
            {
                animator.SetBool("High", false);
            }
        }

        private void lowCheck()
        {
            if (Input.GetKey(KeyCode.K))
            {
                animator.SetBool("Low", true);
            }
            else
            {
                animator.SetBool("Low", false);
            }
        }



    }

}
