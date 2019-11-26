using UnityEngine;
using System.Collections.Generic;
using System;
using MLAPI;
using MLAPI.NetworkedVar;
using MLAPI.Messaging;
using MLAPI.Spawning;

namespace Game
{
    public class Game : NetworkedBehaviour
    {
        public enum GameModes
        {
            TeamDeathMatch = 0,
            Touchdown = 1,
            TeamSmash = 2
        }
        public enum TimeLimits
        {
            m5 = 5,
            m10 = 10,
            m15 = 15,
            m20 = 20,
            m30 = 30,
        }
        public enum PlayerLimits
        {
            p4 = 4,
            p6 = 6,
            p8 = 8,
            p10 = 10,
            p12 = 12
        }

        public GameModes GameMode;
        public TimeLimits TimeLimit;
        public PlayerLimits PlayerLimit;
        public float RespawnTime = 6;

        [Header("Spawn")]
        public bool UseChildSpawns;
        private Transform AlphaSpawn;
        private Transform BetaSpawn;

        [HideInInspector] public float StartTime;
        [HideInInspector] public byte MaxScore;
        public float CurrentTime
        {
            get
            {
                return ((float)TimeLimit * 60) - (Time.time - StartTime);
            }
        }

        public static Game Instance = null;

        static NetworkedVarSettings netSettingsSyncOnce = new NetworkedVarSettings() { ReadPermission = NetworkedVarPermission.Everyone, WritePermission = NetworkedVarPermission.ServerOnly, SendChannel = "Reliable", SendTickrate = 1f };
        static NetworkedVarSettings netSettingsReliable = new NetworkedVarSettings() { ReadPermission = NetworkedVarPermission.Everyone, WritePermission = NetworkedVarPermission.ServerOnly, SendChannel = "Reliable", SendTickrate = 15f };
        static NetworkedVarSettings netSettingsUnreliable = new NetworkedVarSettings() { ReadPermission = NetworkedVarPermission.Everyone, WritePermission = NetworkedVarPermission.ServerOnly, SendChannel = "Unreliable", SendTickrate = 20f };
        // --- NETWORKED VARS --- //
        public NetworkedVarByte nv_GameMode = new NetworkedVarByte(netSettingsSyncOnce);
        public NetworkedVarByte nv_GameLength = new NetworkedVarByte(netSettingsSyncOnce);
        public NetworkedVarByte nv_MaxScore = new NetworkedVarByte(netSettingsSyncOnce);
        public NetworkedVarByte nv_AlphaScore = new NetworkedVarByte(netSettingsReliable);
        public NetworkedVarByte nv_BetaScore = new NetworkedVarByte(netSettingsReliable);

        public NetworkedVarBool nv_InProgress = new NetworkedVarBool(netSettingsReliable);
        public NetworkedVarFloat nv_CurrentTime = new NetworkedVarFloat(netSettingsUnreliable);
        public NetworkedVarFloat nv_PauseTimeLeft = new NetworkedVarFloat(netSettingsUnreliable);
        // --- NETWORKED VARS --- //

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        private void Start()
        {
            // Remove this boiler plate later
            StartMatch();

            // Instantiate the structure object
            GameObject structure = Instantiate(HeadlessStart.Instance.StructurePrefab);
            structure.GetComponent<NetworkedObject>().Spawn();

            // Get Spawn Positions
            AlphaSpawn = GameObject.FindGameObjectWithTag("Alpha Spawn").transform;
            BetaSpawn = GameObject.FindGameObjectWithTag("Beta Spawn").transform;

            // On player connected
            NetworkingManager.Singleton.OnClientConnectedCallback += (clientID) =>
            {
                Debug.Log("Client Connected");
                Player p = SpawnManager.GetPlayerObject(clientID).GetComponent<Player>();
            };
            NetworkingManager.Singleton.OnClientDisconnectCallback += (clientID) =>
            {
                Debug.Log("Client Disconnected");
            };

            /*
            NetworkManager.Instance.Networker.playerAuthenticated += (player, sender) =>
            {
                MainThreadManager.Run(() =>
                {
                    Debug.Log("Player connected... Spawning");

                    // Update all of the structures so things are in sync
                    Structure.Instance.UpdateAllStructures();

                    var a = NetworkManager.Instance.InstantiateAvatar();
                    a.networkStarted += (networkBehavior) =>
                    {

                    };

                    var p = NetworkManager.Instance.InstantiatePlayer();
                    p.networkStarted += (networkBehavior) =>
                    {
                        p.networkObject.AssignOwnership(player);

                        int userID = 0;
                        byte gameAccess = 1;

                        uint networkObjectOwnerID = p.networkObject.NetworkId;
                        byte team = (byte)UnityEngine.Random.Range(1, 3);
                        string name = "test " + UnityEngine.Random.Range(0, 9999);

                        byte[] equippedWeaponIDs = new byte[3];
                        equippedWeaponIDs[0] = 4;
                        equippedWeaponIDs[1] = 2;
                        equippedWeaponIDs[2] = 3;
                        byte[] equippedItemIDs = new byte[0];

                        if (!HeadlessStart.Instance.IsTestServer)
                        {
                            StartCoroutine(DB_API.Server_Match_User_Authorized(p.networkObject.Owner.MatchID, p.networkObject.Owner.ID, callback =>
                            {
                                // Set the player's avatar
                                userID = callback.Account.ID;
                                gameAccess = (byte)callback.Account.GameAccess;

                                networkObjectOwnerID = p.networkObject.NetworkId;
                                team = (byte)callback.Match.TeamID;
                                name = callback.Account.Name;

                                equippedWeaponIDs = new byte[3];
                                equippedWeaponIDs[0] = 4;
                                equippedWeaponIDs[1] = 2;
                                equippedWeaponIDs[2] = 3;
                                equippedItemIDs = new byte[0];

                                SetupPlayer(player, p, a, userID, gameAccess, team, name, equippedWeaponIDs, equippedItemIDs, networkObjectOwnerID);
                            }));
                        }

                        SetupPlayer(player, p, a, userID, gameAccess, team, name, equippedWeaponIDs, equippedItemIDs, networkObjectOwnerID);
                    };
                });
            };
            */

            // On player disconnected
            /*
            NetworkManager.Instance.Networker.playerDisconnected += (player, sender) =>
            {
                MainThreadManager.Run(() =>
                {
                    Debug.Log("Player left... Removing network objects");

                    //Loop through all players and find the player who disconnected, store all it's networkobjects to a list
                    List<NetworkObject> toDelete = new List<NetworkObject>();
                    foreach (var no in sender.NetworkObjectList)
                    {
                        if (no.Owner == player)
                        {
                            //Found him
                            toDelete.Add(no);
                        }
                    }

                    //Remove the actual network object outside of the foreach loop, as we would modify the collection at runtime elsewise. (could also use a return, too late)
                    if (toDelete.Count > 0)
                    {
                        for (int i = toDelete.Count - 1; i >= 0; i--)
                        {
                            if ((PlayerBehavior)toDelete[i].AttachedBehavior)
                                ((PlayerBehavior)toDelete[i].AttachedBehavior).GetComponent<Player>().Avatar.networkObject.Destroy();

                            sender.NetworkObjectList.Remove(toDelete[i]);
                            toDelete[i].Destroy();
                        }
                    }
                });
            };
            */
        }

