using RootMotion.FinalIK;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class ModelBodyReferences : MonoBehaviour
    {
        // --- Final IK --- //
        public BodyTilt Tilt { get; private set; }

        [Header("Back Holster Positions")]
        public Transform UpperBackHolster;
        public Transform LowerBackHolster;
        [Header("Leg Holster Positions")]
        public Transform UpperRightLegHolster;
        public Transform UpperLeftLegHolster;

        [Space(20)]
        [Header("Attacking Position")]
        public Transform RightHandHold;
        public Transform LeftHandHold;

        public void Initialize()
        {
            // Get Components
            Tilt = GetComponent<BodyTilt>();

            // Go through all the child transforms and automagically set the components
            foreach (Transform t in transform.GetComponentsInChildren<Transform>())
            {
                switch (t.name)
                {
                    case "UPPER BACK HOLSTER":
                        UpperBackHolster = t;
                        break;
                    case "LOWER BACK HOLSTER":
                        LowerBackHolster = t;
                        break;

                    case "UPPER RIGHT LEG HOLSTER":
                        UpperRightLegHolster = t;
                        break;
                    case "UPPER LEFT LEG HOLSTER":
                        UpperLeftLegHolster = t;
                        break;

                    case "RIGHT HAND HOLD":
                        RightHandHold = t;
                        break;
                    case "LEFT HAND HOLD":
                        LeftHandHold = t;
                        break;
                }
            }
        }
    }
}