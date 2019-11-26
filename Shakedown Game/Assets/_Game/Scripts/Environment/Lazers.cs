using UnityEngine;

namespace Enviro
{
    public class Lazers : MonoBehaviour
    {
        [Tooltip("1 = Red\n2 = Blue")]
        public int TeamLazer = 0;

        private void OnTriggerEnter(Collider col)
        {
            var controller = col.GetComponent<Player.Controller>();
            if (controller == null)
                return;

            if (!controller.Networker.IsOwner)
                return;

            //if (controller.state.HasBall)
            //    Game.GameModeManager.Instance.TouchdownBall.ResetBall();

            if (Game.Objects.Ball.Instance.LocalOwner == controller)
            {
                Debug.Log("Resetting ball");
                Game.Objects.Ball.Instance.InvokeServerRpc("ResetBall", channel: "Reliable");
            }

            if (Player.Networker.Instance.nv_Team.Value != TeamLazer)
                controller.Networker.InvokeServerRpc("DoDamage", (short)255, channel: "Reliable");
        }
    }
}