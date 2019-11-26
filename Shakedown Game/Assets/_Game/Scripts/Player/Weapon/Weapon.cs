using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;

namespace Player
{
    public class Weapon : MonoBehaviour
    {
        public Controller Controller;
        public BasicMovementAttributes MovementAttributes;

        [Header("Attributes")]
        public int WeaponID;
        public bool IsRanged;
        public bool AimToCamera;    // Does the weapon aim to the camera like a gun when attacking
        public bool HasHeavyAttack;
        public bool HasChargeAttack;

        [Header("Damage")]
        public short HeadDamage;
        public short BodyDamage;
        public short ArmsDamage;
        public short LegsDamage;
        public short StructureDamage;

        [Header("Attack")]
        protected LayerMask HitLayers;
        // The current frame in the attack
        [HideInInspector] public int CurrentAttackFrame;
        // Whether or not we begun the first event in an attack
        [HideInInspector] public bool Attacked;
        // Whether or not we can cancel our attack
        [HideInInspector] public bool CanCancel;

        [Header("Ranged")]
        public int CurrentAmmo;
        public int AmmoPerClip;

        [Header("Skins")]
        public WeaponSkin ActiveSkin;
        public int SelectedSkinID;
        public List<GameObject> Skins;

        public virtual void Initialize(Controller controller)
        {
            Controller = controller;
            HitLayers = LayerMask.GetMask("Network Hitbox", "Default", "Structure");

            // Setup weapon skin
            foreach (GameObject obj in Skins)
            {
                if (obj.GetComponent<WeaponSkin>().SkinID == SelectedSkinID)
                {
                    GameObject skin = Instantiate(obj, Controller.transform);
                    WeaponSkin s = skin.GetComponent<WeaponSkin>();
                    ActiveSkin = s;

                    ActiveSkin.Initialize(Controller.Animator.ModelRefrences, controller.Networker.nv_Team.Value);
                }
            }
        }

        public virtual void UpdateWeapon()
        {
            if (!Attacked && (Controller.Input.Sprint || Controller.Input.DodgeLeft || Controller.Input.DodgeRight))
                CurrentAttackFrame = 0;
        }

        public virtual void StartAttack(int attackFrame, float stopAttackMoveTime = 0.0f)
        {
            Attacked = false;
            CanCancel = true;
            Controller.Motor.StopAttackMove(stopAttackMoveTime);

            CurrentAttackFrame = attackFrame;
            Controller.State.Attacking = true;
        }

        public void CastRay(Vector3 fromPosition, Vector3 direction, float distance)
        {
            RaycastHit hit;
            Ray ray = new Ray(fromPosition, direction);
            if (Physics.Raycast(ray, out hit, distance, HitLayers))
            {
                //Debug.DrawRay(fromPosition, direction * distance, Color.red, 2.0f);
                
                if (hit.transform.gameObject.GetComponent<Enviro.Structures.StructureObject>())
                {
                    DoStructureDamage(hit.transform.gameObject.GetComponent<Enviro.Structures.StructureObject>().ID);
                }
                else if (hit.transform.gameObject.GetComponent<Hitbox>())
                {
                    DoDamage(hit.transform.gameObject.GetComponent<Hitbox>());
                }

                // Do our our hit effects
                //CastRayEffects(hit.point);
                //Controller.Networker.networkObject.SendRpc("CastRayEffects", Receivers.Others, hit.point);
                OnEffects(hit.point);
            }
            else
            {
                // Do our our hit effects
                //CastRayEffects(fromPosition + direction * distance);
                //Controller.Networker.networkObject.SendRpc("CastRayEffects", Receivers.Others, fromPosition + direction * distance);
                OnEffects(fromPosition + direction * distance);
            }
        }

        public virtual void OnEffects(Vector3 point, Vector3 normal = new Vector3(), int attackNumber = 0)
        {
            CastEffects(point, normal, attackNumber);
            Controller.Networker.InvokeServerRpc("CastEffects_Server", point, normal, (byte)attackNumber, channel: "Reliable");
        }

        public virtual void CastEffects(Vector3 hitPoint, Vector3 normal, int attackNumber)
        {

        }

        public virtual void CastMeleeEffects(Vector3 hitPoint, Vector3 hitNormal, AttackEffect effect)
        {
            GameObject hitEffect = Lean.Pool.LeanPool.Spawn(effect.VFX, hitPoint, Quaternion.FromToRotation(effect.FromRotation, hitNormal));
            hitEffect.transform.localPosition += effect.BringFront * hitNormal;
            hitEffect.transform.localScale = new Vector3(effect.Scale, effect.Scale, effect.Scale);
            Lean.Pool.LeanPool.Despawn(hitEffect, 1.5f);

            AudioMixer mixer = Resources.Load<AudioMixer>("Main Mixer");
            AudioSource source = hitEffect.GetComponent<AudioSource>() == true ? hitEffect.GetComponent<AudioSource>() : hitEffect.AddComponent<AudioSource>();

            if (mixer == null || source == null)
                return;

            source.volume = effect.HitVolume;
            source.maxDistance = 30.0f;
            source.outputAudioMixerGroup = mixer.FindMatchingGroups("Sound Effects")[0];
            source.spatialBlend = Controller.Networker.IsOwner ? 0 : 1;
            source.PlayOneShot(effect.HitSFX);
        }

