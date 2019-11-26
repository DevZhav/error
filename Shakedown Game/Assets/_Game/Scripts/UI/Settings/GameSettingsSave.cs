using System.Collections.Generic;
using UnityEngine;

namespace Settings
{
    public class DisplaySettingsSave
    {
        // Display
        public int DisplayScreenMode;
        public int DisplayResoultion;
        public int DisplayFPSCap;
        public int DisplayVSync;
        // --------
        public int FieldOfView;
        public int GraphicsPreset;
        // --------
        public int AntiAliasing;
        public int SunShaft;
        public int ColorGradingMode;
        public int Saturation;
        public int Contrast;

        public DisplaySettingsSave()
        {
            // Display
            DisplayScreenMode = 3;
            DisplayResoultion = 0;
            DisplayFPSCap = 0;
            DisplayVSync = 1;
            // -------
            FieldOfView = 70;
            GraphicsPreset = 5;
            // -------
            AntiAliasing = 1;
            SunShaft = 0;
            ColorGradingMode = 1;
            Saturation = 10;
            Contrast = 5;
        }
    }

    public class SoundSettingsSave
    {
        // Sound
        public int SoundMaster;
        public int SoundMusic;
        public int SoundSFX;
        public int SoundAnnouncer;

        public SoundSettingsSave()
        {
            // Sound
            SoundMaster = 100;
            SoundMusic = 100;
            SoundSFX = 100;
            SoundAnnouncer = 100;
        }
    }

    public class ControlSettingsSave
    {
        // Options
        public float ControlMouseXSensitivity;
        public float ControlMouseYSensitivity;
        // -------
        public int ControlInvertedMouseX;
        public int ControlInvertedMouseY;
        // -------
        public int ControlDoubleTapDodge;
        public int ControlWeaponSwitchInfiniteScroll;

        public ControlSettingsSave()
        {
            // Options
            ControlMouseXSensitivity = 20;
            ControlMouseYSensitivity = 20;

            ControlInvertedMouseX = 0;
            ControlInvertedMouseY = 0;

            ControlDoubleTapDodge = 0;
            ControlWeaponSwitchInfiniteScroll = 0;
        }
    }

    public class InputSettingsSave
    {
        public InputSettingsSave()
        {
        }
    }
}