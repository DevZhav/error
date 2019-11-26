using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player.Weapons
{
    public class HeavyShotgun : Weapon
    {
        [Header("Attributes")]
        public BulletPattern BulletPattern;
        public float AttackLength = 0.3f;
        public float CooldownLength = 1.0f;
        public float ReloadLength = 3.0f;
        public float BulletSpread = 0.01f;
        public float BulletDistance = 40.0f;

        private float attackTimer;
        private float cooldownTimer;
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
            if (Controller.Input.HoldLeftFire && attackTimer <= 0 && cooldownTimer <= 0 && !Controller.State.Reloading && CurrentAmmo > 0)
            {
                CurrentAmmo -= 1;
                StartCoroutine(ShootWithDelay(0.05f));
            }
            else
            {
                attackTimer -= Time.deltaTime;
                cooldownTimer -= Time.deltaTime;
            }

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

        public override void Reload()
        {
            base.Reload();

            Controller.State.Reloading = true;
            Controller.Input.Sprint = false;    // Stop sprinting
            reloadTimer = ReloadLength;
        }

        private IEnumerator ShootWithDelay(float delay)
        {
            Controller.State.Attacking = true;
            attackTimer = AttackLength + delay;
            cooldownTimer = CooldownLength;

            yield return new WaitForSeconds(delay);

            for (int i = 0; i < BulletPattern.Pattern.Length; i++)
            {
                Vector3 bloom = Controller.Camera.transform.TransformDirection(new Vector3(BulletSpread * BulletPattern.Pattern[i].x, BulletSpread * BulletPattern.Pattern[i].y, BulletDistance * 1));
                //Vector3 spread = (Controller.Camera.Cam.transform.forward + ).normalized;
                CastRay(Controller.Camera.Cam.transform.position, bloom, BulletDistance);
            }

            Controller.State.Aiming = false;
            attackTimer = AttackLength;
        }

        public override void DoDamage(Hitbox hitbox)
        {
            base.DoDamage(hitbox);

            Vector3 pushbackDirection = Controller.transform.TransformDirection(new Vector3(0, 0, 1.0f));
            DoPushback(hitbox, pushbackDirection, 12.0f, 0.1f);
        }

        public override void CastEffects(Vector3 hitPoint, Vector3 hitNormal, int attackNumber)
        {
            GameObject trail = Lean.Pool.LeanPool.Spawn(ActiveSkin.AttackEffects[0].VFX, ActiveSkin.MuzzleFlashPosition.position, Quaternion.identity);
            LineRenderer tr = trail.GetComponent<LineRenderer>();

            tr.SetPosition(0, ActiveSkin.MuzzleFlashPosition.position);
            tr.SetPosition(1, hitPoint);

            Lean.Pool.LeanPool.Despawn(trail, 1.0f);
        }

        public override void Enable()
        {
            base.Enable();
        }
    }
}