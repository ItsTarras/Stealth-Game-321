using UnityEngine;
using System;
using System.Collections;

namespace twe36
{

    [RequireComponent(typeof(Animator))]
    public class IKControl : MonoBehaviour
    {

        protected Animator animator;

        public bool ikActive = false;
        public Transform rightHandObj = null;
        public Transform lookObj = null;
        [SerializeField] private Transform handlePosition;
        private bool grabbed = false;

        void Start()
        {
            animator = GetComponent<Animator>();
        }

        //a callback for calculating IK
        void OnAnimatorIK()
        {
            if (animator)
            {

                //if the IK is active, set the position and rotation directly to the goal.
                if (ikActive)
                {
                    // Set the right hand target position and rotation, if one has been assigned
                    if (rightHandObj != null && (transform.position - rightHandObj.transform.position).magnitude < 5)
                    {
                        if (grabbed == false)
                        {
                            if (lookObj != null)
                            {
                                animator.SetLookAtWeight(0.4f);
                                animator.SetLookAtPosition(lookObj.position);
                            }

                            animator.SetLayerWeight(1, 0);
                            animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0.75f);
                            animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0.75f);
                            animator.SetIKPosition(AvatarIKGoal.RightHand, rightHandObj.position);
                            animator.SetIKRotation(AvatarIKGoal.RightHand, rightHandObj.rotation);
                        }
                        else
                        {
                            //Set the weights of the holding animation to be higher.
                            animator.SetLayerWeight(1, 1f);
                        }

                        if (Input.GetKeyDown(KeyCode.E))
                        {
                            if (grabbed == false)
                            {
                                rightHandObj.SetParent(animator.GetBoneTransform(HumanBodyBones.RightHand));
                                rightHandObj.transform.position = animator.GetBoneTransform(HumanBodyBones.RightHand).position;
                                rightHandObj.GetComponent<Rigidbody>().isKinematic = true;
                                //animator.SetIKPosition(AvatarIKGoal.RightHand, handlePosition.position);
                                grabbed = true;
                            }
                            else
                            {
                                rightHandObj.GetComponent<Rigidbody>().isKinematic = false;
                                rightHandObj.SetParent(null);
                                grabbed = false;
                            }
                        }
                    }
                }

                //if the IK is not active, set the position and rotation of the hand and head back to the original position
                else
                {
                    animator.SetIKPositionWeight(AvatarIKGoal.RightHand, 0);
                    animator.SetIKRotationWeight(AvatarIKGoal.RightHand, 0);
                    animator.SetLookAtWeight(0);
                }
            }
        }
    }

}