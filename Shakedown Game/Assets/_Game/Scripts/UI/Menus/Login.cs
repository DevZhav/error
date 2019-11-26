using Microsoft.Win32;
using MLAPI;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UI.Menus
{
    public class Login : MonoBehaviour
    {
        [Header("Login")]
        public TMP_InputField EmailInput;
        public TMP_InputField PasswordInput;
        public Button LoginButton;
        public Toggle AutoLoginToggle;

        [Header("Generic UI")]
        [SerializeField] Tooltip Tooltip;
        [SerializeField] ConfirmDialog ConfirmDialog;

        private void Start()
        {
            // Setup Generic UI
            Tooltip.Initialize();
            ConfirmDialog.Initialize();

            AutoLoginToggle.isOn = PlayerPrefs.GetInt("AutoLogin") == 1 ? true : false;
            if (PlayerPrefs.GetInt("AutoLogin") == 1)
            {
                EmailInput.text = PlayerPrefs.GetString("Email");
                PasswordInput.text = PlayerPrefs.GetString("Password");
                DoLogin(EmailInput.text, PasswordInput.text);
            }
            else
            {
                EmailInput.text = PlayerPrefs.GetString("Email");
                PasswordInput.text = "";
            }

            LoginButton.onClick.AddListener(() =>
            {
                DoLogin(EmailInput.text, PasswordInput.text);
            });

            RegisterApplicationURL();
        }

        private void Update()
        {
            // We do this here incase we jumped to this scene from ingame; Where the cursor was locked
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;

            // To attempt to login by using the enter key
            if (Input.GetKeyDown(KeyCode.Return))
            {
                DoLogin(EmailInput.text, PasswordInput.text);
            }

            // To switch between the user inputs by using the tab key
            if (Input.GetKeyDown(KeyCode.Tab))
            {
                if (EmailInput.isFocused)
                {
                    EventSystem.current.SetSelectedGameObject(PasswordInput.gameObject);
                }
                else if (PasswordInput.isFocused)
                {
                    EventSystem.current.SetSelectedGameObject(EmailInput.gameObject);
                }
            }
        }

        public void DoLogin(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
                return;

            // Do Loading Screen
            LoginButton.interactable = false;
            LoginButton.GetComponentInChildren<TextMeshProUGUI>().SetText("Logging In...");
            //LoadingScreen.Instance.EnableScreen("Logging In", true);

            StartCoroutine(DB_API.User_Auth(email, password, (session) =>
            {
                if (session.Success)
                {
                    // Gather player data
                    StartCoroutine(DB_API.User_Info(session.SessionID, (data) =>
                    {
                        if (data.Success)
                        {
                            // Start pinging the API
                            DB_Object.Instance.Start_User_Ping_API();

                            // Do Loading Screen
                            LoginButton.interactable = false;
                            LoginButton.GetComponentInChildren<TextMeshProUGUI>().SetText("Loggin Successful...");
                            //LoadingScreen.Instance.EnableScreen("Login Successful", false);
                            // Load menu scene
                            SceneManager.LoadScene("Main Menu");
                        }
                        else
                        {
                            // Do Loading Screen
                            LoginButton.interactable = true;
                            LoginButton.GetComponentInChildren<TextMeshProUGUI>().SetText("Login Failed... Try Again");
                            //LoadingScreen.Instance.DisableScreenFade("Login Failed", 0.1f);
                            return;
                        }
                    }));
                }
                else
                {
                    // Do Loading Screen
                    LoginButton.interactable = true;
                    LoginButton.GetComponentInChildren<TextMeshProUGUI>().SetText("Login Failed... Try Again");
                    //LoadingScreen.Instance.DisableScreenFade("Login Failed", 0.1f);
                    return;
                }
            }));

            // Save the username to the playerprefs
            PlayerPrefs.SetString("Email", email);
            PlayerPrefs.SetString("Password", AutoLoginToggle.isOn ? password : "");
            PlayerPrefs.SetInt("AutoLogin", AutoLoginToggle.isOn ? 1 : 0);
        }

        public void DoRegister()
        {
            Application.OpenURL("https://shakedown.gg/register/");
        }

        public void ConnectLocally()
        {
            //NetworkingManager.Singleton.GetComponent<RufflesTransport.RufflesTransport>().ConnectAddress = "127.0.0.1";
            //NetworkingManager.Singleton.GetComponent<RufflesTransport.RufflesTransport>().ConnectPort = 15937;
            NetworkingManager.Singleton.GetComponent<EnetTransport.EnetTransport>().Address = "127.0.0.1";
            NetworkingManager.Singleton.GetComponent<EnetTransport.EnetTransport>().Port = 15937;

            NetworkingManager.Singleton.NetworkConfig.ConnectionData = System.Text.Encoding.ASCII.GetBytes(DB_API.ConnectionData);
            NetworkingManager.Singleton.StartClient();
        }

        public void OpenSettings()
        {
            Settings.GameSettingsManager.Instance.Open();
        }

        public void QuitGame()
        {
            Application.Quit();
        }

        private void RegisterApplicationURL()
        {
            if (Application.isEditor)
                return;

            string path = Application.dataPath + "/../Shakedown.exe";
            path = path.Replace('/', '\\');

            string UriScheme = "shakedown";
            string FriendlyName = "URL:Shakedown";

            using (var key = Registry.CurrentUser.CreateSubKey("SOFTWARE\\Classes\\" + UriScheme))
            {
                key.SetValue("", FriendlyName);
                key.SetValue("URL Protocol", "");

                using (var defaultIcon = key.CreateSubKey("DefaultIcon"))
                {
                    defaultIcon.SetValue("", path + ",1");
                }

                using (var commandKey = key.CreateSubKey(@"shell\open\command"))
                {
                    commandKey.SetValue("", "\"" + path + "\" \"%1\"");
                }
            }
        }
    }
}