using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
#if SCPE
using UnityEngine.Rendering.PostProcessing;
using TextureParameter = UnityEngine.Rendering.PostProcessing.TextureParameter;
using BoolParameter = UnityEngine.Rendering.PostProcessing.BoolParameter;
using FloatParameter = UnityEngine.Rendering.PostProcessing.FloatParameter;
using IntParameter = UnityEngine.Rendering.PostProcessing.IntParameter;
using ColorParameter = UnityEngine.Rendering.PostProcessing.ColorParameter;
using MinAttribute = UnityEngine.Rendering.PostProcessing.MinAttribute;
#endif

namespace SCPE
{
#if !SCPE
    public class TiltShift : ScriptableObject {}
}
#else
    [System.Serializable]
    [PostProcess(typeof(TiltShiftRenderer), PostProcessEvent.AfterStack, "SC Post Effects/Blurring/Tilt Shift")]
    public class TiltShift : PostProcessEffectSettings
    {
        public enum TiltShiftMethod
        {
            Horizontal,
            Radial,
        }

        [Serializable]
        public sealed class TiltShifMethodParameter : ParameterOverride<TiltShiftMethod> { }

        [DisplayName("Method")]
        public TiltShifMethodParameter mode = new TiltShifMethodParameter { value = TiltShiftMethod.Horizontal };

        [Space]

        [Range(0f, 1f)]
        public FloatParameter areaSize = new FloatParameter { value = 1f };

        [Range(0f, 1f), Tooltip("The amount of blurring that must be performed")]
        public FloatParameter amount = new FloatParameter { value = 0.5f };

        public override bool IsEnabledAndSupported(PostProcessRenderContext context)
        {
            if (enabled.value)
            {
                if (amount == 0f || areaSize == 0f) { return false; }
                return true;
            }

            return false;
        }
    }

    internal sealed class TiltShiftRenderer : PostProcessEffectRenderer<TiltShift>
    {
        Shader shader;
        int screenCopyID;

        public override void Init()
        {
            shader = Shader.Find("Hidden/SC Post Effects/Tilt Shift");
            screenCopyID = Shader.PropertyToID("_ScreenCopyTexture");
        }

        public override void Render(PostProcessRenderContext context)
        {
            PropertySheet sheet = context.propertySheets.Get(shader);
            CommandBuffer cmd = context.command;

            sheet.properties.SetFloat("_Size", settings.areaSize);
            sheet.properties.SetFloat("_Amount", settings.amount);
            // sheet.properties.SetFloat("_HighQuality", settings.highQuality ? 1f : 0f);

            //Copy screen contents
            context.command.GetTemporaryRT(screenCopyID, context.width, context.height, 0, FilterMode.Bilinear, context.sourceFormat);
            cmd.BlitFullscreenTriangle(context.source, screenCopyID, sheet, (int)settings.mode.value);
            cmd.SetGlobalTexture("_BlurredTex", screenCopyID);

            // Render blurred texture in blend pass
            cmd.BlitFullscreenTriangle(context.source, context.destination, sheet, 2);

            cmd.ReleaseTemporaryRT(screenCopyID);
        }

    }
}
#endif