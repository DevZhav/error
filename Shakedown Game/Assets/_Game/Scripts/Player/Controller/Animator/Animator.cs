using UnityEngine;
using Animancer;
using System.Collections;

namespace Player
{
    public class Animator : MonoBehaviour
    {
        [Header("References")]
        public Networker Networker;
        private Controller Controller;
        [HideInInspector] public AnimancerComponent Anim;
        public ModelBodyReferences ModelRefrences;
        public AvatarMask UpperbodyMask;
        public AvatarMask AdditiveMask;
        public AudioSource Source;

        [Header("SFX")]
        public AudioSource ShootSource;
        public AudioSource ReloadSource;
        public AudioSource MeleeSource;

        [Header("Animations")]
        public BaseAnimations Base;
        public WeaponAnimations Weapon;

        private float wallJumpX;

        private Vector3 Velocity;
        private Vector3 MoveDirectionXZ;
        private Vector3 MoveDirectionXZRelative;
        private Vector3 interpolatedXZRelative;
        private float MoveSpeed;

        public void Initialize(Controller controller)
        {
            Controller = controller;
            Anim = GetComponent<AnimancerComponent>();

            // Set the upper body layer
            Anim.SetLayerMask(1, UpperbodyMask);
            Anim.GetLayer(1).SetWeight(1);
            // Set additive layer
            Anim.SetLayerMask(2, AdditiveMask);
            Anim.GetLayer(2).SetWeight(1);
            Anim.GetLayer(2).IsAdditive = true;

            // Setup sound source
            foreach (AudioSource source in transform.root.GetComponentsInChildren<AudioSource>())
                source.spatialBlend = Networker.IsOwner ? 0.0f : 1.0f;
        }

