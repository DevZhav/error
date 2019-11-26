using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Xml.Serialization;
using TMPro;
using UnityEngine.Rendering.PostProcessing;
using SCPE;

namespace Settings
{
    public class DisplaySettings : MonoBehaviour
    {
        public DisplaySettingsSave Save;
        const string fileName = "display_settings";

        [Header("Display")]
        public TMP_Dropdown ScreenModeDropdown;
        public TMP_Dropdown ResolutionDropdown;
        public TMP_Dropdown FPSCapDropdown;
        public TMP_Dropdown VSyncDropdown;
        [Space(10)]
        public Slider FieldOfViewSlider;
        public TMP_Dropdown GraphicsPresetDropdown;
        [Space(10)]
        public TMP_Dropdown AntiAliasingDropdown;
        public TMP_Dropdown SunShaftDropdown;
        public TMP_Dropdown ColorGradingModeDropdown;
        public Slider SaturationSlider;
        public Slider ContrastSlider;

        List<Vector3> resolutions = new List<Vector3>();
        PostProcessVolume volume;
        PostProcessLayer layer;

        public void Setup(DisplaySettingsSave save)
        {
            Save = save;
            volume = FindObjectOfType<PostProcessVolume>();
            layer = FindObjectOfType<PostProcessLayer>();

            // Screen Mode Dropdown
            ScreenModeDropdown.ClearOptions();

            for (int i = 0; i < Enum.GetValues(typeof(FullScreenMode)).Length; i++)
            {
                ScreenModeDropdown.options.Add(new TMP_Dropdown.OptionData(Enum.GetNames(typeof(FullScreenMode))[i]));

                //if (Screen.fullScreenMode == (FullScreenMode)i)
                //    ScreenModeDropdown.value = i;
            }

            // Resolution Dropdown
            ResolutionDropdown.ClearOptions();
            resolutions.Clear();
            List<Vector2> resolutionsCheck = new List<Vector2>();
            for (int i = Screen.resolutions.Length - 1; i > 0; i--)
            {
                var res = new Vector3(Screen.resolutions[i].width, Screen.resolutions[i].height, Screen.resolutions[i].refreshRate);

                if (!resolutionsCheck.Contains(res))
                {
                    resolutionsCheck.Add(res);
                    resolutions.Add(res);
                    ResolutionDropdown.options.Add(new TMP_Dropdown.OptionData(Screen.resolutions[i].width + " x " + Screen.resolutions[i].height));
                    continue;
                }
            }
            ResolutionDropdown.value = 0;

            // FPS Cap
            FPSCapDropdown.ClearOptions();
            List<TMP_Dropdown.OptionData> fpsOptions = new List<TMP_Dropdown.OptionData>()
            {
                new TMP_Dropdown.OptionData("300"),
                new TMP_Dropdown.OptionData("240"),
                new TMP_Dropdown.OptionData("144"),
                new TMP_Dropdown.OptionData("120"),
                new TMP_Dropdown.OptionData("90"),
                new TMP_Dropdown.OptionData("60"),
            };
            FPSCapDropdown.AddOptions(fpsOptions);

            // VSync
            VSyncDropdown.ClearOptions();
            List<TMP_Dropdown.OptionData> vsyncOptions = new List<TMP_Dropdown.OptionData>()
            {
                new TMP_Dropdown.OptionData("On"),
                new TMP_Dropdown.OptionData("Off")
            };
            VSyncDropdown.AddOptions(vsyncOptions);
            VSyncDropdown.value = 1;

            // Graphics Preset
            GraphicsPresetDropdown.ClearOptions();
            for (int i = 0; i < QualitySettings.names.Length; i++)
            {
                GraphicsPresetDropdown.options.Add(new TMP_Dropdown.OptionData(QualitySettings.names[i]));
            }

            // Anti Aliasing
            AntiAliasingDropdown.ClearOptions();
            List<TMP_Dropdown.OptionData> antiAliasingOptions = new List<TMP_Dropdown.OptionData>()
            {
                new TMP_Dropdown.OptionData("None"),
                new TMP_Dropdown.OptionData("FXAA"),
                new TMP_Dropdown.OptionData("SMAA"),
                new TMP_Dropdown.OptionData("TAA")
            };
            AntiAliasingDropdown.AddOptions(antiAliasingOptions);

            // Sun Shafts
            SunShaftDropdown.ClearOptions();
            List<TMP_Dropdown.OptionData> sunShaftOptions = new List<TMP_Dropdown.OptionData>()
            {
                new TMP_Dropdown.OptionData("On"),
                new TMP_Dropdown.OptionData("Off")
            };
            SunShaftDropdown.AddOptions(sunShaftOptions);

            // Color Grading Mode
            ColorGradingModeDropdown.ClearOptions();
            List<TMP_Dropdown.OptionData> colorGradingModeOptions = new List<TMP_Dropdown.OptionData>()
            {
                new TMP_Dropdown.OptionData("Low Dynamic Range"),
                new TMP_Dropdown.OptionData("High Dynamic Range")
            };
            ColorGradingModeDropdown.AddOptions(colorGradingModeOptions);
            ColorGradingModeDropdown.value = 1;
        }

