using MLAPI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace Player.HUDElements
{
    public class Pause : MonoBehaviour
    {
        public bool IsOpen()
        {
            return gameObject.activeSelf;
        }

        public void Open()
        {
            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

        public void OpenSettings()
        {
            Settings.GameSettingsManager.Instance.Open();
        }

        public void OpenFeedback()
        {
            //FeedbackScreen.Instance.Open();
        }

        public void LeaveMatch()
        {
            ConfirmDialog.Instance.Open("Leave Match", "Are you sure you want to leave the match?", "Leave Match", "Nevermind", () =>
            {
                StartCoroutine(DB_API.Match_Leave(DB_API.UserAuth.SessionID, DB_API.MatchID, callback => { }));
                NetworkingManager.Singleton.StopClient();
                SceneManager.LoadScene("Main Menu");
            });
        }

        public void QuitGame()
        {
            ConfirmDialog.Instance.Open("Quit Game", "Are you sure you want to quit the game while you are in a match?", "Quit Game", "Nevermind", () =>
            {
                StartCoroutine(DB_API.Match_Leave(DB_API.UserAuth.SessionID, DB_API.MatchID, callback => { }));
                NetworkingManager.Singleton.StopClient();
                Application.Quit();
            });
        }
    }
}