using MLAPI;
using MLAPI.NetworkedVar;
using MLAPI.Messaging;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MLAPI.Spawning;

namespace Game.Objects
{
    public class Ball : NetworkedBehaviour
    {
        // BALL STATUS
        // 0 = Neutral
        // 1 = Dropped
        // 2 = Reset
        // 3 = Touchdown / Freeze
        // 4 = Picked up

        public static Ball Instance;

        static NetworkedVarSettings netSettingsReliable = new NetworkedVarSettings() { ReadPermission = NetworkedVarPermission.Everyone, WritePermission = NetworkedVarPermission.ServerOnly, SendChannel = "Reliable", SendTickrate = 20f };
        static NetworkedVarSettings netSettingsUnreliable = new NetworkedVarSettings() { ReadPermission = NetworkedVarPermission.Everyone, WritePermission = NetworkedVarPermission.ServerOnly, SendChannel = "Unreliable", SendTickrate = 20f };
        // --- NETWORKED VARS --- //
        public NetworkedVarVector3 nv_Position = new NetworkedVarVector3(netSettingsUnreliable);
        public NetworkedVarULong nv_BallOwner = new NetworkedVarULong(netSettingsReliable); // ulong = clientID
        public NetworkedVarByte nv_BallStatus = new NetworkedVarByte(netSettingsReliable);
        // --- NETWORKED VARS --- //

        [Header("Ball Attributes")]
        public LayerMask PlayerLayer;   // The layers that we should check for player collision

        public Player.Controller LocalOwner;
        private Transform ownerBallParentPosition;
        private bool parented;

        private void Start()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);

            nv_BallStatus.OnValueChanged += BallStatusChanged;
        }

        private void BallStatusChanged(byte previousStatus, byte status)
        {
            // If neutral
            if (status == 0)
            {
                parented = false;
                LocalOwner = null;
            }
            // If dropped
            else if (status == 1)
            {
                parented = false;
                LocalOwner = null;
            }
            // If reset
            else if (status == 2)
            {
                parented = false;
                LocalOwner = null;
            }
            // If touchdown
            else if (status == 3)
            {
                // Set the player's camera to the position of the player who touchdown
                Player.Networker.Instance.Controller.Camera.FollowPosition = LocalOwner.CameraPosition;

                //parented = false;
                //LocalOwner = null;

                parented = true;
                //LocalOwner = SpawnManager.GetPlayerObject(nv_BallOwner.Value).GetComponent<Player.Networker>().Controller;
                LocalOwner = PlayerMethods.GetPlayerObjectByOwnerID(nv_BallOwner.Value).GetComponent<Player.Networker>().Controller;
                ownerBallParentPosition = LocalOwner.BallParentPosition;
            }
            // If picked up
            else if (status == 4)
            {
                parented = true;
                //LocalOwner = SpawnManager.GetPlayerObject(nv_BallOwner.Value).GetComponent<Player.Networker>().Controller;
                LocalOwner = PlayerMethods.GetPlayerObjectByOwnerID(nv_BallOwner.Value).GetComponent<Player.Networker>().Controller;
                ownerBallParentPosition = LocalOwner.BallParentPosition;
            }
        }

        private void Update()
        {
            if (nv_BallStatus.Value != 3 && nv_BallStatus.Value != 4)
            {
                // We need to lerp from transform.position to newPosition
                transform.position = Vector3.Lerp(transform.position, nv_Position.Value, 10 * Time.deltaTime);
            }
            else if (nv_BallStatus.Value == 4 && ownerBallParentPosition != null)
            {
                transform.position = ownerBallParentPosition.position;
            }
        }

        private void FixedUpdate()
        {
            if (LocalOwner != null && LocalOwner == Player.Networker.Instance.Controller)
            {
                LocalOwner.State.Stamina -= 9 * Time.deltaTime;
            }
        }

        private void OnTriggerEnter(Collider col)
        {
            if (nv_BallOwner.Value == 0 && nv_BallStatus.Value != 3)
            {
                if (col.GetComponent<Player.Controller>())
                {
                    var serializer = col.GetComponent<Player.Controller>();

                    if (serializer == Player.Networker.Instance.Controller)
                    {
                        if (serializer.Networker.nv_Health.Value > 0 && serializer.Networker.Initialized)
                        {
                            Debug.Log("Attempted to grab the ball");
                            InvokeServerRpc("GrabBall", channel: "Reliable");
                        }
                    }
                }
            }
        }

        /*
        public override void DropBall(RpcArgs args)
        {

        }

        public override void GrabBall(RpcArgs args)
        {

        }

        public override void ResetBall(RpcArgs args)
        {

        }

        public override void Score(RpcArgs args)
        {

        }
        */
    }
}