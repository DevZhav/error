using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player.Weapons
{
    public class CombatBow : Weapon
    {
        [Header("Attributes")]
        public float AttackLength = 1.0f;
        public float MaxChargeLength = 1.0f;
        public float ChargeSpeed = 0.1f;

        private float chargedAmount;
        private float attackTimer;

        public override void Initialize(Controller controller)
        {
            base.Initialize(controller);
        }

        public override void UpdateWeapon()
        {
            if (attackTimer <= 0.0f)
                Controller.State.Attacking = false;

            if (Controller.Input.HoldLeftFire && attackTimer <= 0)
            {
                // If we're holding left click, we want to increase the charge amount over time
                chargedAmount += ChargeSpeed * Time.deltaTime;
                chargedAmount = Mathf.Clamp(chargedAmount, 0, MaxChargeLength);
                Debug.Log(chargedAmount);
                // We also want to set the aiming state
                Controller.State.Aiming = true;
            }
            else if (chargedAmount <= 0.15f && chargedAmount > 0 && attackTimer <= 0)
            {
                StartCoroutine(ShootWithDelay(0.15f));
            }
            else if (chargedAmount > 0.15f && attackTimer <= 0)
            {
                StartCoroutine(ShootWithDelay(0.0f));
            }
            else
                attackTimer -= Time.deltaTime;
        }

        private IEnumerator ShootWithDelay(float delay)
        {
            Controller.State.Attacking = true;
            attackTimer = AttackLength + delay;

            yield return new WaitForSeconds(delay);

            CastRay(Controller.Camera.Cam.transform.position, Controller.Camera.transform.forward, Mathf.Infinity);
            attackTimer = AttackLength;
            chargedAmount = 0;
            Controller.State.Aiming = false;
        }

        public override void CastEffects(Vector3 hitPoint, Vector3 hitNormal, int attackNumber)
        {
            GameObject trail = Lean.Pool.LeanPool.Spawn(ActiveSkin.AttackEffects[0].VFX, ActiveSkin.MuzzleFlashPosition.position, Quaternion.identity);
            LineRenderer tr = trail.GetComponent<LineRenderer>();

            tr.SetPosition(0, ActiveSkin.MuzzleFlashPosition.position);
            tr.SetPosition(1, hitPoint);

            Lean.Pool.LeanPool.Despawn(trail, 1.0f);
        }

        public override void DoDamage(Hitbox hitbox)
        {
            short damage = (short)(BodyDamage * chargedAmount);
            DoDamage(hitbox, damage);
            Controller.HUD.Combo.AddComboDamage(damage);
        }
    }
}