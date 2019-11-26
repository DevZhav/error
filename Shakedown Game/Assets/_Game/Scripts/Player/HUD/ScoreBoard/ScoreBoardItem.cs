using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Player.HUDElements
{
    public class ScoreBoardItem : MonoBehaviour
    {
        [Header("References")]
        public TextMeshProUGUI PlayerNameText;
        public TextMeshProUGUI PlayerKillsText;
        public TextMeshProUGUI PlayerDeathsText;
        public TextMeshProUGUI PlayerDamageText;

        [Header("Colors")]
        public Color TeamColor;
        public Color EnemyColor;

        [Header("Game Mode Specific")]
        public TextMeshProUGUI PlayerTDText;

        public void SetInfoTD(string name, int kills, int deaths, int damage, int tds, bool sameTeam)
        {
            PlayerNameText.SetText(name);
            PlayerKillsText.SetText(kills.ToString());
            PlayerDeathsText.SetText(deaths.ToString());
            PlayerDamageText.SetText(damage.ToString());
            PlayerTDText.SetText(tds.ToString());

            GetComponent<UnityEngine.UI.Extensions.Gradient>().vertex1 = sameTeam ? TeamColor : EnemyColor;
        }

        public void SetInfoTDM(string name, int kills, int deaths, int damage, bool sameTeam)
        {
            // Disable what we don't need
            PlayerTDText.gameObject.SetActive(false);

            PlayerNameText.SetText(name);
            PlayerKillsText.SetText(kills.ToString());
            PlayerDeathsText.SetText(deaths.ToString());
            PlayerDamageText.SetText(damage.ToString());

            GetComponent<UnityEngine.UI.Extensions.Gradient>().vertex1 = sameTeam ? TeamColor : EnemyColor;
        }
    }
}