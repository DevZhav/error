using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ChatFeed : MonoBehaviour
{
    public static ChatFeed Instance;

    private List<TextMeshProUGUI> MessageLabels = new List<TextMeshProUGUI>();
    const int MaxMessages = 20;

    [Header("References")]
    public ScrollRect ScrollView;
    public Transform MessageContentArea;
    public GameObject MessagePrefab;
    public TMP_InputField Input;
    public Image[] DisableObjects;

    public bool IsOpen()
    {
        return Input.gameObject.activeSelf;
    }

    private void Start()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
        DontDestroyOnLoad(gameObject);

        // Ad a callback to our input for when we press enter
        //Input.onEndEdit.AddListener(delegate
        //{
        //    if (UnityEngine.Input.GetKeyDown(KeyCode.Return))
        //    {
        //        
        //
        //        Close();
        //    }
        //});
        Close();
    }

    private void Update()
    {
        // Check if the chat is open
        // And if it is, and we press enter, we want to send a message and close the chat
        if (UnityEngine.Input.GetKeyDown(KeyCode.Return) && IsOpen())
        {
            if (Player.Networker.Instance != null && !string.IsNullOrWhiteSpace(Input.text))
                SendMessageToServer();

            Close();
        }
        else if (UnityEngine.Input.GetKeyDown(KeyCode.Return) && !Settings.GameSettingsManager.Instance.IsOpen() && !IsOpen() && SceneManager.GetActiveScene().buildIndex != 0)
            Open();
        // If the chat is open and we press escape, close it regardless of our state
        if ((UnityEngine.Input.GetKeyDown(KeyCode.Escape) && IsOpen()) || SceneManager.GetActiveScene().buildIndex == 0)
            Close();
    }

    private void SendMessageToServer()
    {
        Player.Networker.Instance.InvokeServerRpc("ChatMessage_Server", Input.text, channel: "Reliable");
        //Player.Networker.Instance.Avatar.networkObject.SendRpc("SendChatMessageToServer", BeardedManStudios.Forge.Networking.Receivers.Owner, Input.text);
    }

    public void Open()
    {
        foreach (Image obj in DisableObjects)
        {
            if (!obj.GetComponent<Mask>())
            {
                obj.enabled = true;
                continue;
            }

            // If the component has a Mask
            obj.GetComponent<Mask>().showMaskGraphic = true;
            obj.raycastTarget = true;
        }

        WakeChat();
        Input.gameObject.SetActive(true);
        Input.ActivateInputField();
        Input.Select();
    }
    public void Close()
    {
        foreach (Image obj in DisableObjects)
        {
            if (!obj.GetComponent<Mask>())
            {
                obj.enabled = false;
                continue;
            }

            // If the component has a Mask
            obj.GetComponent<Mask>().showMaskGraphic = false;
            obj.raycastTarget = false;
        }

        SleepChat();
        Input.text = "";
        Input.DeactivateInputField();
        Input.gameObject.SetActive(false);
    }

    public void Disable()
    {
        MessageContentArea.parent.gameObject.SetActive(false);
    }

    public void Enable()
    {
        MessageContentArea.parent.gameObject.SetActive(true);
    }

    public void AddMessageToChat(byte gameAccess = 0, string senderName = "", byte senderTeam = 0, string senderMessage = "")
    {
        // The name of the player with the colors added
        string name = PlayerMethods.GetColoredTitleName(senderName, gameAccess, senderTeam);
        // The link you click on to get a player's rpofile
        string link = string.Format("<link=\"https://shakedown.gg/player/{0}/\">{1}</link>", senderName, name);
        // The constructed message
        string message =  link + ": " + senderMessage;

        // Check if we're over our max messages
        // If we are, remove the first message
        if (MessageLabels.Count >= MaxMessages)
            MessageLabels.RemoveAt(0);

        // Create an empty gameobject for us to use as a variable
        ChatMessage label = null;

        // Instantiate it as a gameobject first, then get the KillFeedMessage component attached
        label = (Instantiate(MessagePrefab, MessageContentArea) as GameObject).GetComponent<ChatMessage>();

        // Set it as the last index in the transform
        // So that it's shown to the bottom of the UI
        label.transform.SetAsLastSibling();

        // Add the gameobject to our list
        MessageLabels.Add(label.GetComponent<TextMeshProUGUI>());

        // And now set the info on the text mesh
        label.Initialize(!IsOpen(), message);
        StartCoroutine(ScrollToBottom());
    }

    public IEnumerator ScrollToBottom()
    {
        yield return new WaitForEndOfFrame();
        ScrollView.verticalNormalizedPosition = 0;
    }

    public void WakeChat()
    {
        foreach (ChatMessage msg in MessageContentArea.GetComponentsInChildren<ChatMessage>(true))
        {
            msg.Wake();
        }
    }

    public void SleepChat()
    {
        foreach (ChatMessage msg in MessageContentArea.GetComponentsInChildren<ChatMessage>(true))
        {
            msg.Sleep();
        }
    }
}
