using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Player.HUDElements
{
    public class WeaponSlots : MonoBehaviour
    {
        private Controller Controller;

        [System.Serializable]
        public struct WeaponSlot
        {
            public Image Background;
            public Image Icon;
        }
        public WeaponSlot[] Slots = new WeaponSlot[3];

        private IEnumerator coroutine;
        private bool fade;
        private int currentWeapon;

        private Color colorNorm = new Color(0, 0, 0, 0.75f);
        private Color colorSelected = new Color(0, 0.57f, 0.96f, 0.75f);
        private Color colorTrans = new Color(0, 0, 0, 0);

        private void OnEnable()
        {
            // If we die, the root object gets disabled
            // This causes the fade to never get called by the coroutine, so we'll manually call it here
            fade = false;
        }

        public void Initialize(Controller controller)
        {
            Controller = controller;

            // TODO: Remove this... It's only here because Unity is acting weird whenever I try to change transparency of the icons, so I initialize the icons' sprite with a null before setting the actual sprite
            for (int i = 0; i < Slots.Length; i++)
            {
                Slots[i].Icon.sprite = null;
            }
            // Set the weapon slots
            for (int i = 0; i < Slots.Length; i++)
            {
                Slots[i].Icon.sprite = Controller.EquippedWeapons[i].ActiveSkin.Icon;
            }

            DoScript(0);
        }

        public void UpdateScript()
        {
            if (fade)
            {
                for (int i = 0; i < Slots.Length; i++)
                {
                    // If this is not the selected weapon
                    if (i != currentWeapon)
                    {
                        //LerpSlot(Slots[i], 1.0f, 1.0f, 30.0f, 30.0f, colorNorm, 10.0f, 10.0f);

                        Slots[i].Icon.color = Color.Lerp(Slots[i].Icon.color, Color.white, 10 * Time.deltaTime);
                        Slots[i].Background.color = Color.Lerp(Slots[i].Background.color, colorNorm, 10 * Time.deltaTime);

                        Vector3 newScale = new Vector3(1f, 1f, 1f);
                        Slots[i].Icon.rectTransform.localScale = Vector3.Lerp(Slots[i].Icon.rectTransform.localScale, newScale, 30 * Time.deltaTime);
                        Slots[i].Background.rectTransform.localScale = Vector3.Lerp(Slots[i].Background.rectTransform.localScale, newScale, 30 * Time.deltaTime);
                    }
                    // Otherwise if this is the selected weapon
                    else
                    {
                        //LerpSlot(Slots[i], 1.5f, 1.3f, 30.0f, 25.0f, colorSelected, 15.0f, 20.0f);

                        Slots[i].Icon.color = Color.Lerp(Slots[i].Icon.color, Color.white, 15 * Time.deltaTime);
                        Slots[i].Background.color = Color.Lerp(Slots[i].Background.color, colorSelected, 20 * Time.deltaTime);

                        Vector3 newScale = new Vector3(1.5f, 1.5f, 1.5f);
                        Vector3 newScaleBg = new Vector3(1.3f, 1.3f, 1.3f);
                        Slots[i].Icon.rectTransform.localScale = Vector3.Lerp(Slots[i].Icon.rectTransform.localScale, newScale, 30 * Time.deltaTime);
                        Slots[i].Background.rectTransform.localScale = Vector3.Lerp(Slots[i].Background.rectTransform.localScale, newScaleBg, 25 * Time.deltaTime);
                    }
                }
            }
            else
            {
                for (int i = 0; i < Slots.Length; i++)
                {
                    Slots[i].Icon.color = Color.Lerp(Slots[i].Icon.color, colorTrans, 10 * Time.deltaTime);
                    Slots[i].Background.color = Color.Lerp(Slots[i].Background.color, colorTrans, 10 * Time.deltaTime);
                }
            }
        }

        private void LerpSlot(WeaponSlot Slot, float iconScale, float backgroundScale, float iconScaleLerpSpeed, float backgroundScaleLerpSpeed, Color backGroundColor, float iconColorLerpSpeed, float backgroundColorLerpSpeed)
        {
            Vector3 newIconScale = new Vector3(iconScale, iconScale, iconScale);
            Vector3 newBackgroundScale = new Vector3(backgroundScale, backgroundScale, backgroundScale);

            Slot.Icon.rectTransform.localScale = Vector3.Lerp(Slot.Icon.rectTransform.localScale, newIconScale, iconScaleLerpSpeed * Time.deltaTime);
            Slot.Icon.rectTransform.localScale = Vector3.Lerp(Slot.Background.rectTransform.localScale, newBackgroundScale, backgroundScaleLerpSpeed * Time.deltaTime);

            Slot.Icon.color = Color.Lerp(Slot.Icon.color, Color.white, iconColorLerpSpeed * Time.deltaTime);
            Slot.Background.color = Color.Lerp(Slot.Background.color, backGroundColor, backgroundColorLerpSpeed * Time.deltaTime);
        }

        /// <summary>
        /// Displays the weapon change UI
        /// </summary>
        public void DoScript(int weaponSlotNum)
        {
            if (!gameObject.activeSelf)
                return;

            currentWeapon = weaponSlotNum;

            if (coroutine != null)
                StopCoroutine(coroutine);

            coroutine = StartFade();
            StartCoroutine(coroutine);

            // Change the crosshair
            //Crosshair.ChangeCrosshair(activeWeapon.WeaponAsset.CrosshairIndex + 1);
        }

        private IEnumerator StartFade()
        {
            fade = true;
            yield return new WaitForSeconds(0.85f);
            fade = false;
        }
    }
}