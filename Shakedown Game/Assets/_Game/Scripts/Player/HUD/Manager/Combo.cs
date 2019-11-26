using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Player.HUDElements
{
    public class Combo : MonoBehaviour
    {
        public float DisappearTime = 3;
        public float DisappearSpeed = 2.0f;

        [Header("References")]
        public TextMeshProUGUI ComboText;
        public TextMeshProUGUI InfoText;

        short damage;
        float alpha = 0;
        bool fadeOut = false;

        public void UpdateScript()
        {
            if (fadeOut)
                alpha -= DisappearSpeed * Time.deltaTime;
            transform.localScale = Vector2.Lerp(transform.localScale, Vector2.one, 12 * Time.deltaTime);

            ComboText.color = new Color(ComboText.color.r, ComboText.color.g, ComboText.color.b, alpha);
            InfoText.color = new Color(InfoText.color.r, InfoText.color.g, InfoText.color.b, alpha);
        }

        public void AddComboDamage(short dmg)
        {
            // If we've already exhaused our time, reset our damage
            if (alpha <= -5)
                damage = 0;

            damage += dmg;
            ComboText.SetText(damage.ToString());

            transform.localScale = new Vector2(1.6f, 1.6f);
            fadeOut = false;
            alpha = 1;

            StartCoroutine(DisableObject());
        }

        private IEnumerator DisableObject()
        {
            yield return new WaitForSeconds(DisappearTime);
            fadeOut = true;
        }
    }
}