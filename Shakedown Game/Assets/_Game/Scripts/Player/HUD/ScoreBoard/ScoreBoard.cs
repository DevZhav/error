using UnityEngine;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

namespace Player.HUDElements
{
    public class ScoreBoard : MonoBehaviour
    {
        [Header("References")]
        public Controller Controller;
        public GameObject ScoreBoardItem;
        public Transform TeamContentParent;
        public Transform EnemyContentParent;

        [Header("Info")]
        public TextMeshProUGUI ServerInfoText;

        [Header("Mode Headers")]
        public GameObject TouchdownScoreHeader;

        public bool IsOpen()
        {
            return gameObject.activeSelf;
        }

        public void Initialize(Controller controller)
        {
            Controller = controller;
        }

        public void LoadPlayers(int team)
        {
            // Disable or enable headers depending on our game mode
            TouchdownScoreHeader.SetActive(Game.Game.Instance.Mode == Game.Game.GameModes.Touchdown);

            // Remove the already spawned score board items, so we can add them manually again later
            RemoveItems();

            // Grab our team
            GrabTeam(FindObjectsOfType<Networker>().Where(x => x.nv_Team.Value == 1), Controller.Networker.nv_Team.Value == 1);
            GrabTeam(FindObjectsOfType<Networker>().Where(x => x.nv_Team.Value == 2), Controller.Networker.nv_Team.Value == 2);

            // Set the server name + map text
            ServerInfoText.SetText(string.Format("{0} [{1}]", "Room Name Unavailable", SceneManager.GetActiveScene().name));
        }

        private void GrabTeam(IEnumerable networkers, bool sameTeam)
        {
            ScoreBoardItem myItem = null;

            foreach (Networker n in networkers)
            {
                GameObject obj = Instantiate(ScoreBoardItem, sameTeam ? TeamContentParent : EnemyContentParent);
                ScoreBoardItem item = obj.GetComponent<ScoreBoardItem>();

                if (Game.Game.Instance.Mode == Game.Game.GameModes.TeamDeathMatch)
                    item.SetInfoTDM(n.nv_Name.Value, n.nv_Score_Eliminations.Value, n.nv_Score_Deaths.Value, n.nv_Score_Damage.Value, sameTeam);
                else if (Game.Game.Instance.Mode == Game.Game.GameModes.Touchdown)
                    item.SetInfoTD(n.nv_Name.Value, n.nv_Score_Eliminations.Value, n.nv_Score_Deaths.Value, n.nv_Score_Damage.Value, n.nv_Score_Score.Value, sameTeam);

                // Change the color's alpha based on whether we're dead or alive
                item.PlayerNameText.color = new Color(item.PlayerNameText.color.r, item.PlayerNameText.color.g, item.PlayerNameText.color.b, n.nv_Dead.Value == true ? 0.5f : 1.0f);
                // And also strikethrough
                string s = n.nv_Dead.Value == true ? "<s>" : "";
                item.PlayerNameText.SetText(s + n.nv_Name.Value);

                if (n == Controller.Networker)
                    myItem = item;
            }

            if (myItem != null)
            {
                myItem.transform.SetAsFirstSibling();
                myItem.PlayerNameText.color = new Color32(96, 255, 96, ((Color32)myItem.PlayerNameText.color).a);
            }
        }

        private void RemoveItems()
        {
            foreach (Transform t in TeamContentParent.GetComponentsInChildren<Transform>())
            {
                if (t != TeamContentParent)
                    Destroy(t.gameObject);
            }

            foreach (Transform t in EnemyContentParent.GetComponentsInChildren<Transform>())
            {
                if (t != EnemyContentParent)
                    Destroy(t.gameObject);
            }
        }
        public void Open()
        {
            gameObject.SetActive(true);
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }

    }
}