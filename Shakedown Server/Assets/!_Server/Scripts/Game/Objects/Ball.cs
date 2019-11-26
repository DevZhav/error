using MLAPI;
using MLAPI.Messaging;
using MLAPI.NetworkedVar;
using MLAPI.Spawning;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Game.Objects
{
    public class Ball : NetworkedBehaviour
    {
        public static Ball Instance;
        // BALL STATUS
        // 0 = Neutral
        // 1 = Dropped
        // 2 = Reset
        // 3 = Touchdown / Freeze
        // 4 = Picked up

        [Header("Gravity")]
        public LayerMask GroundLayers;  // The layers that the ball will detect as ground
        public float Distance = 1.25f;  // How far below the ball should we check for a ground?
        public float FallSpeed = 6.0f;  // How fast should the ball move toward the ground?

        [Header("Reset")]
        public float ResetTime = 6.0f;
        private bool shouldReset;       // Should the ball check to reset?
        private float resetTimer;

        public Player LocalBallOwner;

        static NetworkedVarSettings netSettingsReliable = new NetworkedVarSettings() { ReadPermission = NetworkedVarPermission.Everyone, WritePermission = NetworkedVarPermission.ServerOnly, SendChannel = "Reliable", SendTickrate = 20f };
        static NetworkedVarSettings netSettingsUnreliable = new NetworkedVarSettings() { ReadPermission = NetworkedVarPermission.Everyone, WritePermission = NetworkedVarPermission.ServerOnly, SendChannel = "Unreliable", SendTickrate = 20f };
        // --- NETWORKED VARS --- //
        public NetworkedVarVector3 nv_Position = new NetworkedVarVector3(netSettingsUnreliable);
        public NetworkedVarULong nv_BallOwner = new NetworkedVarULong(netSettingsReliable); // ulong = clientID
        public NetworkedVarByte nv_BallStatus = new NetworkedVarByte(netSettingsReliable);
        // --- NETWORKED VARS --- //

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        private void FixedUpdate()
        {
            // If someone isn't holding the ball, and it hasn't been scored
            // Then apply gravity and check for reset
            if (nv_BallStatus.Value != 3 && nv_BallStatus.Value != 4)
            {
                SimulateGravity();
                CheckReset();
            }
            else if (nv_BallStatus.Value == 4)
            {
                transform.position = LocalBallOwner.ReceivedPosition;
                nv_Position.Value = transform.position;
            }

            if (LocalBallOwner == null && nv_BallStatus.Value == 4)
                LocalDropBall();
        }

        private void SimulateGravity()
        {
            RaycastHit hit;
            // If there's no ground below
            if (!Physics.Raycast(transform.position, -transform.up, out hit, Distance, GroundLayers))
            {
                Vector3 newPositon = transform.position;
                newPositon.y -= FallSpeed * Time.deltaTime;

                transform.position = newPositon;
                nv_Position.Value = transform.position;
            }
            else
            {
                if (hit.transform.tag == "Ground Killer")
                {
                    ResetBall();
                }
            }
        }

        private void CheckReset()
        {
            if (shouldReset)
            {
                if (resetTimer >= ResetTime)
                {
                    LocalResetBall();
                }

                resetTimer += Time.deltaTime;
            }
        }

        public void LocalResetBall()
        {
            nv_BallStatus.Value = 2;
            nv_BallOwner.Value = 0;
            LocalBallOwner = null;

            // We shouldn't check to reset
            shouldReset = false;
            resetTimer = 0;

            transform.position = GameObject.FindGameObjectWithTag("Ball Spawn").transform.position;
        }

        public void LocalDropBall()
        {
            nv_BallStatus.Value = 1;
            nv_BallOwner.Value = 0;
            LocalBallOwner = null;

            // We should check to reset
            shouldReset = true;
            resetTimer = 0;
        }

        public void LocalScore(byte team)
        {
            nv_BallStatus.Value = 3;
            //networkObject.BallOwner = 0;
            //localBallOwner = null;

            // We shouldn't check to reset
            shouldReset = false;
            resetTimer = 0;

            // Add to our score
            if (team == 1)
                Game.Instance.nv_AlphaScore.Value += 1;
            else if (team == 2)
                Game.Instance.nv_BetaScore.Value += 1;

            // End the round
            Game.Instance.GetComponent<Touchdown>().EndRound();

            // Do some checks, like if the game should end
            // or if it's half time, etc.
            Game.Instance.DoChecks();
        }

        [ServerRPC(RequireOwnership = false)]
        public void GrabBall()
        {
            if (nv_BallStatus.Value == 3 || nv_BallStatus.Value == 4)
            {
                Debug.Log("Someone tried to grab the ball in the wrong state. State: " + nv_BallStatus.Value);
                return;
            }

            var p = SpawnManager.GetPlayerObject(ExecutingRpcSender);
            Player player = p.GetComponent<Player>();

            if (player.nv_Health.Value <= 0)
                return;

            Debug.Log("Ball grabbed from player");
            nv_BallStatus.Value = 4;
            nv_BallOwner.Value = ExecutingRpcSender;
            LocalBallOwner = player;

            // We shouldn't check to reset
            shouldReset = false;
            resetTimer = 0;
        }

        /*
        public override void GrabBall(RpcArgs args)
        {
            MainThreadManager.Run(() =>
            {
                if (networkObject.BallStatus == 3 || networkObject.BallStatus == 4)
                {
                    Debug.Log("Someone tried to grab the ball in the wrong state. State: " + networkObject.BallStatus);
                    return;
                }

                // Get all of the player objects
                Player[] p = FindObjectsOfType<Player>();
                for (int i = 0; i < p.Length; i++)
                {
                    // If it's not the player who requested the grab, we skip to the next iteration
                    if (p[i].networkObject.Owner.NetworkId != args.Info.SendingPlayer.NetworkId)
                        continue;
                    // If the player isn't even alive, just break off
                    if (p[i].networkObject.Health <= 0)
                        break;

                    Debug.Log("Ball grabbed from player");
                    networkObject.BallStatus = 4;
                    networkObject.BallOwner = p[i].networkObject.NetworkId;
                    localBallOwner = p[i];

                    // We shouldn't check to reset
                    shouldReset = false;
                    resetTimer = 0;

                    break;
                }
            });
        }
        */

        [ServerRPC(RequireOwnership = false)]
        public void ResetBall()
        {
            if (nv_BallStatus.Value == 3)
            {
                Debug.Log("Someone tried to reset the ball while it was scored");
                return;
            }

            var p = SpawnManager.GetPlayerObject(ExecutingRpcSender);
            Player player = p.GetComponent<Player>();

            if (p.OwnerClientId != nv_BallOwner.Value)
                return;

            Debug.Log("Ball reset from player");
            LocalResetBall();
        }

        /*
        public override void ResetBall(RpcArgs args)
        {
            MainThreadManager.Run(() =>
            {   
                if (networkObject.BallStatus == 3)
                {
                    Debug.Log("Someone tried to reset the ball while it was scored");
                    return;
                }

                // Get all of the player objects
                Player[] p = FindObjectsOfType<Player>();
                for (int i = 0; i < p.Length; i++)
                {
                    // If it's not the player who requested the grab, we skip to the next iteration
                    if (p[i].networkObject.Owner.NetworkId != args.Info.SendingPlayer.NetworkId)
                        continue;

                    Debug.Log("Ball reset from player");
                    // And reset the ball
                    ResetBall();
                    break;
                }
            });
        }
        */

        [ServerRPC(RequireOwnership = false)]
        public void DropBall()
        {
            var p = SpawnManager.GetPlayerObject(ExecutingRpcSender);
            Player player = p.GetComponent<Player>();

            if (p.OwnerClientId != nv_BallOwner.Value)
                return;

            Debug.Log("Ball dropped from player");
            LocalDropBall();
        }

        /*
        public override void DropBall(RpcArgs args)
        {
            Debug.Log("Someone tried to drop the ball");
            MainThreadManager.Run(() =>
            {
                // Get all of the player objects
                Player[] p = FindObjectsOfType<Player>();
                for (int i = 0; i < p.Length; i++)
                {
                    // If it's not the player who requested the grab, we skip to the next iteration
                    if (p[i].networkObject.Owner.NetworkId != args.Info.SendingPlayer.NetworkId)
                        continue;

                    Debug.Log("Ball dropped from player");
                    // And reset the ball
                    DropBall();
                    break;
                }
            });
        }
        */

        [ServerRPC(RequireOwnership = false)]
        public void Score()
        {
            if (nv_BallStatus.Value == 3)
            {
                Debug.Log("Someone tried to score when the round was over");
                return;
            }

            var p = SpawnManager.GetPlayerObject(ExecutingRpcSender);
            Player player = p.GetComponent<Player>();

            if (p.OwnerClientId != nv_BallOwner.Value)
                return;

            Debug.Log("Score from player");
            // Add a score to the player
            player.nv_Score_Score.Value += 1;
            // Add a global score to the team
            LocalScore(player.nv_Team.Value);
        }

        /*
        public override void Score(RpcArgs args)
        {
            MainThreadManager.Run(() =>
            {
                if (networkObject.BallStatus == 3)
                {
                    Debug.Log("Someone tried to score when the round was over");
                    return;
                }

                // Get all of the player objects
                Player[] p = FindObjectsOfType<Player>();
                for (int i = 0; i < p.Length; i++)
                {
                    // If it's not the player who requested the grab, we skip to the next iteration
                    if (p[i].networkObject.Owner.NetworkId != args.Info.SendingPlayer.NetworkId)
                        continue;

                    Debug.Log("Score from player");
                    // Add to the scoring player's score
                    p[i].Avatar.Score += 1;
                    // And reset the ball
                    Score(p[i].Avatar.Team);
                    break;
                }
            });
        }
        */
    }
}