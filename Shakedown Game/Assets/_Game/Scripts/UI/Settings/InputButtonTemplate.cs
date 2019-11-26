using TMPro;
using UnityEngine.UI;
using UnityEngine;
using Rewired;

namespace Settings.Template
{
    public class InputButtonTemplate : MonoBehaviour
    {
        public Button RemapButton;
        public TextMeshProUGUI RemapButtonText;
        public TextMeshProUGUI LabelText;

        public InputAction InputAction;
        public AxisRange AxisRange;

        public bool IsKeyboard;

        public void SetupInputButton(InputAction input, AxisRange range, string action)
        {
            InputAction = input;
            AxisRange = range;

            LabelText.SetText(action);
        }
    }
}