using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace UnityEngine.UI
{
    public class SliderTextDisplay : MonoBehaviour
    {
        private Slider slider;
        public TextMeshProUGUI Text;

        [Header("")]
        public string Prefix;
        public string Suffix;

        [Header("")]
        public float Multiply = 1.0f;
        public bool RoundToInt = false;

        private void Awake()
        {
            slider = GetComponent<Slider>();
        }

        private void Update()
        {
            float val = RoundToInt ? Mathf.RoundToInt(slider.value * Multiply) : slider.value * Multiply;
            string t = Prefix + val + Suffix;
            Text.SetText(t);
        }
    }
}