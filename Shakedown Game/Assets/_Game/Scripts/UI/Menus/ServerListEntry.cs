using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using System;
using MLAPI;

namespace UI.Menus
{
    public class ServerListEntry : MonoBehaviour
    {
        public TextMeshProUGUI ServerNameText;
        public TextMeshProUGUI MapNameText;
        public TextMeshProUGUI ModeNameText;
        public TextMeshProUGUI PlayerCountText;
        public Button JoinButton;

        public void Initialize(Match_List.Match match)
        {
            ServerNameText.SetText(match.Name);
            PlayerCountText.SetText(match.PlayerOnline.ToString() + "/" + match.PlayerLimit.ToString());

            MapNameText.SetText(match.Map);
            ModeNameText.SetText(Enum.GetName(typeof(Game.Game.GameModes), match.GameMode));

            JoinButton.onClick.RemoveAllListeners();
            JoinButton.onClick.AddListener(() =>
            {
                JoinMatch(match);
            });
        }

        private void JoinMatch(Match_List.Match match)
        {
            StartCoroutine(DB_API.Match_Join(DB_API.UserAuth.SessionID, match.ID, "", callback =>
            {
                if (callback.Success)
                {
                    DB_API.MatchID = match.ID;
                    DB_Object.Instance.Start_Match_Ping_API();

                    string ip = callback.Server.IP;
                    ushort port = (ushort)callback.Server.Port;

                    NetworkingManager.Singleton.GetComponent<EnetTransport.EnetTransport>().Address = ip;
                    NetworkingManager.Singleton.GetComponent<EnetTransport.EnetTransport>().Port = port;

                    //NetworkingManager.Singleton.GetComponent<RufflesTransport.RufflesTransport>().ConnectAddress = ip;
                    //NetworkingManager.Singleton.GetComponent<RufflesTransport.RufflesTransport>().ConnectPort = port;

                    //NetworkingManager.Singleton.GetComponent<LiteNetLibTransport.LiteNetLibTransport>().Address = ip;
                    //NetworkingManager.Singleton.GetComponent<LiteNetLibTransport.LiteNetLibTransport>().Port = port;

                    NetworkingManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(DB_API.ConnectionData);
                    NetworkingManager.Singleton.StartClient();
                }
            }));
        }
    }
}