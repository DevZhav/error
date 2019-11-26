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
    public class ControlSettings : MonoBehaviour
    {
        public ControlSettingsSave Save;
        const string fileName = "control_settings";

        [Header("Options")]
        public Slider MouseXSlider;
        public Slider MouseYSlider;
        public TMP_Dropdown InvertedMouseXDropdown;
        public TMP_Dropdown InvertedMouseYDropdown;
        [Space(10)]
        public TMP_Dropdown DoubleTapDodgeDropdown;
        public TMP_Dropdown WeaponSwitchScrollInfiniteDropdown;

        public void Setup(ControlSettingsSave save)
        {
            Save = save;

            // Inverted Mouse X
            InvertedMouseXDropdown.ClearOptions();
            List<TMP_Dropdown.OptionData> invertedMouseXOptions = new List<TMP_Dropdown.OptionData>()
            {
                new TMP_Dropdown.OptionData("Off"),
                new TMP_Dropdown.OptionData("On"),
            };
            InvertedMouseXDropdown.AddOptions(invertedMouseXOptions);

            // Inverted Mouse Y
            InvertedMouseYDropdown.ClearOptions();
            List<TMP_Dropdown.OptionData> invertedMouseYOptions = new List<TMP_Dropdown.OptionData>()
            {
                new TMP_Dropdown.OptionData("Off"),
                new TMP_Dropdown.OptionData("On"),
            };
            InvertedMouseYDropdown.AddOptions(invertedMouseYOptions);

            // Double Tap Dodge
            DoubleTapDodgeDropdown.ClearOptions();
            List<TMP_Dropdown.OptionData> doubleTapOptions = new List<TMP_Dropdown.OptionData>()
            {
                new TMP_Dropdown.OptionData("Off"),
                new TMP_Dropdown.OptionData("On")
            };
            DoubleTapDodgeDropdown.AddOptions(doubleTapOptions);

            // Weapon Switch Scroll Infinite
            WeaponSwitchScrollInfiniteDropdown.ClearOptions();
            List<TMP_Dropdown.OptionData> scrollInfinitepOptions = new List<TMP_Dropdown.OptionData>()
            {
                new TMP_Dropdown.OptionData("Off"),
                new TMP_Dropdown.OptionData("On")
            };
            WeaponSwitchScrollInfiniteDropdown.AddOptions(scrollInfinitepOptions);
        }

        public void Apply()
        {
            // Inverted Mouse X
            Save.ControlInvertedMouseX = InvertedMouseXDropdown.value;
            // Inverted Mouse Y
            Save.ControlInvertedMouseY = InvertedMouseYDropdown.value;

            // Mouse X
            Save.ControlMouseXSensitivity = MouseXSlider.value;

            // Mouse Y
            Save.ControlMouseYSensitivity = MouseYSlider.value;

            // Double Tap Dodge
            Save.ControlDoubleTapDodge = DoubleTapDodgeDropdown.value;

            // Weapon Scroll Infinite
            Save.ControlWeaponSwitchInfiniteScroll = WeaponSwitchScrollInfiniteDropdown.value;
        }

        public void LoadOptions()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ControlSettingsSave));

            using (FileStream stream = new FileStream(GetSavePath(fileName), FileMode.OpenOrCreate))
            {
                try
                {
                    Save = (ControlSettingsSave)serializer.Deserialize(stream);

                    // Controls
                    MouseXSlider.value = Save.ControlMouseXSensitivity;
                    MouseYSlider.value = Save.ControlMouseYSensitivity;

                    InvertedMouseXDropdown.value = Save.ControlInvertedMouseX;
                    InvertedMouseYDropdown.value = Save.ControlInvertedMouseY;

                    DoubleTapDodgeDropdown.value = Save.ControlDoubleTapDodge;
                    WeaponSwitchScrollInfiniteDropdown.value = Save.ControlWeaponSwitchInfiniteScroll;

                    Apply();
                }
                catch (Exception)
                {
                    stream.Close();
                    Apply();
                    SaveOptions();
                    Debug.Log("Could not open settings.");
                }
            }
        }

        public void SaveOptions()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(ControlSettingsSave));

            using (FileStream stream = new FileStream(GetSavePath(fileName), FileMode.Create))
            {
                try
                {
                    serializer.Serialize(stream, Save);
                    Debug.Log("Settings saved on " + Application.persistentDataPath + "/" + fileName + ".config");
                }
                catch (Exception)
                {
                    Debug.Log("Could not save settings.");
                }
            }

            LoadOptions();
        }

        private static string GetSavePath(string name)
        {
            return Path.Combine(Application.persistentDataPath, name + ".config");
        }
    }
}