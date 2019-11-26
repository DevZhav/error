using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class WeaponSkin : MonoBehaviour
    {
        public enum BodyPositions
        {
            RightHand,
            LeftHand,
            UpperBack,
            LowerBack,
            UpperRightLeg,
            UpperLeftLeg
        }

        [Header("Attributes")]
        public Sprite Icon;
        public byte SkinID;
        public BodyPositions HolsterPos;
        public BodyPositions HoldingPos;

        [Header("References")]
        public Transform MuzzleFlashPosition;
        private ModelBodyReferences BodyRef;
        public XftWeapon.XWeaponTrail Trail;

        [Header("Animations")]
        public bool StrafeByDefault = false;
        public WeaponAnimations Animations;

        [Header("Effects")]
        public AttackEffect[] AttackEffects;

        [Header("Sound")]
        public AudioClip ReloadSFX;
        public AudioClip[] AdditionalSFX;

        [Header("Adjusted Positions")]
        public Vector3 AdjustedPositionHolster;
        public Vector3 AdjustedRotationHolster;
        [Header("")]
        public Vector3 AdjustedPositionAttacking;
        public Vector3 AdjustedRotationAttacking;

        public void Initialize(ModelBodyReferences bodyRef, int team)
        {
            BodyRef = bodyRef;
            Trail = GetComponentInChildren<XftWeapon.XWeaponTrail>();

            PlayerMethods.SetTeamRenderers(GetComponentsInChildren<MeshRenderer>(), team);

            // Setup the animators fade duration here, just incase we didn't do it in the editor
            Animations.BasicMovement.FadeDuration = 0.15f;
            Animations.BasicAirborne.FadeDuration = 0.15f;
            Animations.StrafeMovement.FadeDuration = 0.15f;
            Animations.Airborne.FadeDuration = 0.15f;
            // Setup the animators directional type here, just incase we didn't do it in the editor
            Animations.StrafeMovement.Type = Animancer.MixerState.Serializable2D.MixerType.Directional;
            Animations.Airborne.Type = Animancer.MixerState.Serializable2D.MixerType.Directional;

            Holster();
        }

        public void Hold()
        {
            transform.parent = GetAttackingPosition();
            transform.localPosition = AdjustedPositionAttacking;
            transform.localRotation = Quaternion.Euler(AdjustedRotationAttacking);
            transform.localScale = Vector3.one;
        }

        public void Holster()
        {
            transform.parent = GetHolsterPosition();
            transform.localPosition = AdjustedPositionHolster;
            transform.localRotation = Quaternion.Euler(AdjustedRotationHolster);
            transform.localScale = Vector3.one;
        }

        public Transform GetHolsterPosition()
        {
            if (BodyRef == null)
            {
                Debug.Log("No body reference found");
                return null;
            }

            switch (HolsterPos)
            {
                case BodyPositions.RightHand:
                    return BodyRef.RightHandHold;
                case BodyPositions.LeftHand:
                    return BodyRef.LeftHandHold;
                case BodyPositions.UpperBack:
                    return BodyRef.UpperBackHolster;
                case BodyPositions.LowerBack:
                    return BodyRef.LowerBackHolster;
                case BodyPositions.UpperRightLeg:
                    return BodyRef.UpperRightLegHolster;
                case BodyPositions.UpperLeftLeg:
                    return BodyRef.UpperLeftLegHolster;
            }

            return BodyRef.UpperBackHolster;
        }
        public Transform GetAttackingPosition()
        {
            if (BodyRef == null)
            {
                Debug.Log("No body reference found");
                return null;
            }

            switch (HoldingPos)
            {
                case BodyPositions.RightHand:
                    return BodyRef.RightHandHold;
                case BodyPositions.LeftHand:
                    return BodyRef.LeftHandHold;
                case BodyPositions.UpperBack:
                    return BodyRef.UpperBackHolster;
                case BodyPositions.LowerBack:
                    return BodyRef.LowerBackHolster;
                case BodyPositions.UpperRightLeg:
                    return BodyRef.UpperRightLegHolster;
                case BodyPositions.UpperLeftLeg:
                    return BodyRef.UpperLeftLegHolster;
            }

            return BodyRef.RightHandHold;
        }

        public AudioClip GetShootSFX()
        {
            int random = Random.Range(0, AttackEffects[0].AttackSFX.Length);
            return AttackEffects[0].AttackSFX[random];
        }

        public AttackEffect GetAttackEffect(int attack)
        {
            return AttackEffects[attack];
        }
    }
}