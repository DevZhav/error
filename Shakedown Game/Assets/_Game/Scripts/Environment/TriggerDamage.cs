using UnityEngine;

namespace Enviro
{
    public class TriggerDamage : MonoBehaviour
    {
        public short Damage;

        [Header("Effects")]
        public GameObject HitParticleEffect;

        private void OnTriggerEnter(Collider col)
        {
            var controller = col.GetComponent<Player.Controller>();
            if (controller == null)
                return;

            if (HitParticleEffect != null)
            {
                GameObject obj = Lean.Pool.LeanPool.Spawn(HitParticleEffect, controller.transform.position, Quaternion.Euler(0, 0, 0));
                Lean.Pool.LeanPool.Despawn(obj, 0.5f);
            }

            if (!controller.Networker.IsOwner)
                return;

            if (Game.Objects.Ball.Instance.LocalOwner == controller)
            {
                Debug.Log("Resetting ball");
                Game.Objects.Ball.Instance.InvokeServerRpc("ResetBall", channel: "Reliable");
            }

            controller.Networker.InvokeServerRpc("DoDamage", Damage, channel: "Reliable");
        }
    }
}