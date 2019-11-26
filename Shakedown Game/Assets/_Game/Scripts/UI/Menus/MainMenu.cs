using MLAPI;
using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI.Menus
{
    public class MainMenu : MonoBehaviour
    {
        [Header("References")]
        public GameObject Manager;

        [Header("Profile")]
        public TextMeshProUGUI NamePlateText;
        public Image Avatar;

        [Header("Server List")]
        public TextMeshProUGUI ServerStatus;
        public RectTransform ServerListRect;
        public GameObject ServerEntryObject;

        private void Start()
        {
            User_Info info = DB_API.UserInfo;
            NamePlateText.SetText(PlayerMethods.GetColoredTitleName(info.Account.Name, (byte)info.Account.GameAccess, 0));
            StartCoroutine(DB_API.User_Avatar("m", callback =>
            {
                Avatar.sprite = Sprite.Create(callback, new Rect(0.0f, 0.0f, callback.width, callback.height), new Vector2(0.5f, 0.5f));
            }));

            InvokeRepeating("UpdateServerList", 0.0f, 3.0f);
        }

        private void Update()
        {
            // We do this here incase we jumped to this scene from ingame; Where the cursor was locked
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        private void OnGUI()
        {
            // Only for when we're in the editor
            if (!Application.isEditor)
                return;

            if (GUILayout.Button("Local Connect"))
            {
                NetworkingManager.Singleton.GetComponent<EnetTransport.EnetTransport>().Address = "127.0.0.1";
                NetworkingManager.Singleton.GetComponent<EnetTransport.EnetTransport>().Port = 15937;

                //NetworkingManager.Singleton.GetComponent<LiteNetLibTransport.LiteNetLibTransport>().Address = "127.0.0.1";
                //NetworkingManager.Singleton.GetComponent<LiteNetLibTransport.LiteNetLibTransport>().Port = 15937;

                NetworkingManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(DB_API.ConnectionData);
                NetworkingManager.Singleton.StartClient();
            }
        }

        private void UpdateServerList()
        {
            StartCoroutine(DB_API.Match_List(DB_API.UserAuth.SessionID, callback =>
            {
                if (callback.Success)
                {
                    // Clear the game objects of removed rooms
                    for (int i = 0; i < ServerListRect.childCount; i++)
                        Destroy(ServerListRect.GetChild(i).gameObject);

                    // If no matches are created
                    if (callback.Matches.Count == 0)
                    {
                        ServerStatus.SetText("No Servers Found...");
                        return;
                    }
                    ServerStatus.SetText("");

                    // Otherwise instantiate server objects for the matches
                    foreach (var match in callback.Matches)
                    {
                        GameObject obj = Instantiate(ServerEntryObject, ServerListRect);
                        obj.GetComponent<ServerListEntry>().Initialize(match);
                    }
                }
                else
                {
                    ServerStatus.SetText("Failed to get Match List");

                    // Clear the game objects of removed rooms
                    for (int i = 0; i < ServerListRect.childCount; i++)
                        Destroy(ServerListRect.GetChild(i).gameObject);
                }
            }));
        }

        public void CreateMatch()
        {
            StartCoroutine(DB_API.Match_Create(DB_API.UserAuth.SessionID, "duh herro", "Reactor", 1, 12, 10, 10, "", true, callback =>
            {
                if (callback.Success)
                {
                    StartCoroutine(DB_API.Match_Join(DB_API.UserAuth.SessionID, callback.MatchID, "", c =>
                    {
                        if (c.Success)
                        {
                            DB_API.MatchID = callback.MatchID;
                            DB_Object.Instance.Start_Match_Ping_API();

                            string ip = c.Server.IP;
                            ushort port = (ushort)c.Server.Port;

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
            }));
        }

        public void OpenSettings()
        {
            Settings.GameSettingsManager.Instance.Open();
        }

        public void Logout()
        {
            PlayerPrefs.DeleteKey("Email");
            PlayerPrefs.DeleteKey("Password");
            PlayerPrefs.DeleteKey("AutoLogin");
            SceneManager.LoadScene("Login");
        }

        public void QuitGame()
        {
            Application.Quit();
        }
    }
}