using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player.Weapons
{
    public class ElectricBat : Weapon
    {
        [Header("Reference")]
        public HitDetection HeavySwingHitbox;
        public HitDetection SpecialAttackHitbox;

        [Header("")]
        public float MaxChargeLength = 1.0f;
        public float ChargeSpeed = 0.2f;

        private float heavyAttackCooldownTime;
        private float specialAttackCooldownTime;

        private float chargedAmount;
        private float lastChargedAmount;

        public override void Initialize(Controller controller)
        {
            base.Initialize(controller);
            HeavySwingHitbox.gameObject.SetActive(false);
            SpecialAttackHitbox.gameObject.SetActive(false);
        }

        public override void UpdateWeapon()
        {
            base.UpdateWeapon();

            if (!Controller.State.Attacking && alreadyCollidedHitbox)
                alreadyCollidedHitbox = false;

            // Handle the attack SFX
            UpdateSFX();
            // Handle the weapon trails
            UpdateTrail();

            if (!Controller.State.Attacking)
            {
                if (Controller.Input.HoldLeftFire && Time.time >= heavyAttackCooldownTime /*&& !Controller.Motor.State.IsWallJumping*/)
                {
                    // If we're holding left click, we want to increase the charge amount over time
                    chargedAmount += ChargeSpeed * Time.deltaTime;
                    chargedAmount = Mathf.Clamp(chargedAmount, 0, MaxChargeLength);
                    // We also want to set the aiming state
                    Controller.State.Aiming = true;
                    Controller.State.ChargedAmount = chargedAmount;
                    MovementAttributes.AimSpeed = GameMath.Remap(chargedAmount, 0, MaxChargeLength, MovementAttributes.WalkSpeed, 4.0f);
                }
                else if (chargedAmount > 0 && Time.time >= heavyAttackCooldownTime)
                {
                    Controller.State.AttackNumber = 0;

                    playedSFX = false;
                    StartAttack(45, 0.5f);
                }
                else if (Controller.Input.RightFireDown && Time.time >= specialAttackCooldownTime && Controller.Motor.State.IsGrounded)
                {
                    Controller.State.AttackNumber = 1;

                    playedSFX = false;
                    StartAttack(70);
                }
            }
            else
            {
                // Reset the aiming
                Controller.State.Aiming = false;
                // If we've exhausted the attack frames
                if (CurrentAttackFrame <= 0)
                {
                    Controller.State.Attacking = false;
                    ActiveSkin.Trail.StopSmoothly(0.1f);

                    // If it's the heavy attack, we want to set it's cooldown time here, so that it begins after the actual attack
                    if (Controller.State.AttackNumber == 0)
                        heavyAttackCooldownTime = Time.time + 0.5f;//0.33f;
                    else if (Controller.State.AttackNumber == 1)
                        specialAttackCooldownTime = Time.time + 0.9f;
                }
                else
                    // Reduce the attack timer (go through the attack)
                    CurrentAttackFrame--;

                SingleAttack(Controller.State.AttackNumber);
            }
        }

        private void SingleAttack(byte attackNumber)
        {
            if (attackNumber == 0)
            {
                // If the current attack frame = (attackLength - n)
                if (!Attacked && CurrentAttackFrame == 25)
                {
                    SpecialAttackHitbox.gameObject.SetActive(false);
                    StartCoroutine(DoHitDetection(HeavySwingHitbox));
                    Attacked = true;
                    CanCancel = false;
                }
                if (CurrentAttackFrame > 0)
                    MovementAttributes.AttackSpeed = 4f;
            }
            else if (attackNumber == 1)
            {
                if (!Attacked && CurrentAttackFrame == 69)
                {
                    Controller.Motor.DoAttackMove(Vector3.forward, 15, 0.95f, true, true, 0.6f);
                    Attacked = true;
                }
                if (CurrentAttackFrame > 20 && CurrentAttackFrame % 10 == 0)
                {
                    HeavySwingHitbox.gameObject.SetActive(false);
                    StartCoroutine(DoHitDetection(SpecialAttackHitbox));
                    // We do this here so that the attack can be continuous
                    alreadyCollidedHitbox = false;
                    CanCancel = false;
                }
            }

            chargedAmount = 0;
        }

        bool playedSFX;
        private void UpdateSFX()
        {
            if (playedSFX)
                return;
            playedSFX = true;

            int attackNumber = Controller.State.AttackNumber;
            AttackEffect effect = ActiveSkin.GetAttackEffect(attackNumber);
            Controller.Animator.PlayMeleeSFX(effect.AttackSFX[0], effect.AttackVolume);
        }

        private void UpdateTrail()
        {
            if (Controller.State.AttackNumber == 0)
            {
                if (CurrentAttackFrame == 40)
                    ActiveSkin.Trail.Activate();
                else if (CurrentAttackFrame == 15)
                    ActiveSkin.Trail.StopSmoothly(0.1f);
            }
            else if (Controller.State.AttackNumber == 1)
            {
                if (CurrentAttackFrame == 65)
                    ActiveSkin.Trail.Activate();
                else if (CurrentAttackFrame == 20)
                    ActiveSkin.Trail.StopSmoothly(0.1f);
            }
        }

        private IEnumerator DoHitDetection(HitDetection hit)
        {
            hit.gameObject.SetActive(true);
            hit.CastRay();
            yield return new WaitForEndOfFrame();
            hit.gameObject.SetActive(false);
        }

        private bool alreadyCollidedHitbox;
        public override void OnMeleeHit(int hitType, Hitbox hitbox, RaycastHit hit)
        {
            base.OnMeleeHit(hitType, hitbox, hit);
            // NOTE WELL FUTURE ZHAV
            // you can use hit.Distance to calculate damage based on whether the hit was further out or closer in

            if (alreadyCollidedHitbox == true || hitbox.Controller.Networker.b2.IsWallSlammed)
                return;
            alreadyCollidedHitbox = true;

            // If it's a heavy attack
            if (Controller.State.AttackNumber == 0)
            {
                float normalizedChargedAmount = GameMath.Remap(Controller.State.ChargedAmount, 0, MaxChargeLength, 1.0f, 1.5f);
                Vector3 knockbackDirection = Controller.transform.TransformDirection(new Vector3(0, 0.5f, 1.0f));
                DoKnockback(hitbox, knockbackDirection, 13.0f * normalizedChargedAmount, 1.0f * normalizedChargedAmount, true, 0f);
                DoDamage(hitbox, (short)(15 * normalizedChargedAmount));
                Controller.HUD.Combo.AddComboDamage((short)(15 * normalizedChargedAmount));

                Controller.Camera.Shake(0.35f);
                Controller.Animator.PauseAnimation(0.15f);
            }
            // If it's a dash attack
            else if (Controller.State.AttackNumber == 1)
            {
                DoFlinch(hitbox, 0.05f);
                DoDamage(hitbox, 10);
                Controller.HUD.Combo.AddComboDamage(10);
            }
        }

        // Structures
        public override void OnMeleeHit(int hitType, RaycastHit hit)
        {
            base.OnMeleeHit(hitType, hit);

            // If it's a heavy attack
            if (Controller.State.AttackNumber == 0)
            {
                Controller.Camera.Shake(0.35f);
                Controller.Animator.PauseAnimation(0.12f);
            }
            // If it's a dash attack
            else if (Controller.State.AttackNumber == 1)
            {

            }

            // If it's a structure
            if (hitType == 0)
            {
                // If it's a heavy attack
                if (Controller.State.AttackNumber == 0)
                    DoStructureDamage(hit, 70);
                // If it's a dash attack
                else if (Controller.State.AttackNumber == 1)
                    DoStructureDamage(hit, 20);
            }
        }

        public override void CastEffects(Vector3 hitPoint, Vector3 hitNormal, int attackNumber)
        {
            CastMeleeEffects(hitPoint, hitNormal, ActiveSkin.GetAttackEffect(attackNumber));
        }

        public override void Enable()
        {
            base.Enable();
            alreadyCollidedHitbox = false;
            ActiveSkin.Trail.Deactivate();
        }

        public override void Disable()
        {
            base.Disable();
            alreadyCollidedHitbox = false;
            ActiveSkin.Trail.Deactivate();
        }
    }
}