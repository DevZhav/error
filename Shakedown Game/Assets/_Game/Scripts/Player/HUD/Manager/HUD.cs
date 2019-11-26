using UnityEngine;
using Player.HUDElements;
using MLAPI;

namespace Player
{
    public class HUD : MonoBehaviour
    {
        private Controller Controller;

        [Header("References")]
        public Statistics Statistics;
        public Ammo Ammo;
        public WeaponSlots WeaponSlots;
        public KillFeed KillFeed;
        public Combo Combo;
        public ScoreBoard ScoreBoard;
        public Pause Pause;

        [Header("Global Transform")]
        public Transform NameTagParent;

        public void Initialize(Controller controller)
        {
            Controller = controller;

            Statistics.Initialize(Controller.MaxHealth, controller.MaxStamina);
            WeaponSlots.Initialize(Controller);
            ScoreBoard.Initialize(Controller);

            Pause.Close();
            ScoreBoard.Close();
        }

        public void UpdateHUD()
        {
            UpdateStates();

            if (!Controller.Networker.nv_Dead.Value)
            {
                //Statistics.UpdateScript(Controller.State.Health, Controller.State.Stamina);
                //WeaponSlots.UpdateScript();
                //Ammo.UpdateScript(Controller.ActiveWeapon.CurrentAmmo, Controller.ActiveWeapon.AmmoPerClip);
            }
            Statistics.UpdateScript(Controller.Networker.nv_Health.Value, Controller.State.Stamina);
            WeaponSlots.UpdateScript();
            Ammo.UpdateScript(Controller.ActiveWeapon.CurrentAmmo, Controller.ActiveWeapon.AmmoPerClip);

            Combo.UpdateScript();
        }

        private void UpdateStates()
        {
            // CHAT
            if (ChatFeed.Instance.IsOpen())
            {
                Controller.Input.StopInput = true;
                ChangeCursorState(false);
            }
            else if (ChatFeed.Instance.IsOpen() && UnityEngine.Input.GetKeyDown(KeyCode.Escape))
            {
                ChatFeed.Instance.Close();
            }
            // SCORE BOARD
            else if (UnityEngine.Input.GetKeyDown(KeyCode.Tab) && !ChatFeed.Instance.IsOpen() && !Pause.IsOpen())
            {
                ScoreBoard.LoadPlayers(Controller.Networker.nv_Team.Value);
                ScoreBoard.Open();
            }
            else if (UnityEngine.Input.GetKeyUp(KeyCode.Tab))
            {
                ScoreBoard.Close();
            }
            // PAUSE MENU
            else if (UnityEngine.Input.GetKeyDown(KeyCode.Escape) && !Pause.IsOpen() && !ChatFeed.Instance.IsOpen() && !ScoreBoard.IsOpen())
            {
                Pause.Open();
                Controller.Input.StopInput = true;
                ChangeCursorState(false);
            }
            else if (UnityEngine.Input.GetKeyDown(KeyCode.Escape) && Pause.IsOpen())
            {
                if (!Settings.GameSettingsManager.Instance.IsOpen() /*&& !FeedbackScreen.Instance.IsOpen()*/)
                    Pause.Close();
            }
            // SETTINGS
            else if (Settings.GameSettingsManager.Instance.IsOpen())
            {
                ChatFeed.Instance.Disable();
            }
            else
            {
                ChatFeed.Instance.Enable();
            }

            // Give back input incase we closed the menu with the button
            if (!Settings.GameSettingsManager.Instance.IsOpen() && !Pause.IsOpen() && !ChatFeed.Instance.IsOpen() && !ScoreBoard.IsOpen())
            {
                Controller.Input.StopInput = false;
                ChangeCursorState(true);
            }
        }

        public void ChangeCursorState(bool locked)
        {
            if (!locked)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else if (locked)
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }

        public void OnGUI()
        {
            string gameInfo = "";
            gameInfo += "Ping: " + "N/A";
            gameInfo += "\n" + "Game Mode: " + Game.Game.Instance.nv_GameMode.Value;
            gameInfo += "\n" + "Game Length: " + Game.Game.Instance.nv_GameLength.Value / 60;
            gameInfo += "\n" + "Alpha Score: " + Game.Game.Instance.nv_AlphaScore.Value;
            gameInfo += "\n" + "Beta Score: " + Game.Game.Instance.nv_BetaScore.Value;
            gameInfo += "\n" + "Max Score: " + Game.Game.Instance.nv_MaxScore.Value;
            gameInfo += "\n" + "In Progress: " + Game.Game.Instance.nv_InProgress.Value;
            gameInfo += "\n" + "Current Time: " + (Game.Game.Instance.nv_CurrentTime.Value / 60).ToString("F2");
            gameInfo += "\n" + "Pause Time Left: " + (Game.Game.Instance.nv_PauseTimeLeft.Value).ToString("F2");

            GUILayout.Label(gameInfo);
        }
    }
}