using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using TMPro;

namespace Settings
{
    public class GameSettingsManager : MonoBehaviour
    {
        [Header("Instance")]
        public static GameSettingsManager Instance = null;
        public GameObject SettingsScreen;

        [Header("Feedback Text")]
        public TextMeshProUGUI FeedbackText;

        [Header("Save")]
        private DisplaySettingsSave DisplaySave;
        private SoundSettingsSave SoundSave;
        private ControlSettingsSave ControlSave;
        private InputSettingsSave InputSave;

        [Header("Buttons")]
        public Button ButtonDisplay;
        public Button ButtonSound;
        public Button ButtonControls;

        [Header("Panels")]
        public GameObject PanelDisplay;
        public GameObject PanelSound;
        public GameObject PanelControls;

        [Header("")]
        [HideInInspector] public DisplaySettings DisplaySettings;
        [HideInInspector] public SoundSettings SoundSettings;
        [HideInInspector] public ControlSettings ControlSettings;
        [HideInInspector] public InputSettings InputSettings;

        public bool IsOpen()
        {
            return SettingsScreen.activeSelf;
        }

        public void Open()
        {
            LoadSettings();
            SettingsScreen.SetActive(true);
        }

        public void Close()
        {
            SettingsScreen.SetActive(false);
        }

        private void Awake()
        {
            if (Instance == null)
                Instance = this;
            else
                Destroy(gameObject);
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            // Disable feedback text
            FeedbackText.SetText(string.Empty);
            FeedbackText.transform.parent.gameObject.SetActive(false);

            //Creates an empty settings file to load. This is because it will return null reference exceptions if it's not done first. 
            DisplaySave = new DisplaySettingsSave();
            SoundSave = new SoundSettingsSave();
            ControlSave = new ControlSettingsSave();
            InputSave = new InputSettingsSave();

            // Setup buttons
            ButtonDisplay.interactable = false; // Set the display to be interactable by default

            ButtonDisplay.onClick.AddListener(delegate
            {
                PanelDisplay.SetActive(true);
                PanelSound.SetActive(false);
                PanelControls.SetActive(false);

                ButtonDisplay.interactable = false;
                ButtonSound.interactable = true;
                ButtonControls.interactable = true;
            });

            ButtonSound.onClick.AddListener(delegate
            {
                PanelDisplay.SetActive(false);
                PanelSound.SetActive(true);
                PanelControls.SetActive(false);

                ButtonDisplay.interactable = true;
                ButtonSound.interactable = false;
                ButtonControls.interactable = true;
            });

            ButtonControls.onClick.AddListener(delegate
            {
                PanelDisplay.SetActive(false);
                PanelSound.SetActive(false);
                PanelControls.SetActive(true);

                ButtonDisplay.interactable = true;
                ButtonSound.interactable = true;
                ButtonControls.interactable = false;

            });

            // Get components
            DisplaySettings = PanelDisplay.GetComponent<DisplaySettings>();
            SoundSettings = PanelSound.GetComponent<SoundSettings>();
            ControlSettings = PanelControls.GetComponent<ControlSettings>();
            InputSettings = PanelControls.GetComponent<InputSettings>();

            // Set the default screen
            PanelDisplay.SetActive(true);
            PanelSound.SetActive(false);
            PanelControls.SetActive(false);

            // Setup Options
            LoadSettings();
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.Escape))
            {
                Close();
            }
        }

        public void LoadSettings()
        {
            DisplaySettings.Setup(DisplaySave);
            DisplaySettings.LoadOptions();

            SoundSettings.Setup(SoundSave);
            SoundSettings.LoadOptions();

            ControlSettings.Setup(ControlSave);
            ControlSettings.LoadOptions();

            InputSettings.Setup(InputSave);
            InputSettings.LoadOptions();
        }

        public void ApplySettings()
        {
            DisplaySettings.Apply();
            DisplaySettings.SaveOptions();

            SoundSettings.Apply();
            SoundSettings.SaveOptions();

            ControlSettings.Apply();
            ControlSettings.SaveOptions();

            InputSettings.Apply();
            InputSettings.SaveOptions();

            StartCoroutine(SetFeedbackText("Settings saved"));
        }

        // Used for the UI Button
        public void SaveSettings()
        {
            // Open the confirmation Dialog
            // If confirm then run commands
            ConfirmDialog.Instance.Open("Apply Settings", "Are you sure you want to apply the current settings?", "Apply Settings", () =>
            {
                ApplySettings();
                Close();
            });
        }

        public void ResetSettings()
        {
            // Open the confirmation Dialog
            // If confirm then run commands
            ConfirmDialog.Instance.Open("Reset Settings", "Are you sure you want to reset ALL settings to default?", "Reset To Default", () =>
            {
                DisplaySettings.Save = new DisplaySettingsSave();
                SoundSettings.Save = new SoundSettingsSave();
                ControlSettings.Save = new ControlSettingsSave();
                InputSettings.Save = new InputSettingsSave();

                DisplaySettings.SaveOptions();
                SoundSettings.SaveOptions();
                ControlSettings.SaveOptions();
                InputSettings.SaveOptions();

                LoadSettings();
            });
        }

        private IEnumerator SetFeedbackText(string text)
        {
            FeedbackText.SetText(text);
            FeedbackText.transform.parent.gameObject.SetActive(true);
            yield return new WaitForSeconds(5.0f);
            FeedbackText.SetText(string.Empty);
            FeedbackText.transform.parent.gameObject.SetActive(false);
        }
    }
}