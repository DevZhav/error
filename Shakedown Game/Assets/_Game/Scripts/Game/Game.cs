using UnityEngine;
using MLAPI.NetworkedVar;
using MLAPI;
using MLAPI.Messaging;

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
        public GameModes Mode
        {
            get
            {
                return (GameModes)nv_GameMode.Value;
            }
        }

        public static Game Instance;

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

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
        }

        [ClientRPC]
        public void KillfeedMessage(string killerName, byte killerTeam, string victimName, byte victimTeam, byte weaponID)
        {

            Player.Networker.Instance.Controller.HUD.KillFeed.Message(killerName, killerTeam, victimName, victimTeam, weaponID);
        }
    }
}