        /// <summary>
        /// A callback for when one of our hit detection functions hit an object
        /// </summary>
        /// <param name="hitType">0 = structure | 1 = player | 2 = anything else</param>
        /// <param name="col">The collider we hit</param>
        public virtual void OnMeleeHit(int hitType, Hitbox hitbox, RaycastHit hit)
        {
            OnEffects(hit.point, hit.normal, Controller.State.AttackNumber);
        }

        public virtual void OnMeleeHit(int hitType, RaycastHit hit)
        {
            OnEffects(hit.point, hit.normal, Controller.State.AttackNumber);
        }

        public virtual void HitPlayer()
        {
            Debug.Log("Hit Player");
        }

        public virtual void HitStructure()
        {
            Debug.Log("Hit Structure");
        }

        public virtual void HitObject()
        {
            Debug.Log("Hit Object");
        }

        public virtual void DoDamage(Hitbox hitbox)
        {
            short damage = 0;
            
            switch (hitbox.Type)
            {
                case Hitbox.HitboxType.Head:
                    damage = HeadDamage;
                    break;

                case Hitbox.HitboxType.Body:
                    damage = BodyDamage;
                    break;

                case Hitbox.HitboxType.Arms:
                    damage = ArmsDamage;
                    break;

                case Hitbox.HitboxType.Legs:
                    damage = LegsDamage;
                    break;
            }

            Controller.HUD.Combo.AddComboDamage(damage);
            hitbox.Controller.Networker.InvokeServerRpc("DoDamage", damage, channel: "Reliable");
        }

        /// <summary>
        /// Sends an RPC to the target player with the damage amount
        /// </summary>
        /// <param name="hitbox"></param>
        public void DoDamage(Hitbox hitbox, short damage)
        {
            Controller.HUD.Combo.AddComboDamage(damage);
            hitbox.Controller.Networker.InvokeServerRpc("DoDamage", damage, channel: "Reliable");
        }

        public virtual void DoStructureDamage(byte structureID)
        {
            Enviro.Structure.Instance.InvokeServerRpc("DoDamage", structureID, StructureDamage, channel: "Reliable");
        }

        public void DoStructureDamage(RaycastHit hit, short damage)
        {
            byte structureID = hit.collider.GetComponent<Enviro.Structures.StructureObject>().ID;
            Enviro.Structure.Instance.InvokeServerRpc("DoDamage", structureID, damage, channel: "Reliable");
        }

        public void DoKnockback(Hitbox hitbox, Vector3 direction, float speed, float length, bool wallBounce, float slamLength)
        {
            hitbox.Controller.Networker.InvokeServerRpc("Knockback_Server", direction, speed, length, wallBounce, slamLength, channel: "Reliable");
        }

        public void DoPushback(Hitbox hitbox, Vector3 direction, float speed, float length)
        {
            hitbox.Controller.Networker.InvokeServerRpc("Pushback_Server", direction, speed, length, channel: "Reliable");
        }

        public void DoFlinch(Hitbox hitbox, float length)
        {
            hitbox.Controller.Networker.InvokeServerRpc("Flinch_Server", length, channel: "Reliable");
        }

        public void DoStun(Hitbox hitbox, float length)
        {
            hitbox.Controller.Networker.InvokeServerRpc("Stun_Server", length, channel: "Reliable");
        }

        public virtual void Reload()
        {

        }

        public virtual void Disable()
        {
            // Disable our game object
            gameObject.SetActive(false);

            // Disable our third person objects
            ActiveSkin.gameObject.SetActive(false);
        }

        public virtual void Enable()
        {
            // Enable our game object
            gameObject.SetActive(true);

            // Enable our third person objects
            ActiveSkin.gameObject.SetActive(true);

            Controller.Animator.Weapon = ActiveSkin.Animations;

            // If we have no bullets, and we switch to this weapon, we should automatically reload
            if (CurrentAmmo <= 0)
                Reload();
        }
    }

    [System.Serializable]
    public class AttackEffect
    {
        public string Name;

        [Header("Visual")]
        public GameObject VFX;
        public Vector3 FromRotation = Vector3.forward;
        public float BringFront = 0.3f;
        public float Scale = 0.5f;

        [Header("Audio")]
        public AudioClip[] AttackSFX;
        public float AttackVolume = 0.5f;
        public AudioClip HitSFX;
        public float HitVolume = 0.5f;
    }
}