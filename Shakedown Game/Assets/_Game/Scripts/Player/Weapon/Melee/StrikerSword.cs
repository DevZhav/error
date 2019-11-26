using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player.Weapons
{
    public class StrikerSword : Weapon
    {
        [Header("Reference")]
        public HitDetection HeavySwingHitbox;
        public HitDetection LightSwingHitbox;
        public HitDetection SpecialAttackHitbox;

        private float heavyAttackCooldownTime;
        private float lightAttackCooldownTime;
        private float specialAttackCooldownTime;

        public override void Initialize(Controller controller)
        {
            base.Initialize(controller);
            HeavySwingHitbox.gameObject.SetActive(false);
            LightSwingHitbox.gameObject.SetActive(false);
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

            // We want to be able to attack if we're grounded or dodging
            if (Controller.Input.HeavyLeftFire && Time.time >= heavyAttackCooldownTime && (!Controller.State.Attacking || (!Controller.State.Attacking && Controller.Motor.State.IsGrounded) || Controller.Motor.State.IsDodging || (Controller.State.Attacking && Controller.State.AttackNumber == 4)))
            {
                Controller.State.AttackNumber = 0;
                playedSFX = false;

                StartAttack(60, 0.5f);
            }
            if (!Controller.State.Attacking)
            {
                if (Controller.Input.LeftFireDown && Time.time >= lightAttackCooldownTime)
                {
                    if (Controller.State.AttackNumber == 1)
                        Controller.State.AttackNumber = 2;
                    else if (Controller.State.AttackNumber == 2)
                        Controller.State.AttackNumber = 3;
                    else
                        Controller.State.AttackNumber = 1;

                    playedSFX = false;
                    StartAttack(50);

                    // This attack should cancel jump delay
                    Controller.Motor.CancelJumpDelay = true;
                }
                else if ((Controller.Input.RightFireDown || Controller.Input.HoldRightFire) && Time.time >= specialAttackCooldownTime && Controller.Motor.State.IsGrounded)
                {
                    Controller.State.AttackNumber = 4;
                    playedSFX = false;

                    StartAttack(70);
                }
            }
            else
            {
                // If we've exhausted the attack frames
                if (CurrentAttackFrame <= 0)
                {
                    Controller.State.Attacking = false;
                    ActiveSkin.Trail.StopSmoothly(0.1f);

                    // If it's the heavy attack, we want to set it's cooldown time here, so that it begins after the actual attack
                    if (Controller.State.AttackNumber == 0)
                        heavyAttackCooldownTime = Time.time + 0.1f;//0.33f;
                    else if (Controller.State.AttackNumber > 0 && Controller.State.AttackNumber < 4)
                        lightAttackCooldownTime = Time.time + 0.1f;//0.2f;
                    else if (Controller.State.AttackNumber == 4)
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
                    LightSwingHitbox.gameObject.SetActive(false);
                    SpecialAttackHitbox.gameObject.SetActive(false);
                    StartCoroutine(DoHitDetection(HeavySwingHitbox));
                    Attacked = true;
                    CanCancel = false;
                }
                if (CurrentAttackFrame > 0)
                    MovementAttributes.AttackSpeed = 4f;
            }
            else if (attackNumber > 0 && attackNumber < 4)
            {
                if (!Attacked && CurrentAttackFrame == 25)
                {
                    HeavySwingHitbox.gameObject.SetActive(false);
                    SpecialAttackHitbox.gameObject.SetActive(false);
                    StartCoroutine(DoHitDetection(LightSwingHitbox));
                    Attacked = true;
                    CanCancel = false;
                }
                if (CurrentAttackFrame > 0)
                    MovementAttributes.AttackSpeed = 5f;
            }
            else if (attackNumber == 4)
            {
                if (!Attacked && CurrentAttackFrame == 69)
                {
                    Controller.Motor.DoAttackMove(Vector3.forward, 15, 0.95f, true, true, 0.6f);
                    Attacked = true;
                }
                if (CurrentAttackFrame > 20 && CurrentAttackFrame % 10 == 0)
                {
                    LightSwingHitbox.gameObject.SetActive(false);
                    HeavySwingHitbox.gameObject.SetActive(false);
                    StartCoroutine(DoHitDetection(SpecialAttackHitbox));
                    // We do this here so that the attack can be continuous
                    alreadyCollidedHitbox = false;
                    CanCancel = false;
                }
            }
        }

        bool playedSFX;
        private void UpdateSFX()
        {
            if (playedSFX)
                return;
            playedSFX = true;

            int attackNumber = Controller.State.AttackNumber;
            if (attackNumber > 0 && attackNumber < 4)
                attackNumber = 1;
            else if (attackNumber == 4)
                attackNumber = 2;

            AttackEffect effect = ActiveSkin.GetAttackEffect(attackNumber);
            Controller.Animator.PlayMeleeSFX(effect.AttackSFX[0], effect.AttackVolume);
        }

        private void UpdateTrail()
        {
            if (Controller.State.AttackNumber == 0)
            {
                if (CurrentAttackFrame == 44)
                    ActiveSkin.Trail.Activate();
                else if (CurrentAttackFrame == 15)
                    ActiveSkin.Trail.StopSmoothly(0.1f);
            }
            else if (Controller.State.AttackNumber > 0 && Controller.State.AttackNumber < 4)
            {
                if (CurrentAttackFrame == 30)
                    ActiveSkin.Trail.Activate();
                else if (CurrentAttackFrame == 15)
                    ActiveSkin.Trail.StopSmoothly(0.1f);
            }
            else if (Controller.State.AttackNumber == 4)
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
                Vector3 knockbackDirection = Controller.transform.TransformDirection(new Vector3(0, 0.4f, 1.0f));
                DoKnockback(hitbox, knockbackDirection, 14.5f, 0.8f, false, 0.5f);
                DoDamage(hitbox, 35);

                Controller.Camera.Shake(0.3f);
                Controller.Animator.PauseAnimation(0.1f);
            }
            // If it's a light attack
            else if (Controller.State.AttackNumber > 0 && Controller.State.AttackNumber < 4)
            {
                DoFlinch(hitbox, 0.1f);
                DoDamage(hitbox, 20);

                Controller.Camera.Shake(0.1f);
                Controller.Animator.PauseAnimation(0.05f);
            }
            // If it's a dash attack
            else if (Controller.State.AttackNumber == 4)
            {
                DoFlinch(hitbox, 0.05f);
                DoDamage(hitbox, 10);
            }
        }

        // Structures
        public override void OnMeleeHit(int hitType, RaycastHit hit)
        {
            base.OnMeleeHit(hitType, hit);

            // If it's a heavy attack
            if (Controller.State.AttackNumber == 0)
            {
                Controller.Camera.Shake(0.3f);
                Controller.Animator.PauseAnimation(0.12f);
            }
            // If it's a light attack
            else if (Controller.State.AttackNumber > 0 && Controller.State.AttackNumber < 4)
            {
                Controller.Camera.Shake(0.1f);
                Controller.Animator.PauseAnimation(0.075f);
            }
            // If it's a dash attack
            else if (Controller.State.AttackNumber == 4)
            {

            }

            // If it's a structure
            if (hitType == 0)
            {
                // If it's a heavy attack
                if (Controller.State.AttackNumber == 0)
                    DoStructureDamage(hit, 70);
                // If it's a light attack
                else if (Controller.State.AttackNumber > 0 && Controller.State.AttackNumber < 4)
                    DoStructureDamage(hit, 25);
                // If it's a dash attack
                else if (Controller.State.AttackNumber == 4)
                    DoStructureDamage(hit, 20);
            }
        }

        public override void CastEffects(Vector3 hitPoint, Vector3 hitNormal, int attackNumber)
        {
            if (attackNumber > 0 && attackNumber < 4)
                attackNumber = 1;
            else if (attackNumber == 4)
                attackNumber = 2;
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