using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Player.HUDElements
{
    public class KillFeedMessage : MonoBehaviour
    {
        public float DisappearTime = 10;
        public float DisappearSpeed = 2.0f;

        [Header("")]
        public TextMeshProUGUI Killer;
        public TextMeshProUGUI Victim;
        public Image WeaponIcon;

        float alpha = 1;
        bool fadeOut = false;

        public void Initialize(string killer, string victim, Sprite weaponIcon)
        {
            Killer.SetText(killer);
            Victim.SetText(victim);
            WeaponIcon.sprite = weaponIcon;
            StartCoroutine(DisableObject());
        }

        private void LateUpdate()
        {
            if (fadeOut && Killer.color.a > 0.01f)
            {
                alpha -= DisappearSpeed * Time.deltaTime;
                Killer.color = new Color(Killer.color.r, Killer.color.g, Killer.color.b, alpha);
                Victim.color = new Color(Victim.color.r, Victim.color.g, Victim.color.b, alpha);
                WeaponIcon.color = new Color(WeaponIcon.color.r, WeaponIcon.color.g, WeaponIcon.color.b, alpha);
            }
            else if (alpha <= 0.012f)
            {
                Destroy(gameObject);
            }
        }

        private IEnumerator DisableObject()
        {
            yield return new WaitForSeconds(DisappearTime);
            fadeOut = true;
        }
    }
}