        /*
        private void SetupPlayer(NetworkingPlayer player, PlayerBehavior p, AvatarBehavior a, int userID, byte gameAccess, byte team, string name, byte[] equippedWeaponIDs, byte[] equippedItemIDs, uint networkObjectOwnerID)
        {
            // Set the avatar for the player
            Avatar avi = a.GetComponent<Avatar>();
            p.GetComponent<Player>().Avatar = avi;

            // Sync the variables locally
            avi.UserID = userID;
            avi.GameAccess = gameAccess;

            avi.Team = team;
            avi.Name = name;
            avi.EquippedWeaponIDs = equippedWeaponIDs;
            avi.EquippedItemIDs = equippedItemIDs;

            avi.networkObject.SendRpc("SetupAvatar", Receivers.OthersBuffered, networkObjectOwnerID, team, name, equippedWeaponIDs, equippedItemIDs);

            // Set the players spawn
            Transform spawn = GetSpawn(team);
            p.networkObject.SendRpc(player, "Spawn", spawn.position, spawn.eulerAngles.y);
        }
        */

        int matchCreateAttempts = 0;
        public virtual void StartMatch()
        {
            nv_GameMode.Value = (byte)GameMode;
            nv_GameLength.Value = (byte)TimeLimit;

            nv_AlphaScore.Value = 0;
            nv_BetaScore.Value = 0;
            nv_MaxScore.Value = MaxScore;

            /*
            HeadlessStart h = HeadlessStart.Instance;
            StartCoroutine(DB_API.Match_Create(h.Name, h.Map, h.GameMode, h.PlayerLimit, h.TimeLimit, h.ScoreLimit, h.Password, h.IsTestServer, callback =>
            {
                if (callback.Success)
                {
                    networkObject.GameMode = (byte)h.GameMode;
                    networkObject.GameLength = (byte)h.TimeLimit;

                    networkObject.AlphaScore = 0;
                    networkObject.BetaScore = 0;
                    networkObject.MaxScore = (byte)h.ScoreLimit;
                }
                else
                {
                    if (matchCreateAttempts < 5)
                    {
                        StartMatch();
                        matchCreateAttempts++;
                    }
                    else
                        Application.Quit();
                }
            }));
            */

            StartTime = Time.time;
            nv_InProgress.Value = true;
        }

        public virtual void StopMatch()
        {
            nv_InProgress.Value = false;
        }

        /// <summary>
        /// Pauses the match for x number of seconds
        /// </summary>
        /// <param name="seconds"></param>
        public virtual void PauseMatch(float seconds)
        {
            resumed = false;
            nv_InProgress.Value = false;
            nv_PauseTimeLeft.Value = seconds;
        }

        bool resumed;
        public virtual void CheckPause()
        {
            if (nv_PauseTimeLeft.Value > 0)
            {
                nv_PauseTimeLeft.Value -= Time.deltaTime;
            }
            else if (!resumed)
            {
                resumed = true;
                nv_InProgress.Value = true;
                ResumeMatch();
            }
        }

        public virtual void CheckProgress()
        {

        }

        public virtual void ResumeMatch()
        {
            nv_InProgress.Value = true;
        }

        public virtual void DoChecks()
        {

        }

        public Transform GetSpawn(byte team)
        {
            Transform spawn = team == 1 ? AlphaSpawn : BetaSpawn;
            return spawn;
        }

        public void SendKillfeedMessage(string killerName, byte killerTeam, string victimName, byte victimTeam, byte weaponID)
        {
            InvokeClientRpcOnEveryone("KillfeedMessage", killerName, killerTeam, victimName, victimTeam, weaponID, channel: "Reliable");
        }
    }
}