using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ConfirmDialog : MonoBehaviour
{
    public static ConfirmDialog Instance;

    [Header("References")]
    public Button AcceptButton;
    public Button DeclineButton;
    public TextMeshProUGUI Title;
    public TextMeshProUGUI Message;

    public void Initialize()
    {
        if (Instance == null)
            Instance = this;
        else if (Instance != this)
            Destroy(transform.root.gameObject);
        DontDestroyOnLoad(transform.root.gameObject);
    }

    public void Open(string title, string message, UnityAction accept, UnityAction decline = null)
    {
        Title.SetText(title);
        Message.SetText(message);

        // --- BUTTONS --- //
        AcceptButton.onClick.RemoveAllListeners();
        DeclineButton.onClick.RemoveAllListeners();

        AcceptButton.onClick.AddListener(accept);
        if (decline != null)
            DeclineButton.onClick.AddListener(decline);

        // We want to close regardless of what we click
        AcceptButton.onClick.AddListener(Close);
        DeclineButton.onClick.AddListener(Close);

        // Reset the button text
        AcceptButton.GetComponentInChildren<TextMeshProUGUI>().SetText("accept");
        DeclineButton.GetComponentInChildren<TextMeshProUGUI>().SetText("decline");

        // Enable the game object
        gameObject.SetActive(true);
    }

    public void Open(string title, string message, string acceptButtonText, string declineButtonText, UnityAction accept, UnityAction decline = null)
    {
        Open(title, message, accept, decline);
        AcceptButton.GetComponentInChildren<TextMeshProUGUI>().SetText(acceptButtonText);
        DeclineButton.GetComponentInChildren<TextMeshProUGUI>().SetText(declineButtonText);
    }

    public void Open(string title, string message, string acceptButtonText, UnityAction accept, UnityAction decline = null)
    {
        Open(title, message, accept, decline);
        AcceptButton.GetComponentInChildren<TextMeshProUGUI>().SetText(acceptButtonText);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
