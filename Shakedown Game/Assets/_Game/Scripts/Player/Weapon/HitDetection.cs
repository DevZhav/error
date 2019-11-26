using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Player.Weapons
{
    public class HitDetection : MonoBehaviour
    {
        private Weapon Weapon;
        public float ObjectRaycastLength;
        private LayerMask HitLayers;

        private void Awake()
        {
            Weapon = GetComponentInParent<Weapon>();
            HitLayers = LayerMask.GetMask("Default", "Structure", "Network Hitbox");
        }

        private void OnTriggerEnter(Collider col)
        {
            // If we hit a collider and it's either the front melee or back melee collider
            if (col.GetComponent<Hitbox>() && (col.GetComponent<Hitbox>().Type == Hitbox.HitboxType.FrontMelee || col.GetComponent<Hitbox>().Type == Hitbox.HitboxType.BackMelee))
            {
                Debug.Log("Hit a melee collider");
                // We cast a ray from the hitbox position to the point to make sure there's no obstacles in the way
                RaycastHit hit;
                if (Physics.Raycast(transform.position, col.transform.position - transform.position, out hit, ObjectRaycastLength * 2, HitLayers))
                {
                    Debug.Log("We hit some collider after checking");
                    // So only if we hit a hitbox
                    if (hit.collider.GetComponent<Hitbox>())
                    {
                        Debug.Log("Yup, definitely not through a wall");
                        // Send our weapon the player that we hit
                        Weapon.OnMeleeHit(1, col.GetComponent<Hitbox>(), hit);
                    }
                }
            }
            // If we hit a defensive shield
            else if (col.GetComponent<Hitbox>() && col.GetComponent<Hitbox>().Type == Hitbox.HitboxType.Shield)
            {
                // We should flinch ourself
                Weapon.Controller.Motor.DoFlinch(0.1f);
            }
        }

        public void CastRay()
        {
            RaycastHit hit;
            if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.up), out hit, ObjectRaycastLength, HitLayers))
            {
                if (hit.collider.GetComponent<Hitbox>())
                {
                    // Do nothing
                }
                else if (hit.collider.GetComponent<Enviro.Structures.StructureObject>())
                {
                    // Send our weapon the structure that we hit
                    Weapon.OnMeleeHit(0, hit);
                }
                else
                {
                    // Send our weapon the object that we hit
                    Weapon.OnMeleeHit(2, hit);
                }
            }
        }

        private void OnDrawGizmos()
        {
            Gizmos.DrawLine(transform.position, transform.position + transform.up * ObjectRaycastLength);
        }
    }
}