using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WhiskerTales.UI
{
    /// <summary>
    /// Loading screen per spec:
    ///   - Hanok background (reused)
    ///   - Center: cat placeholder (white circle)
    ///   - Top-left: Whisker Tales text logo
    ///   - Bottom: "Loading..." text + paw indicator
    ///   - Breathing zoom on the cat: scale 1.00 -> 1.02 over 8s, loop (coroutine)
    ///   - 2-4 floating petals
    /// Auto-advances to WhiskerGameScene after a short delay (placeholder loader).
    /// </summary>
    public sealed class WhiskerLoadingScreen : MonoBehaviour
    {
        private const float BreathDuration = 8f;
        private const float BreathMin = 1.00f;
        private const float BreathMax = 1.02f;
        private const float AutoAdvanceSeconds = 3.5f;

        private RectTransform cat;
        private TextMeshProUGUI dotsLabel;

        private void Start()
        {
            var canvas = WhiskerScreenBootstrap.CreateScreenCanvas("LoadingCanvas",
                new Color(0.10f, 0.06f, 0.04f, 1f));
            var root = (RectTransform)canvas.transform;

            // Background tint (hanok bg placeholder — solid warm color until art is wired)
            var bg = WhiskerTheme.MakePanel(root, "Background",
                new Color(0.32f, 0.20f, 0.16f, 1f));
            WhiskerTheme.Stretch((RectTransform)bg.transform);

            // Top-left logo text
            var logo = WhiskerTheme.MakeText(root, "Logo", "Whisker Tales",
                52, WhiskerTheme.Cream, TextAlignmentOptions.Left);
            logo.fontStyle = FontStyles.Bold;
            WhiskerTheme.Anchor((RectTransform)logo.transform,
                new Vector2(0f, 1f), new Vector2(0f, 1f),
                new Vector2(60f, -120f), new Vector2(520f, -60f));

            // Center cat placeholder
            var catGo = WhiskerTheme.MakeCircle(root, "Cat", 460f, new Color(1f, 1f, 1f, 0.95f));
            cat = (RectTransform)catGo.transform;
            cat.anchoredPosition = new Vector2(0f, 0f);
            StartCoroutine(BreathingZoom());

            // Bottom "Loading..." + paw row
            var row = WhiskerTheme.MakePanel(root, "LoadingRow", new Color(0, 0, 0, 0));
            WhiskerTheme.Anchor((RectTransform)row.transform,
                new Vector2(0f, 0.15f), new Vector2(1f, 0.22f), Vector2.zero, Vector2.zero);
            dotsLabel = WhiskerTheme.MakeText(row.transform, "LoadingText", "Loading...",
                42, WhiskerTheme.Cream);
            WhiskerTheme.Stretch((RectTransform)dotsLabel.transform);
            StartCoroutine(AnimateDots());

            var pawRow = WhiskerTheme.MakePanel(root, "PawRow", new Color(0, 0, 0, 0));
            WhiskerTheme.Anchor((RectTransform)pawRow.transform,
                new Vector2(0f, 0.09f), new Vector2(1f, 0.14f), Vector2.zero, Vector2.zero);
            for (int i = 0; i < 5; i++)
            {
                var paw = WhiskerTheme.MakeCircle(pawRow.transform, $"Paw{i}", 36f, WhiskerTheme.SoftOrange);
                var prt = (RectTransform)paw.transform;
                float x = -240f + i * 120f;
                prt.anchoredPosition = new Vector2(x, 0f);
            }

            // Floating petals
            for (int i = 0; i < 4; i++) StartCoroutine(Petal(root, i));

            // Auto advance
            StartCoroutine(AutoAdvance());
        }

        private IEnumerator BreathingZoom()
        {
            float t = 0f;
            while (cat != null)
            {
                t += Time.unscaledDeltaTime;
                float u = (Mathf.Sin(t / BreathDuration * Mathf.PI * 2f) + 1f) * 0.5f;
                float s = Mathf.Lerp(BreathMin, BreathMax, u);
                cat.localScale = new Vector3(s, s, 1f);
                yield return null;
            }
        }

        private IEnumerator AnimateDots()
        {
            string[] frames = { "Loading", "Loading.", "Loading..", "Loading..." };
            int i = 0;
            while (dotsLabel != null)
            {
                dotsLabel.text = frames[i++ % frames.Length];
                yield return new WaitForSeconds(0.4f);
            }
        }

        private IEnumerator Petal(RectTransform parent, int seed)
        {
            var petal = WhiskerTheme.MakeCircle(parent, $"Petal{seed}",
                Random.Range(18f, 28f), new Color(0.95f, 0.78f, 0.78f, 0.75f));
            var rt = (RectTransform)petal.transform;
            while (petal != null)
            {
                float startX = Random.Range(-400f, 400f);
                float drift = Random.Range(-180f, 180f);
                float duration = Random.Range(6f, 10f);
                float t = 0f;
                while (t < duration)
                {
                    t += Time.unscaledDeltaTime;
                    float u = t / duration;
                    rt.anchoredPosition = new Vector2(startX + drift * u, 1200f - 2400f * u);
                    rt.localRotation = Quaternion.Euler(0f, 0f, u * 360f);
                    yield return null;
                }
            }
        }

        private IEnumerator AutoAdvance()
        {
            yield return new WaitForSecondsRealtime(AutoAdvanceSeconds);
            WhiskerScreens.Go(WhiskerScreens.Puzzle);
        }
    }
}
