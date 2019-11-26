using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Objects
{
    public class TouchdownPlate : MonoBehaviour
    {
        public enum Team
        {
            Alpha = 1,
            Beta = 2
        }
        public Team TeamPlate;

        private void OnTriggerEnter(Collider col)
        {
            if (col.GetComponent<Player.Controller>())
            {
                Player.Controller c = col.GetComponent<Player.Controller>();

                var controller = col.GetComponent<Player.Controller>();
                if (controller == null)
                    return;

                if (!controller.Networker.IsOwner)
                    return;

                if (Ball.Instance.LocalOwner == controller)
                {
                    Debug.Log("Scoring ball");
                    Ball.Instance.InvokeServerRpc("Score", channel: "Reliable");
                }
            }
        }
    }
}