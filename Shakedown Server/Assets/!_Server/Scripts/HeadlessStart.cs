using MLAPI;
using MLAPI.SceneManagement;
using MLAPI.Spawning;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;
using UnityEngine.SceneManagement;

public class HeadlessStart : MonoBehaviour
{
    public static HeadlessStart Instance;
    public Guid UUID = Guid.NewGuid();
    public ServerSettings Settings = null;

    [Header("Prefabs")]
    public GameObject GamePrefab;
    public GameObject StructurePrefab;
    public GameObject PlayerPrefab;

    int startAttempts = 0; // How many times have we tried to start the server thus far

    private void Start()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
            Destroy(gameObject);

        // Set the target framerate
        Application.targetFrameRate = 60;

        // Get the server settings
        LoadFile();

        UUID = Guid.NewGuid();
        Debug.Log("UUIDv4: " + UUID);
        if (!Settings.IsTestServer)
        {
            StartCoroutine(DB_API.Server_Create(UUID.ToString(), Settings.PlayerLimit, 1, Settings.IP, Settings.Port, callback =>
            {
                if (callback.Success)
                {
                    StartServer();
                }
                else
                {
                    if (startAttempts < 5)
                    {
                        Start();
                        startAttempts++;
                    }
                    else
                        Application.Quit();
                }
            }));
        }
        else
            StartServer();

        //string title = string.Format("Shakedown Server | Name: {0} | Port: {1} | Map: {2} | GameMode: {3} | PlayerLimit: {4} | ScoreLimit: {5} | Password: {6}", Name, Port, Map, GameMode, PlayerLimit, ScoreLimit, Password);
        //ChangeTitle(title);
    }

    private void StartServer()
    {
        Debug.Log("Starting server...");
        DontDestroyOnLoad(gameObject);

        //NetworkingManager.Singleton.GetComponent<RufflesTransport.RufflesTransport>().ServerListenPort = Port;

        NetworkingManager.Singleton.GetComponent<EnetTransport.EnetTransport>().Address = Settings.IP;
        NetworkingManager.Singleton.GetComponent<EnetTransport.EnetTransport>().Port = (ushort)Settings.Port;

        //NetworkingManager.Singleton.GetComponent<LiteNetLibTransport.LiteNetLibTransport>().Address = Settings.IP;
        //NetworkingManager.Singleton.GetComponent<LiteNetLibTransport.LiteNetLibTransport>().Port = (ushort)Settings.Port;

        NetworkingManager.Singleton.ConnectionApprovalCallback += ApprovalCheck;
        NetworkingManager.Singleton.StartServer();
        NetworkSceneManager.SwitchScene("Reactor");
    }

    private void ApprovalCheck(byte[] connectionData, ulong clientID, NetworkingManager.ConnectionApprovedDelegate callback)
    {
        Debug.Log("Checking Connection Approval");
        if (Settings.IsTestServer)
        {
            Debug.Log("A connection was approved without security checks.");
            callback(true, 1897319656204293034, true, null, null);
            SpawnPlayer(clientID);
            return;
        }

        string cd = System.Text.Encoding.ASCII.GetString(connectionData);
        string[] data = cd.Split(',');
        int matchID = int.Parse(data[0]);
        int userID = int.Parse(data[1]);

        StartCoroutine(DB_API.Server_Match_User_Authorized(matchID, userID, c =>
        {
            Debug.Log("Client Connection Approval: " + c.Success);
            callback(true, null, c.Success, null, null);
            SpawnPlayer(clientID);
        }));
    }

    private void SpawnPlayer(ulong clientID)
    {
        //GameObject playerGameObject = Instantiate(PlayerPrefab);
        //playerGameObject.GetComponent<NetworkedObject>().SpawnWithOwnership(clientID);
        //Debug.Log("Spawn " + clientID);
    }

    private void OnLevelWasLoaded(int level)
    {
        Debug.Log("Setting up scene...");
        var game = Instantiate(GamePrefab);
        game.GetComponent<NetworkedObject>().Spawn();
        var g = game.GetComponent<Game.Game>();

        g.GameMode = (Game.Game.GameModes)Settings.GameMode;
        g.PlayerLimit = (Game.Game.PlayerLimits)Settings.PlayerLimit;
        g.TimeLimit = (Game.Game.TimeLimits)Settings.TimeLimit;
        g.MaxScore = (byte)Settings.ScoreLimit;
    }

    private void OnApplicationQuit()
    {
        NetworkingManager.Singleton.StopServer();
    }

    static string GetArg(params string[] names)
    {
        var args = Environment.GetCommandLineArgs();
        for (int i = 0; i < args.Length; i++)
        {
            foreach (var name in names)
            {
                if (args[i] == name && args.Length > i + 1)
                {
                    return args[i + 1];
                }
            }
        }

        return null;
    }

    public void LoadFile()
    {
        string serverSettingsPath = Application.dataPath + (Application.isEditor ? "/" : "/../") + "settings.json";

        if (!File.Exists(serverSettingsPath))
        {
            SaveFile();
            return;
        }

        Debug.Log("Loading Server Settings");
        StreamReader reader = new StreamReader(serverSettingsPath);
        string json = reader.ReadToEnd();

        Settings = JsonUtility.FromJson<ServerSettings>(json);
        Debug.Log(Settings.ToString());

        // This is here to attempt to get our server's IP address automatically
        if (Settings.IP.ToLower() == "auto")
        {
            string externalIP = new WebClient().DownloadString("http://icanhazip.com");
            Debug.Log("Received Public IP: " + externalIP);
        }
    }

    [ContextMenu("Save File")]
    public void SaveFile()
    {
        Debug.Log("Saving Server Settings");
        string serverSettingsPath = Application.dataPath + (Application.isEditor ? "/" : "/../") + "settings.json";

        ServerSettings save = new ServerSettings();
        string json = JsonUtility.ToJson(save, true);

        StreamWriter writer = new StreamWriter(serverSettingsPath);
        writer.Write(json);
        writer.Close();
    }
}

[System.Serializable]
public class ServerSettings
{
    public string IP = "AUTO";
    public int Port = 15937;
    public bool IsTestServer = false;
    public string Map = "Reactor";
    public int GameMode = 1;
    public int PlayerLimit = 12;
    public int TimeLimit = 10;
    public int ScoreLimit = 10;
    public string Password = "";

    public override string ToString() => JsonUtility.ToJson(this, true).ToString();
}