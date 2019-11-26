using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using TMPro;
using UnityEngine.Audio;

namespace Settings
{
    public class SoundSettings : MonoBehaviour
    {
        public SoundSettingsSave Save;
        const string fileName = "sound_settings";
        public AudioMixer Mixer;

        [Header("Sound")]
        public Slider MasterSlider;
        public Slider MusicSlider;
        public Slider SoundEffectsSlider;
        public Slider AnnouncerSlider;

        public void Setup(SoundSettingsSave save)
        {
            Save = save;
        }

        public void Apply()
        {
            Mixer.SetFloat("masterVolume", AdjustedVolume(MasterSlider.value));
            Save.SoundMaster = (int)MasterSlider.value;

            Mixer.SetFloat("musicVolume", AdjustedVolume(MusicSlider.value));
            Save.SoundMusic = (int)MusicSlider.value;

            Mixer.SetFloat("soundEffectsVolume", AdjustedVolume(SoundEffectsSlider.value));
            Save.SoundSFX = (int)SoundEffectsSlider.value;

            Mixer.SetFloat("announcerVolume", AdjustedVolume(AnnouncerSlider.value));
            Save.SoundAnnouncer = (int)AnnouncerSlider.value;
        }

        private float AdjustedVolume(float sliderValue)
        {
            sliderValue = Mathf.Clamp(sliderValue / 100, 0.0001f, 1.0f);
            return Mathf.Log10(sliderValue) * 20;
        }

        public void LoadOptions()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(SoundSettingsSave));

            using (FileStream stream = new FileStream(GetSavePath(fileName), FileMode.OpenOrCreate))
            {
                try
                {
                    Save = (SoundSettingsSave)serializer.Deserialize(stream);

                    // Sound
                    MasterSlider.value = Save.SoundMaster;
                    MusicSlider.value = Save.SoundMusic;
                    SoundEffectsSlider.value = Save.SoundSFX;
                    AnnouncerSlider.value = Save.SoundAnnouncer;

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
            XmlSerializer serializer = new XmlSerializer(typeof(SoundSettingsSave));

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