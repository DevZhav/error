using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player.Weapons
{
    public class AssaultRifle : Weapon
    {
        [Header("Attributes")]
        public float AttackLength;
        public float AttackDistance;
        public float AttackSpread;

        [Header("")]
        public float ReloadLength;

        private float attackTimer;
        private float reloadTimer;

        public override void Initialize(Controller controller)
        {
            base.Initialize(controller);
            CurrentAmmo = AmmoPerClip;
        }

        public override void UpdateWeapon()
        {
            if (attackTimer <= 0.0f)
                Controller.State.Attacking = false;

            if (reloadTimer <= 0)
            {
                if (Controller.State.Reloading)
                {
                    CurrentAmmo = AmmoPerClip;
                    Controller.State.Reloading = false;
                }
                else
                    Controller.State.Reloading = false;
            }

            // If we left click, we want to fire our weapon
            if (Controller.Input.HoldLeftFire && attackTimer <= 0 && !Controller.State.Reloading && CurrentAmmo > 0)
            {
                Controller.State.AttackNumber = 0;
                MovementAttributes.AttackSpeed = 4f;
                Controller.State.Attacking = true;
                attackTimer = AttackLength;

                Vector3 spread = (Controller.Camera.Cam.transform.forward + Random.insideUnitSphere * AttackSpread).normalized;
                CastRay(Controller.Camera.Cam.transform.position, spread, AttackDistance);

                CurrentAmmo -= 1;
                attackTimer = AttackLength;
            }
            else
                attackTimer -= Time.deltaTime;

            // Reload the gun
            if ((Controller.Input.Reload && !Controller.State.Reloading && CurrentAmmo != AmmoPerClip) || (CurrentAmmo <= 0 && !Controller.State.Reloading))
            {
                Reload();
            }
            //else if (Controller.Input.Reload && Controller.State.Reloading)
            //{
            //    Debug.Log("Cancelled Reloading Heavy Shotgun");
            //    Controller.State.Reloading = false;
            //}
            else
                reloadTimer -= Time.deltaTime;
        }

        public override void DoDamage(Hitbox hitbox)
        {
            base.DoDamage(hitbox);
            Controller.HUD.Combo.AddComboDamage(BodyDamage);
        }

        public override void Reload()
        {
            base.Reload();

            Controller.State.Reloading = true;
            Controller.Input.Sprint = false;    // Stop sprinting
            reloadTimer = ReloadLength;
        }

        public override void CastEffects(Vector3 hitPoint, Vector3 hitNormal, int attackNumber)
        {
            // play sfx
            //ActiveSkin.GetShootSFX();

            GameObject trail = Lean.Pool.LeanPool.Spawn(ActiveSkin.AttackEffects[0].VFX, ActiveSkin.MuzzleFlashPosition.position, Quaternion.identity);
            LineRenderer tr = trail.GetComponent<LineRenderer>();

            tr.SetPosition(0, ActiveSkin.MuzzleFlashPosition.position);
            tr.SetPosition(1, hitPoint);

            Lean.Pool.LeanPool.Despawn(trail, 1.0f);
        }
    }
}