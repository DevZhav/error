using System.Collections;
using UnityEngine;

namespace Effects
{
    public class BulletTrail : MonoBehaviour
    {
        public Color StartColor;
        public Color EndColor;
        public Color FadeColor;
        
        private LineRenderer LineRenderer;
        private bool FadeLine;

        private void Start()
        {
            LineRenderer = GetComponent<LineRenderer>();
        }

        private void OnEnable()
        {
            if (LineRenderer == null)
                LineRenderer = GetComponent<LineRenderer>();

            LineRenderer.startColor = StartColor;
            LineRenderer.endColor = EndColor;
            StartCoroutine(DelayBeforeFade());
        }

        private void Update()
        {
            if (!FadeLine)
                return;

            LineRenderer.startColor = Color.Lerp(LineRenderer.startColor, FadeColor, 12 * Time.deltaTime);
            LineRenderer.endColor = Color.Lerp(LineRenderer.endColor, FadeColor, 12 * Time.deltaTime);
        }

        private IEnumerator DelayBeforeFade()
        {
            FadeLine = false;
            yield return new WaitForSeconds(0.2f);
            FadeLine = true;
        }
    }
}