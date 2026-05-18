using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WhiskerTales.UI
{
    /// <summary>
    /// Sleep mode screen per spec (수면모드.png):
    ///   - Dark hanok background (80% overlay)
    ///   - Top: small Whisker Tales mark
    ///   - Mid-top: moon icon + "고양이가 자고 있어요" + "깨우지 않게 폰을 내려놓아 주세요"
    ///   - Center: sleeping cat placeholder
    ///   - Bottom: ASMR player (prev / play-pause / next + volume slider)
    ///   - After 30s of no input, dim the whole screen to alpha 0.5 (linearly over 4s)
    /// </summary>
    public sealed class WhiskerSleepModeScreen : MonoBehaviour
    {
        private const float IdleSeconds = 30f;
        private const float DimDuration = 4f;
        private const float DimTargetAlpha = 0.5f;

        private CanvasGroup root;
        private float lastInputTime;
        private bool dimmed;
        private bool playing = true;
        private float volume = 0.30f;
        private TextMeshProUGUI playPauseGlyph;
        private RectTransform volumeKnob;
        private TextMeshProUGUI volumeText;

        private void Start()
        {
            var canvas = WhiskerScreenBootstrap.CreateScreenCanvas("SleepModeCanvas",
                new Color(0.02f, 0.02f, 0.04f, 1f));
            var rootGo = canvas.gameObject;
            root = rootGo.AddComponent<CanvasGroup>();
            var rt = (RectTransform)canvas.transform;

            // Dark overlay (80%)
            var bg = WhiskerTheme.MakePanel(rt, "DarkBg", new Color(0.04f, 0.04f, 0.06f, 1f));
            WhiskerTheme.Stretch((RectTransform)bg.transform);
            var dim = WhiskerTheme.MakePanel(rt, "Overlay", new Color(0f, 0f, 0f, 0.80f));
            WhiskerTheme.Stretch((RectTransform)dim.transform);

            BuildTopMark(rt);
            BuildHeader(rt);
            BuildSleepingCat(rt);
            BuildAsmrPlayer(rt);
            BuildFooter(rt);
            WhiskerBottomNavBar.AttachTo(rt, WhiskerBottomNavBar.Tab.Sleep);

            lastInputTime = Time.unscaledTime;
            StartCoroutine(IdleDimmer());
        }

        private void Update()
        {
            if (Input.anyKey || Input.touchCount > 0 || Input.GetMouseButton(0))
                ResetIdle();
        }

        private void ResetIdle()
        {
            lastInputTime = Time.unscaledTime;
            if (dimmed) StartCoroutine(FadeTo(1f, DimDuration * 0.25f));
            dimmed = false;
        }

        private IEnumerator IdleDimmer()
        {
            while (root != null)
            {
                yield return new WaitForSecondsRealtime(1f);
                if (!dimmed && Time.unscaledTime - lastInputTime > IdleSeconds)
                {
                    dimmed = true;
                    yield return FadeTo(DimTargetAlpha, DimDuration);
                }
            }
        }

        private IEnumerator FadeTo(float target, float dur)
        {
            float start = root.alpha;
            float t = 0f;
            while (t < dur)
            {
                t += Time.unscaledDeltaTime;
                root.alpha = Mathf.Lerp(start, target, Mathf.Clamp01(t / dur));
                yield return null;
            }
            root.alpha = target;
        }

        private void BuildTopMark(RectTransform parent)
        {
            var mark = WhiskerTheme.MakeText(parent, "Mark", "Whisker Tales",
                30, new Color(0.85f, 0.78f, 0.66f, 0.9f));
            WhiskerTheme.Anchor((RectTransform)mark.transform,
                new Vector2(0f, 0.93f), new Vector2(1f, 0.97f), Vector2.zero, Vector2.zero);
        }

        private void BuildHeader(RectTransform parent)
        {
            var moon = WhiskerTheme.MakeText(parent, "Moon", "🌙",
                64, new Color(0.92f, 0.88f, 0.70f));
            WhiskerTheme.Anchor((RectTransform)moon.transform,
                new Vector2(0f, 0.82f), new Vector2(1f, 0.88f), Vector2.zero, Vector2.zero);

            var title = WhiskerTheme.MakeText(parent, "Title",
                "고양이가 자고 있어요", 60, new Color(0.92f, 0.88f, 0.74f));
            WhiskerTheme.Anchor((RectTransform)title.transform,
                new Vector2(0f, 0.74f), new Vector2(1f, 0.81f), Vector2.zero, Vector2.zero);

            var sub = WhiskerTheme.MakeText(parent, "Sub",
                "깨우지 않게 폰을 내려놓아 주세요", 32,
                new Color(0.80f, 0.74f, 0.62f));
            WhiskerTheme.Anchor((RectTransform)sub.transform,
                new Vector2(0f, 0.68f), new Vector2(1f, 0.73f), Vector2.zero, Vector2.zero);
        }

        private void BuildSleepingCat(RectTransform parent)
        {
            var cat = WhiskerTheme.MakeCircle(parent, "SleepingCat",
                520f, new Color(0.42f, 0.36f, 0.30f, 0.85f));
            ((RectTransform)cat.transform).anchoredPosition = new Vector2(0f, -200f);
        }

        private void BuildAsmrPlayer(RectTransform parent)
        {
            var card = WhiskerTheme.MakePanel(parent, "AsmrCard",
                new Color(0.08f, 0.07f, 0.06f, 0.80f));
            var img = card.GetComponent<Image>();
            img.sprite = WhiskerTheme.RoundedSprite;
            img.type = Image.Type.Sliced;
            WhiskerTheme.Anchor((RectTransform)card.transform,
                new Vector2(0.04f, 0.14f), new Vector2(0.96f, 0.34f),
                Vector2.zero, Vector2.zero);

            // Title
            var trackKo = WhiskerTheme.MakeText(card.transform, "Ko",
                "모닥불 + 고양이 골골송", 38, new Color(0.92f, 0.87f, 0.74f));
            WhiskerTheme.Anchor((RectTransform)trackKo.transform,
                new Vector2(0f, 0.72f), new Vector2(1f, 0.88f), Vector2.zero, Vector2.zero);
            var trackEn = WhiskerTheme.MakeText(card.transform, "En",
                "✦ Campfire + Cat Purring ✦", 26, new Color(0.78f, 0.72f, 0.62f));
            trackEn.fontStyle = FontStyles.Italic;
            WhiskerTheme.Anchor((RectTransform)trackEn.transform,
                new Vector2(0f, 0.60f), new Vector2(1f, 0.72f), Vector2.zero, Vector2.zero);

            // Volume slider
            var sliderTrack = WhiskerTheme.MakePanel(card.transform, "VolTrack",
                new Color(0.45f, 0.40f, 0.30f, 0.7f));
            var sliderImg = sliderTrack.GetComponent<Image>();
            sliderImg.sprite = WhiskerTheme.RoundedSprite;
            sliderImg.type = Image.Type.Sliced;
            WhiskerTheme.Anchor((RectTransform)sliderTrack.transform,
                new Vector2(0.12f, 0.45f), new Vector2(0.88f, 0.50f),
                Vector2.zero, Vector2.zero);

            volumeKnob = (RectTransform)WhiskerTheme.MakeCircle(card.transform, "VolKnob",
                40f, new Color(0.88f, 0.80f, 0.66f)).transform;
            volumeKnob.anchorMin = new Vector2(0.12f, 0.475f);
            volumeKnob.anchorMax = new Vector2(0.12f, 0.475f);
            UpdateVolumeKnob();

            volumeText = WhiskerTheme.MakeText(card.transform, "Vol",
                "30%", 26, new Color(0.85f, 0.78f, 0.66f));
            WhiskerTheme.Anchor((RectTransform)volumeText.transform,
                new Vector2(0f, 0.33f), new Vector2(1f, 0.42f), Vector2.zero, Vector2.zero);

            // Player buttons
            var prev = WhiskerTheme.MakeCircle(card.transform, "Prev",
                70f, new Color(0.20f, 0.16f, 0.12f, 0.9f));
            ((RectTransform)prev.transform).anchorMin = new Vector2(0.30f, 0.20f);
            ((RectTransform)prev.transform).anchorMax = new Vector2(0.30f, 0.20f);
            var pi = WhiskerTheme.MakeText(prev.transform, "I", "⏮", 36, WhiskerTheme.Cream);
            WhiskerTheme.Stretch((RectTransform)pi.transform);
            WhiskerButton.Attach(prev, () => ResetIdle());

            var play = WhiskerTheme.MakeCircle(card.transform, "Play",
                100f, new Color(0.18f, 0.15f, 0.12f, 0.95f));
            ((RectTransform)play.transform).anchorMin = new Vector2(0.50f, 0.20f);
            ((RectTransform)play.transform).anchorMax = new Vector2(0.50f, 0.20f);
            playPauseGlyph = WhiskerTheme.MakeText(play.transform, "G",
                playing ? "⏸" : "▶", 52, WhiskerTheme.Cream);
            WhiskerTheme.Stretch((RectTransform)playPauseGlyph.transform);
            WhiskerButton.Attach(play, () =>
            {
                playing = !playing;
                playPauseGlyph.text = playing ? "⏸" : "▶";
                ResetIdle();
            });

            var next = WhiskerTheme.MakeCircle(card.transform, "Next",
                70f, new Color(0.20f, 0.16f, 0.12f, 0.9f));
            ((RectTransform)next.transform).anchorMin = new Vector2(0.70f, 0.20f);
            ((RectTransform)next.transform).anchorMax = new Vector2(0.70f, 0.20f);
            var ni = WhiskerTheme.MakeText(next.transform, "I", "⏭", 36, WhiskerTheme.Cream);
            WhiskerTheme.Stretch((RectTransform)ni.transform);
            WhiskerButton.Attach(next, () => ResetIdle());

            // ASMR badge top-left
            var badge = WhiskerTheme.MakeText(card.transform, "Asmr", "♪ ASMR",
                24, new Color(0.78f, 0.72f, 0.60f), TextAlignmentOptions.Left);
            WhiskerTheme.Anchor((RectTransform)badge.transform,
                new Vector2(0.03f, 0.88f), new Vector2(0.3f, 1f), Vector2.zero, Vector2.zero);

            // Heart top-right
            var heart = WhiskerTheme.MakeText(card.transform, "Heart", "♡",
                32, new Color(0.78f, 0.72f, 0.60f), TextAlignmentOptions.Right);
            WhiskerTheme.Anchor((RectTransform)heart.transform,
                new Vector2(0.7f, 0.88f), new Vector2(0.97f, 1f), Vector2.zero, Vector2.zero);
        }

        private void UpdateVolumeKnob()
        {
            if (volumeKnob == null) return;
            float tMin = 0.12f, tMax = 0.88f;
            float x = Mathf.Lerp(tMin, tMax, volume);
            volumeKnob.anchorMin = new Vector2(x, 0.475f);
            volumeKnob.anchorMax = new Vector2(x, 0.475f);
        }

        private void BuildFooter(RectTransform parent)
        {
            var f = WhiskerTheme.MakeText(parent, "Footer",
                "☾ 편안한 밤 되세요, 집사님 ♡", 26,
                new Color(0.78f, 0.72f, 0.62f));
            WhiskerTheme.Anchor((RectTransform)f.transform,
                new Vector2(0f, 0.10f), new Vector2(1f, 0.13f), Vector2.zero, Vector2.zero);
        }
    }
}
