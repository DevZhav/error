using UnityEngine;
using TMPro;

namespace Player.HUDElements
{
    public class Ammo : MonoBehaviour
    {
        [Header("References")]
        public TextMeshProUGUI CurrentAmmoText;
        public TextMeshProUGUI ClipAmmoText;

        public void UpdateScript(int currentAmmo, int ammoPerClip)
        {
            CurrentAmmoText.SetText(currentAmmo.ToString());
            ClipAmmoText.SetText(ammoPerClip.ToString());
        }
    }
}