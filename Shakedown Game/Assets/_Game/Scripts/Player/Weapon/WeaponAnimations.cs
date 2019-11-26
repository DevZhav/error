using Animancer;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    [CreateAssetMenu(fileName = "Weapon Animation", menuName = "Shakedown/Weapon Animations", order = 1)]
    public class WeaponAnimations : ScriptableObject
    {
        [Header("Locomotion")]
        public LinearMixerState.Serializable BasicMovement;
        public MixerState.Serializable2D StrafeMovement;
        public LinearMixerState.Serializable BasicAirborne;
        public MixerState.Serializable2D Airborne;

        [Header("Special")]
        public AnimationClip Sprint;
        public AnimationClip DrawWeapon;

        [Header("Upperbody")]
        public AnimationClip ReloadWeapon;
        public LinearMixerState.Serializable Aiming;
        public LinearMixerState.Serializable Shooting;

        [Header("Attack")]
        public AttackAnimations Attack;
    }

    [System.Serializable]
    public class AttackAnimations
    {
        public List<AttackAnim> Attacking;
        public AnimationClip AttackingIdleLowerbody;
    }

    [System.Serializable]
    public class AttackAnim
    {
        public int AttackNumber;
        public AnimationClip Animation;
        public bool UseFullbody;
    }
}