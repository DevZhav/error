using UnityEditor;
using UnityEngine;
#if SCPE
using UnityEngine.Rendering.PostProcessing;
using UnityEditor.Rendering.PostProcessing;
#endif

namespace SCPE
{
#if !SCPE
    public sealed class TiltShiftEditor : Editor {} }
#else
    [PostProcessEditor(typeof(TiltShift))]
    public sealed class TiltShiftEditor : PostProcessEffectEditor<TiltShift>
    {

        SerializedParameterOverride mode;
        SerializedParameterOverride areaSize;
        SerializedParameterOverride amount;

        public override void OnEnable()
        {
            mode = FindParameterOverride(x => x.mode);
            areaSize = FindParameterOverride(x => x.areaSize);
            amount = FindParameterOverride(x => x.amount);
        }

        public override string GetDisplayTitle()
        {
            return base.GetDisplayTitle() + SCPE_GUI.ModeTitle(mode);
        }

        public override void OnInspectorGUI()
        {
            SCPE_GUI.DisplayDocumentationButton("tilt-shift");

            PropertyField(mode);
            PropertyField(areaSize);
            PropertyField(amount);
        }
    }
}
#endif