        public void Apply()
        {
            volume = FindObjectOfType<PostProcessVolume>();
            layer = FindObjectOfType<PostProcessLayer>();
            // Just an extra method incase we don't have a layer, and we're in game and the camera hasn't loaded yet
            if (layer == null && Player.Networker.Instance != null)
                layer = Player.Networker.Instance.Controller.Camera.Cam.GetComponent<PostProcessLayer>();

            // Resolution
            Screen.SetResolution((int)resolutions[ResolutionDropdown.value].x, (int)resolutions[ResolutionDropdown.value].y, (FullScreenMode)ScreenModeDropdown.value, (int)resolutions[ResolutionDropdown.value].z);
            Save.DisplayScreenMode = ScreenModeDropdown.value;
            Save.DisplayResoultion = ResolutionDropdown.value;

            // FPS Cap
            Application.targetFrameRate = int.Parse(FPSCapDropdown.options[FPSCapDropdown.value].text);
            Save.DisplayFPSCap = FPSCapDropdown.value;

            // Vsync
            QualitySettings.vSyncCount = VSyncDropdown.value == 1 ? 1 : 0;
            Save.DisplayVSync = VSyncDropdown.value;

            // Field of View
            Save.FieldOfView = (int)FieldOfViewSlider.value;

            // Graphics Preset
            QualitySettings.SetQualityLevel(GraphicsPresetDropdown.value);
            Save.GraphicsPreset = GraphicsPresetDropdown.value;

            if (volume != null)
            {
                // Sun Shafts
                volume.profile.TryGetSettings(out Sunshafts shafts);
                shafts.active = SunShaftDropdown.value == 0;

                // Color Grading Mode
                volume.profile.TryGetSettings(out ColorGrading color);
                color.gradingMode.value = (GradingMode)ColorGradingModeDropdown.value;
                // Saturation
                color.saturation.value = GameMath.Remap(SaturationSlider.value, 0, 100, 10, 30);
                // Contrast
                color.contrast.value = GameMath.Remap(ContrastSlider.value, 0, 100, 5, 25);
            }

            // --- VOLUME SAVING --- //
            // We want to save Volume values regardless of if we can apply it or not
            Save.SunShaft = SunShaftDropdown.value;
            Save.ColorGradingMode = ColorGradingModeDropdown.value;
            Save.Saturation = (int)GameMath.Remap(SaturationSlider.value, 0, 100, 10, 30);
            Save.Contrast = (int)GameMath.Remap(ContrastSlider.value, 0, 100, 5, 25);

            if (layer != null)
            {
                // Anti Aliasing
                layer.antialiasingMode = (PostProcessLayer.Antialiasing)AntiAliasingDropdown.value;
            }

            // --- LAYER SAVING --- //
            // We want to save Layer values regardless of if we can set it or not
            Save.AntiAliasing = AntiAliasingDropdown.value;
        }

        public void LoadOptions()
        {
            XmlSerializer serializer = new XmlSerializer(typeof(DisplaySettingsSave));

            using (FileStream stream = new FileStream(GetSavePath(fileName), FileMode.OpenOrCreate))
            {
                try
                {
                    Save = (DisplaySettingsSave)serializer.Deserialize(stream);

                    //Display 
                    ScreenModeDropdown.value = Save.DisplayScreenMode;
                    ResolutionDropdown.value = Save.DisplayResoultion;
                    FPSCapDropdown.value = Save.DisplayFPSCap;
                    VSyncDropdown.value = Save.DisplayVSync;

                    FieldOfViewSlider.value = Save.FieldOfView;
                    GraphicsPresetDropdown.value = Save.GraphicsPreset;

                    AntiAliasingDropdown.value = Save.AntiAliasing;
                    SunShaftDropdown.value = Save.SunShaft;
                    ColorGradingModeDropdown.value = Save.ColorGradingMode;
                    SaturationSlider.value = GameMath.Remap(Save.Saturation, 10, 30, 0, 100);
                    ContrastSlider.value = GameMath.Remap(Save.Contrast, 5, 25, 0, 100);

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
            XmlSerializer serializer = new XmlSerializer(typeof(DisplaySettingsSave));

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