        public void HandleAnimations()
        {
            if (Networker == null)
                return;
            // We set these variables here so we can access them at any time in another script
            // It's basically caching them so things can be a bit more performant
            Velocity = Networker.GetVelocity();
            MoveDirectionXZ = new Vector3(Velocity.x, 0, Velocity.z);
            MoveDirectionXZRelative = transform.InverseTransformDirection(new Vector3(Velocity.x, 0, Velocity.z));
            MoveSpeed = Networker.GetMoveSpeed();

            // Upperbody stuff
            if (Networker.b1.DrawingWeapon)
            {
                if (!Anim.IsPlaying(Weapon.DrawWeapon))
                {
                    // Set layer weight
                    Anim.GetLayer(1).SetWeight(1);

                    Anim.Stop(Weapon.ReloadWeapon);
                    Anim.CrossFade(Weapon.ReloadWeapon, 0.1f, 1);
                }
            }
            else if (Networker.b1.Reloading)
            {
                if (!Anim.IsPlaying(Weapon.ReloadWeapon))
                {
                    // Set layer weight
                    Anim.GetLayer(1).SetWeight(1);

                    Anim.Stop(Weapon.ReloadWeapon);
                    Anim.CrossFade(Weapon.ReloadWeapon, 0.2f, 1);
                }
            }
            else if ((Networker.b1.Attacking || Networker.b1.Aiming) && Controller.ActiveWeapon.AimToCamera)
            {
                // Set layer weight
                Anim.GetLayer(1).SetWeight(1);

                var state = (LinearMixerState)Anim.Transition(Weapon.Aiming, 1);
                state.Parameter = (float)Networker.nv_Pitch.Value / 100;
            }
            else if (Networker.b1.Attacking && !Controller.ActiveWeapon.AimToCamera)
            {
                foreach (AttackAnim attackAnim in Weapon.Attack.Attacking)
                {
                    if (attackAnim.AttackNumber != Networker.nv_AttackNumber.Value)
                        continue;
                    if (attackAnim.UseFullbody)
                        break;

                    if (!Anim.IsPlaying(attackAnim.Animation))
                    {
                        // Set layer weight
                        Anim.GetLayer(1).SetWeight(1);

                        Anim.Stop(attackAnim.Animation);
                        Anim.CrossFade(attackAnim.Animation, 0.15f, 1);
                    }
                    break;
                }
            }
            else if (Networker.b1.Aiming && !Controller.ActiveWeapon.AimToCamera)
            {
                // Set layer weight
                Anim.GetLayer(1).SetWeight(1);

                var state = (LinearMixerState)Anim.Transition(Weapon.Aiming, 1);
                state.Parameter = 0;
            }
            else
            {
                // Set layer weight
                Anim.GetLayer(1).StartFade(0, 0.15f);
            }

            // Lowerbody stuff
            if (Networker.b1.Attacking || Networker.b1.Aiming)
            {
                if (!Controller.ActiveWeapon.AimToCamera)
                {
                    if (Networker.b1.Aiming)
                    {
                        DoBasicMoveLocomotion();
                    }
                    else
                    {
                        foreach (AttackAnim attackAnim in Weapon.Attack.Attacking)
                        {
                            if (attackAnim.AttackNumber != Networker.nv_AttackNumber.Value)
                                continue;

                            transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(0, 0, 0), 18 * Time.deltaTime);
                            if (!attackAnim.UseFullbody)
                            {
                                DoBasicMoveLocomotion();
                                break;
                            }

                            if (!Anim.IsPlaying(attackAnim.Animation))
                            {
                                Anim.GetLayer(1).SetWeight(0);
                                Anim.Stop(attackAnim.Animation);
                                Anim.CrossFade(attackAnim.Animation, 0.15f);
                            }
                            break;
                        }
                    }
                }
                else if (Controller.ActiveWeapon.AimToCamera)
                {
                    DoBasicMoveLocomotion();

                    Vector3 direction = new Vector3(Networker.nv_AimPositionX.Value / 10, Networker.nv_AimPositionY.Value / 10, Networker.nv_AimPositionZ.Value / 10) - Controller.transform.position;
                    Quaternion lookRotation = Quaternion.LookRotation(direction);
                    lookRotation.x = 0;
                    lookRotation.z = 0;

                    transform.rotation = Quaternion.Lerp(transform.rotation, lookRotation, 18 * Time.deltaTime);
                }
            }
            else if (Networker.b2.IsWallJumpFrozen)
            {
                transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(0, 0, 0), 18 * Time.deltaTime);
                Anim.CrossFade(Base.WallJumpFrozen, 0.15f);
            }
            else if (Networker.b2.IsWallJumping && Velocity.y >= -4)
            {
                var state = (LinearMixerState)Anim.Transition(Base.WallJumping);
                if (wallJumpX > 6)
                    Base.WallJumping.State.Parameter = 1;
                else if (wallJumpX < -6)
                    Base.WallJumping.State.Parameter = -1;
                else
                    Base.WallJumping.State.Parameter = 0;

                transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(0, 0, 0), 15 * Time.deltaTime);
            }
            else if (Networker.b1.IsDodging)
            {
                if (Networker.b1.DodgeDirection && !Anim.IsPlaying(Base.DodgeRight))
                {
                    Anim.Stop(Base.DodgeRight);
                    // Moving right
                    var state = Anim.CrossFade(Base.DodgeRight, 0.2f);
                    state.Speed = 1.4f;
                }
                if (!Networker.b1.DodgeDirection && !Anim.IsPlaying(Base.DodgeLeft))
                {
                    Anim.Stop(Base.DodgeLeft);
                    // Moving left
                    var state = Anim.CrossFade(Base.DodgeLeft, 0.2f);
                    state.Speed = 1.4f;
                }

                transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(0, 0, 0), 15 * Time.deltaTime);
            }
            else if (Networker.b2.IsWallSlammed)
            {
                if (!Anim.IsPlaying(Base.WallSlammed))
                    Anim.CrossFade(Base.WallSlammed, 0.1f);

                Quaternion to = Quaternion.LookRotation(MoveDirectionXZ);
                transform.rotation = Quaternion.Lerp(transform.rotation, to, 20 * Time.deltaTime);
            }
            else if (Networker.b2.IsKnockingBack)
            {
                Anim.CrossFade(Base.KnockingBack, 0.25f);

                // Rotate
                Quaternion to = Quaternion.LookRotation(-MoveDirectionXZ);
                transform.rotation = Quaternion.Lerp(transform.rotation, to, 15 * Time.deltaTime);
            }
            else if (Networker.b2.IsFlinching)
            {
                
            }
            else if (Networker.b2.IsStunned)
            {
                
            }
            else
            {
                if (!Networker.b1.IsSprinting)
                {
                    DoBasicMoveLocomotion();
                }
                else
                {
                    transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(0, 0, 0), 20 * Time.deltaTime);
                    Anim.CrossFade(Weapon.Sprint, 0.2f);
                }
            }

            // Update weapon holster/attack/rest
            if (Networker.b1.IsSprinting || Networker.b1.IsDodging || Networker.b2.IsWallJumpFrozen || Networker.b2.IsWallJumping || Networker.b2.IsStunned || Networker.b2.IsKnockingBack || Networker.b2.IsWallSlammed)
                Controller.ActiveWeapon.ActiveSkin.Holster();
            else if (Networker.b1.Attacking || Networker.b1.Reloading)
                Controller.ActiveWeapon.ActiveSkin.Hold();
            else
                Controller.ActiveWeapon.ActiveSkin.Hold();

            // Update Final IK
            ModelRefrences.Tilt.weight = Networker.b1.IsSprinting ? 1 : 0;

            // Animation effects
            CheckDodge();
            //CheckWallJump();
            CheckJump();
            CheckLand();
            CheckSprint();

            if (Networker.b1.IsGrounded && !Networker.b1.IsDodging && !Networker.b2.IsKnockingBack && !Networker.b2.IsStunned && !Networker.b2.IsFlinching && MoveDirectionXZ.magnitude != 0.0f && !Networker.nv_Dead.Value && !(Networker.b1.Attacking && !Controller.ActiveWeapon.AimToCamera))
                DoFootstep();
        }

        private void DoBasicMoveLocomotion()
        {
            Vector3 inverseDir = transform.InverseTransformDirection(MoveDirectionXZ);//new Vector3(transform.InverseTransformDirection(NetObj.MoveDirection).x, 0, transform.InverseTransformDirection(NetObj.MoveDirection).z);
            interpolatedXZRelative = Vector3.Lerp(interpolatedXZRelative, inverseDir, 10 * Time.deltaTime);

            Vector2 moveDir = new Vector2(interpolatedXZRelative.x, interpolatedXZRelative.z) / MoveSpeed;

            if (Networker.b1.IsGrounded)
            {
                if ((Networker.b1.Attacking && !Controller.ActiveWeapon.IsRanged) && moveDir.magnitude <= 0.1f)
                {
                    transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(0, 0, 0), 15 * Time.deltaTime);

                    Anim.CrossFade(Weapon.Attack.AttackingIdleLowerbody, 0.2f);
                }
                else if (Networker.b1.Attacking || Networker.b1.Aiming || Controller.ActiveWeapon.ActiveSkin.StrafeByDefault)
                {
                    transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(0, 0, 0), 15 * Time.deltaTime);

                    var state = (DirectionalMixerState)Anim.Transition(Weapon.StrafeMovement);
                    state.Parameter = moveDir;
                }
                else
                {
                    if (MoveDirectionXZ.magnitude >= 0.05f)
                    {
                        Quaternion to = Quaternion.LookRotation(MoveDirectionXZ);
                        transform.rotation = Quaternion.Lerp(transform.rotation, to, 15 * Time.deltaTime);
                    }
                    else
                    {
                        transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(0, 0, 0), 15 * Time.deltaTime);
                    }

                    var state = (LinearMixerState)Anim.Transition(Weapon.BasicMovement);
                    state.Parameter = moveDir.magnitude;
                }
            }
            /*
            else if (Networker.b2.IsWallSliding)
            {
                //inverseDir = new Vector3(transform.InverseTransformDirection(NetObj.WallHitNormal).x, 0, transform.InverseTransformDirection(NetObj.WallHitNormal).z);

                //if ((inverseDir.x > 0 || inverseDir.z < 0) && !Anim.IsPlaying(Base.WallSlideLeft))
                //    Anim.CrossFade(Base.WallSlideLeft, 0.15f);
                //else if ((inverseDir.x < 0 || inverseDir.z > 0) && !Anim.IsPlaying(Base.WallSlideRight))
                //    Anim.CrossFade(Base.WallSlideRight, 0.15f);

                //Quaternion to = Quaternion.LookRotation(NetObj.WallHitNormal);
                //transform.rotation = Quaternion.Lerp(transform.rotation, to, 10 * Time.deltaTime);
                Vector3 wallHitDir = new Vector3(NetObj.WallHitNormal.x, 0, NetObj.WallHitNormal.y);
                inverseDir = new Vector3(transform.InverseTransformDirection(wallHitDir).x, 0, transform.InverseTransformDirection(wallHitDir).z);
                //interpolatedXZRelative = Vector3.Lerp(interpolatedXZRelative, inverseDir, 10 * Time.deltaTime);
                //moveDir = new Vector2(interpolatedXZRelative.x, interpolatedXZRelative.z);
                moveDir = wallHitDir;

                var state = (DirectionalMixerState)Anim.Transition(Base.WallSliding);
                state.Parameter = moveDir;
            }
            */
            else
            {
                if (Networker.b1.Attacking || Networker.b1.Aiming || Controller.ActiveWeapon.ActiveSkin.StrafeByDefault)
                {
                    transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(0, 0, 0), 15 * Time.deltaTime);

                    var state = (DirectionalMixerState)Anim.Transition(Weapon.Airborne);
                    state.Parameter = moveDir;
                }
                else
                {
                    if (MoveDirectionXZ.magnitude >= 0.05f)
                    {
                        Quaternion to = Quaternion.LookRotation(MoveDirectionXZ);
                        transform.rotation = Quaternion.Lerp(transform.rotation, to, 15 * Time.deltaTime);
                    }
                    else
                    {
                        transform.localRotation = Quaternion.Lerp(transform.localRotation, Quaternion.Euler(0, 0, 0), 15 * Time.deltaTime);
                    }

                    var state = (LinearMixerState)Anim.Transition(Weapon.BasicAirborne);
                    state.Parameter = moveDir.magnitude;
                }
            }
        }

        public void PauseAnimation(float length)
        {
            StartCoroutine(PauseAnimationWait(length));
        }
        private IEnumerator PauseAnimationWait(float length)
        {
            Anim.CurrentState.Speed = 0;
            yield return new WaitForSeconds(length);
            Anim.CurrentState.Speed = 1;
        }

        #region Animation Effects
        [Header("Dodge")]
        public AudioClip DodgeSFX;
        public float DodgeVolume = 0.5f;
        public GameObject DodgeEffectBlue;
        public GameObject DodgeEffectRed;
        public Transform DodgeSpawn;
        bool dodged = false;
        private void CheckDodge()
        {
            if (Networker.b1.IsDodging && !dodged)
            {
                dodged = true;

                DodgeSpawn.localRotation = Networker.b1.DodgeDirection == false ? Quaternion.Euler(0, 270, 0) : DodgeSpawn.localRotation = Quaternion.Euler(0, 90, 0);
                //DodgeSpawn.localPosition = Networker.b1.DodgeDirection == false ? new Vector3(2, 0, 0) : new Vector3(-2, 0, 0);

                GameObject obj = Controller.Networker.nv_Team.Value != Networker.Instance.nv_Team.Value ? DodgeEffectRed : DodgeEffectBlue;
                //obj = Lean.Pool.LeanPool.Spawn(obj, DodgeSpawn.position, DodgeSpawn.rotation);
                obj = Lean.Pool.LeanPool.Spawn(obj, DodgeSpawn.position, DodgeSpawn.rotation);
                Lean.Pool.LeanPool.Despawn(obj, 2.0f);

                Source.PlayOneShot(DodgeSFX, DodgeVolume);

                /*
                // Remove Stamina
                Controller.state.Stamina -= Controller.Stamina.DodgeStamina;
                */
            }
            else if (!Networker.b1.IsDodging && dodged)
                dodged = false;
        }

        [Header("Sprint")]
        public AudioSource SprintSource;
        public AudioClip SprintSFX;
        bool sprinted = false;
        public void CheckSprint()
        {
            if (Networker.b1.IsSprinting && !sprinted)
            {
                SprintSource.Stop();
                SprintSource.clip = SprintSFX;
                SprintSource.Play();

                sprinted = true;
            }
            else if (!Networker.b1.IsSprinting)
                sprinted = false;
        }

        [Header("Jump")]
        public AudioClip JumpSFX;
        public float JumpVolume = 0.5f;
        public GameObject JumpEffect;
        public Transform JumpSpawn;
        bool jumped = false;
        private void CheckJump()
        {
            if (!Networker.b1.IsGrounded && Velocity.y > 0 && !jumped)
            {
                GameObject obj = Lean.Pool.LeanPool.Spawn(JumpEffect, JumpSpawn.position, JumpSpawn.rotation);
                Lean.Pool.LeanPool.Despawn(obj, 2.0f);

                Source.PlayOneShot(JumpSFX, JumpVolume);

                jumped = true;
            }
            else if (Networker.b1.IsGrounded && jumped)
                jumped = false;
        }

        [Header("Land")]
        public AudioClip LandGround;
        public AudioClip LandConcrete;
        public AudioClip LandMetal;
        public AudioClip LandGlass;
        public AudioClip LandWood;
        public AudioClip LandWater;
        public float LandVolume = 0.5f;
        public GameObject LandEffect;
        bool landed = false;
        private void CheckLand()
        {
            if (Networker.b1.IsGrounded && !landed)
            {
                GameObject obj = Lean.Pool.LeanPool.Spawn(LandEffect, JumpSpawn.position, JumpSpawn.rotation);
                Lean.Pool.LeanPool.Despawn(obj, 2.0f);

                if (!Networker.b1.IsSprinting && !Networker.b1.IsDodging)
                {
                    RaycastHit hit;
                    if (Physics.Raycast(transform.localPosition, Vector3.down, out hit, (Controller.Motor.Controller.height / 2) + Controller.Motor.Controller.radius, FootstepLayers))
                    {
                        if (hit.collider.gameObject.HasTag("Concrete"))
                            Source.PlayOneShot(LandConcrete, LandVolume);
                        else if (hit.collider.gameObject.HasTag("Metal"))
                            Source.PlayOneShot(LandMetal, LandVolume);
                        else if (hit.collider.gameObject.HasTag("Glass"))
                            Source.PlayOneShot(LandGlass, LandVolume);
                        else if (hit.collider.gameObject.HasTag("Wood"))
                            Source.PlayOneShot(LandWood, LandVolume);
                        else if (hit.collider.gameObject.HasTag("Water"))
                            Source.PlayOneShot(LandWater, LandVolume);
                        else
                            Source.PlayOneShot(LandGround, LandVolume);
                    }
                }

                /*
                if (Networker.b3.IsJumpDelay)
                {
                    // Play landing animation
                    Anim.GetLayer(3).SetWeight(1);
                    Anim.GetLayer(3).IsAdditive = true;
                    Anim.Play(Base.Land, 3);
                }
                */

                landed = true;
            }
            else if (!Networker.b1.IsGrounded && landed)
                landed = false;

            /*
            if (!Networker.b3.IsJumpDelay)
                Anim.GetLayer(3).SetWeight(0);
            */
        }

        [Header("Wall Jump")]
        public AudioClip WallJumpSFX;
        public AudioClip WallJumpFailSFX;
        public float WallJumpVolume = 0.5f;
        public GameObject WallJumpEffectBlue;
        public GameObject WallJumpEffectRed;
        public Transform WallJumpSpawn;
        public void DoWallJump(Vector3 point, Vector3 normal)
        {
            // Set our walljumping animation based on the angle we hit the wall
            Vector3 v = MoveDirectionXZ;
            v = Controller.transform.InverseTransformDirection(v);

            //Anim.CrossFade(Base.WallJumpFrozen, 0.15f);
            wallJumpX = v.x;

            // FX
            Quaternion rot = Quaternion.FromToRotation(Vector3.forward, normal);
            GameObject obj = Controller.Networker.nv_Team.Value != Networker.Instance.nv_Team.Value ? WallJumpEffectRed : WallJumpEffectBlue;
            obj = Lean.Pool.LeanPool.Spawn(obj, point, rot);
            obj.transform.localPosition += 0.1f * normal;
            Lean.Pool.LeanPool.Despawn(obj, 2.0f);

            if (Networker.nv_Stamina.Value >= Controller.Motor.Stamina.WallJumpStamina)
                Source.PlayOneShot(WallJumpSFX, WallJumpVolume);
            else
                Source.PlayOneShot(WallJumpFailSFX, WallJumpVolume);
        }

        [Header("Footstep")]
        public AudioSource FootstepSource;
        public float FootstepVolume = 0.5f;
        public LayerMask FootstepLayers;
        public float FootstepPerSecond = 0.18f;
        public float SprintFootstepPerSecond = 0.18f;

        private AudioClip currentClip;
        private int currentClipNum;
        private float lastFootstep;
        private float footstepPerSecond;
        private float footstepVolume;

        [Header("")]
        public AudioClip[] Ground;
        public AudioClip[] Concrete;
        public AudioClip[] Metal;
        public AudioClip[] Glass;
        public AudioClip[] Wood;
        public AudioClip[] Water;
        private void DoFootstep()
        {
            // Just to reset the clip number
            if (currentClipNum > 3)
                currentClipNum %= 3;

            // Setup some private variables to use later
            footstepPerSecond = Networker.b1.IsSprinting ? SprintFootstepPerSecond : FootstepPerSecond;
            footstepVolume = Networker.b1.IsSprinting || Networker.b1.Attacking ? FootstepVolume / 3 : FootstepVolume;

            if (Time.time >= lastFootstep + (footstepPerSecond))
            {
                RaycastHit hit;
                if (Physics.Raycast(transform.localPosition, Vector3.down, out hit, (Controller.Motor.Controller.height / 2) + Controller.Motor.Controller.radius, FootstepLayers))
                {
                    if (hit.collider.gameObject.HasTag("Concrete"))
                        currentClip = Concrete[currentClipNum];
                    else if (hit.collider.gameObject.HasTag("Metal"))
                        currentClip = Metal[currentClipNum];
                    else if (hit.collider.gameObject.HasTag("Glass"))
                        currentClip = Glass[currentClipNum];
                    else if (hit.collider.gameObject.HasTag("Wood"))
                        currentClip = Wood[currentClipNum];
                    else if (hit.collider.gameObject.HasTag("Water"))
                        currentClip = Water[currentClipNum];
                    else
                        currentClip = Ground[currentClipNum];
                }

                FootstepSource.PlayOneShot(currentClip, footstepVolume);
                currentClipNum++;
                lastFootstep = Time.time;
            }
        }

        [Header("Switch Weapon")]
        public AudioClip SwitchWeaponSFX;
        public float SwitchWeaponVolume = 0.5f;
        public void DoWeaponSwitch()
        {
            Source.PlayOneShot(SwitchWeaponSFX, SwitchWeaponVolume);
        }

        public void PlayShootSFX(AudioClip clip, float volume)
        {
            ShootSource.volume = volume;
            ShootSource.PlayOneShot(clip);
        }

        public void PlayReloadSFX(AudioClip clip, float volume)
        {
            ReloadSource.volume = volume;
            ReloadSource.clip = clip;
            ReloadSource.Play();
        }

        public void StopReloadSFX()
        {
            ReloadSource.Stop();
        }

        public void PlayMeleeSFX(AudioClip clip, float volume)
        {
            MeleeSource.volume = volume;
            MeleeSource.PlayOneShot(clip);
        }
        #endregion
    }

    [System.Serializable]
    public class BaseAnimations
    {
        [Header("")]
        public AnimationClip Land;

        [Header("")]
        public AnimationClip DodgeLeft;
        public AnimationClip DodgeRight;

        [Header("")]
        public MixerState.Serializable2D WallSliding;
        public AnimationClip WallJumpFrozen;
        public LinearMixerState.Serializable WallJumping;

        [Header("")]
        public AnimationClip KnockingBack;
        public AnimationClip WallSlammed;
        public AnimationClip PushingBack;
        public AnimationClip Flinching;
        public AnimationClip Stunned;
    }
}