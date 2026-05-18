using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WhiskerTales.UI
{
    /// <summary>
    /// Nabi Room screen per spec (냥이방.png):
    ///   - Hanok-interior background tint (placeholder)
    ///   - Center: cat placeholder (white circle, Nabi)
    ///   - Top: back / logo / camera / help row
    ///   - Top-left card: "Nabi / 나비" + Lv. 2
    ///   - Top-right card: +5 Affinity, +10 Coins indicators
    ///   - Affinity bar (heart + 50/100 + Lv.2)
    ///   - Three action buttons (Pet / Give Treat / Play)
    ///   - Tip row at the very bottom
    /// </summary>
    public sealed class WhiskerNabiRoomScreen : MonoBehaviour
    {
        private int affinity = 50;
        private const int AffinityMax = 100;
        private Image affinityFill;
        private TextMeshProUGUI affinityLabel;

        private void Start()
        {
            var canvas = WhiskerScreenBootstrap.CreateScreenCanvas("NabiRoomCanvas",
                new Color(0.34f, 0.24f, 0.18f, 1f));
            var root = (RectTransform)canvas.transform;

            var bg = WhiskerTheme.MakePanel(root, "Background",
                new Color(0.42f, 0.30f, 0.22f, 1f));
            WhiskerTheme.Stretch((RectTransform)bg.transform);

            BuildTopBar(root);
            BuildNabiCard(root);
            BuildRewardCard(root);
            BuildCatPlaceholder(root);
            BuildAffinityBar(root);
            BuildActionButtons(root);
            BuildTipRow(root);
            WhiskerBottomNavBar.AttachTo(root, WhiskerBottomNavBar.Tab.Nabi);
        }

        private void BuildTopBar(RectTransform parent)
        {
            // Back arrow (returns to puzzle)
            var back = MakeRoundButton(parent, "Back", "←", 0f, 1f, new Vector2(70f, -100f), 110f,
                () => WhiskerScreens.Go(WhiskerScreens.Puzzle));

            // Logo (center top)
            var logo = WhiskerTheme.MakeText(parent, "Logo", "Whisker Tales",
                52, WhiskerTheme.Cream);
            logo.fontStyle = FontStyles.Bold;
            WhiskerTheme.Anchor((RectTransform)logo.transform,
                new Vector2(0.3f, 1f), new Vector2(0.7f, 1f),
                new Vector2(0f, -160f), new Vector2(0f, -60f));

            // Camera button (top-right area)
            MakeRoundButton(parent, "Camera", "📷", 1f, 1f, new Vector2(-200f, -100f), 110f, () => { });
            // Help button (far top-right)
            MakeRoundButton(parent, "Help", "?", 1f, 1f, new Vector2(-70f, -100f), 110f, () => { });
        }

        private GameObject MakeRoundButton(RectTransform parent, string name, string glyph,
            float ax, float ay, Vector2 anchoredPos, float diameter, System.Action onClick)
        {
            var btn = WhiskerTheme.MakeCircle(parent, name, diameter, WhiskerTheme.PanelTint);
            var rt = (RectTransform)btn.transform;
            rt.anchorMin = new Vector2(ax, ay);
            rt.anchorMax = new Vector2(ax, ay);
            rt.anchoredPosition = anchoredPos;
            var g = WhiskerTheme.MakeText(btn.transform, "G", glyph, 48, WhiskerTheme.DeepBrown);
            WhiskerTheme.Stretch((RectTransform)g.transform);
            WhiskerButton.Attach(btn, onClick);
            return btn;
        }

        private void BuildNabiCard(RectTransform parent)
        {
            var card = WhiskerTheme.MakePanel(parent, "NabiCard", WhiskerTheme.PanelTint);
            var img = card.GetComponent<Image>();
            img.sprite = WhiskerTheme.RoundedSprite;
            img.type = Image.Type.Sliced;
            var rt = (RectTransform)card.transform;
            WhiskerTheme.Anchor(rt, new Vector2(0f, 0.78f), new Vector2(0f, 0.86f),
                new Vector2(50f, 0f), new Vector2(420f, 0f));

            var nameKo = WhiskerTheme.MakeText(card.transform, "Nabi", "Nabi  /  나비",
                48, WhiskerTheme.DeepBrown);
            nameKo.fontStyle = FontStyles.Bold;
            WhiskerTheme.Anchor((RectTransform)nameKo.transform,
                new Vector2(0f, 0.55f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);

            var lvl = WhiskerTheme.MakeText(card.transform, "Lv", "★ Lv. 2",
                36, WhiskerTheme.WarmBrown);
            WhiskerTheme.Anchor((RectTransform)lvl.transform,
                new Vector2(0f, 0f), new Vector2(1f, 0.50f), Vector2.zero, Vector2.zero);
        }

        private void BuildRewardCard(RectTransform parent)
        {
            var card = WhiskerTheme.MakePanel(parent, "RewardCard", WhiskerTheme.PanelTint);
            var img = card.GetComponent<Image>();
            img.sprite = WhiskerTheme.RoundedSprite;
            img.type = Image.Type.Sliced;
            var rt = (RectTransform)card.transform;
            WhiskerTheme.Anchor(rt, new Vector2(1f, 0.78f), new Vector2(1f, 0.88f),
                new Vector2(-460f, 0f), new Vector2(-50f, 0f));

            var heart = WhiskerTheme.MakeCircle(card.transform, "Heart", 44f, WhiskerTheme.HeartPink);
            ((RectTransform)heart.transform).anchoredPosition = new Vector2(-150f, 36f);

            var aff = WhiskerTheme.MakeText(card.transform, "Aff", "+5 Affinity",
                32, WhiskerTheme.DeepBrown, TextAlignmentOptions.Left);
            WhiskerTheme.Anchor((RectTransform)aff.transform,
                new Vector2(0f, 0.5f), new Vector2(1f, 1f),
                new Vector2(120f, 0f), new Vector2(-20f, 0f));

            var coin = WhiskerTheme.MakeCircle(card.transform, "Coin", 44f, WhiskerTheme.SoftOrange);
            ((RectTransform)coin.transform).anchoredPosition = new Vector2(-150f, -36f);

            var coinTxt = WhiskerTheme.MakeText(card.transform, "Coins", "+10 Coins",
                32, WhiskerTheme.DeepBrown, TextAlignmentOptions.Left);
            WhiskerTheme.Anchor((RectTransform)coinTxt.transform,
                new Vector2(0f, 0f), new Vector2(1f, 0.5f),
                new Vector2(120f, 0f), new Vector2(-20f, 0f));
        }

        private void BuildCatPlaceholder(RectTransform parent)
        {
            var cat = WhiskerTheme.MakeCircle(parent, "NabiCat", 580f, new Color(1f, 1f, 1f, 0.95f));
            var rt = (RectTransform)cat.transform;
            rt.anchoredPosition = new Vector2(0f, 120f);
        }

        private void BuildAffinityBar(RectTransform parent)
        {
            var bar = WhiskerTheme.MakePanel(parent, "AffinityBar", WhiskerTheme.PanelTint);
            var img = bar.GetComponent<Image>();
            img.sprite = WhiskerTheme.RoundedSprite;
            img.type = Image.Type.Sliced;
            var rt = (RectTransform)bar.transform;
            WhiskerTheme.Anchor(rt, new Vector2(0.05f, 0.31f), new Vector2(0.95f, 0.38f),
                Vector2.zero, Vector2.zero);

            var heart = WhiskerTheme.MakeCircle(bar.transform, "Heart", 56f, WhiskerTheme.HeartPink);
            var hr = (RectTransform)heart.transform;
            hr.anchorMin = new Vector2(0f, 0.5f);
            hr.anchorMax = new Vector2(0f, 0.5f);
            hr.anchoredPosition = new Vector2(50f, 0f);

            var cap = WhiskerTheme.MakeText(bar.transform, "Cap", "Affinity",
                28, WhiskerTheme.WarmBrown, TextAlignmentOptions.Left);
            WhiskerTheme.Anchor((RectTransform)cap.transform,
                new Vector2(0f, 0.55f), new Vector2(0.4f, 1f),
                new Vector2(100f, 0f), Vector2.zero);

            // Track + fill
            var track = WhiskerTheme.MakePanel(bar.transform, "Track", WhiskerTheme.AffinityBg);
            var trackImg = track.GetComponent<Image>();
            trackImg.sprite = WhiskerTheme.RoundedSprite;
            trackImg.type = Image.Type.Sliced;
            WhiskerTheme.Anchor((RectTransform)track.transform,
                new Vector2(0f, 0.20f), new Vector2(1f, 0.55f),
                new Vector2(100f, 0f), new Vector2(-180f, 0f));

            var fill = WhiskerTheme.MakeImage(track.transform, "Fill", WhiskerTheme.HeartPink);
            fill.GetComponent<Image>().sprite = WhiskerTheme.RoundedSprite;
            fill.GetComponent<Image>().type = Image.Type.Sliced;
            affinityFill = fill.GetComponent<Image>();
            affinityFill.rectTransform.anchorMin = new Vector2(0f, 0f);
            affinityFill.rectTransform.anchorMax = new Vector2((float)affinity / AffinityMax, 1f);
            affinityFill.rectTransform.offsetMin = Vector2.zero;
            affinityFill.rectTransform.offsetMax = Vector2.zero;

            affinityLabel = WhiskerTheme.MakeText(bar.transform, "Lbl",
                $"{affinity}/{AffinityMax}", 28, WhiskerTheme.Cream);
            WhiskerTheme.Anchor((RectTransform)affinityLabel.transform,
                new Vector2(0f, 0.20f), new Vector2(1f, 0.55f),
                new Vector2(100f, 0f), new Vector2(-180f, 0f));

            var lv = WhiskerTheme.MakeText(bar.transform, "Lv", "Lv. 2",
                30, WhiskerTheme.DeepBrown);
            lv.fontStyle = FontStyles.Bold;
            WhiskerTheme.Anchor((RectTransform)lv.transform,
                new Vector2(1f, 0.20f), new Vector2(1f, 0.80f),
                new Vector2(-170f, 0f), new Vector2(-20f, 0f));
        }

        private void RefreshAffinity()
        {
            if (affinityFill != null)
                affinityFill.rectTransform.anchorMax = new Vector2((float)affinity / AffinityMax, 1f);
            if (affinityLabel != null)
                affinityLabel.text = $"{affinity}/{AffinityMax}";
        }

        private void Interact(int gain)
        {
            affinity = Mathf.Min(AffinityMax, affinity + gain);
            RefreshAffinity();
        }

        private void BuildActionButtons(RectTransform parent)
        {
            var titles = new[] { ("쓰다듬기", "Pet", new Color(0.96f, 0.78f, 0.78f)),
                                 ("간식 주기", "Give Treat", new Color(0.96f, 0.83f, 0.56f)),
                                 ("놀아주기", "Play", new Color(0.78f, 0.88f, 0.66f)) };
            for (int i = 0; i < 3; i++)
            {
                float min = 0.04f + i * 0.32f;
                float max = min + 0.28f;
                var card = WhiskerTheme.MakePanel(parent, $"Action{i}", titles[i].Item3);
                var img = card.GetComponent<Image>();
                img.sprite = WhiskerTheme.RoundedSprite;
                img.type = Image.Type.Sliced;
                WhiskerTheme.Anchor((RectTransform)card.transform,
                    new Vector2(min, 0.14f), new Vector2(max, 0.28f), Vector2.zero, Vector2.zero);

                var ko = WhiskerTheme.MakeText(card.transform, "Ko", titles[i].Item1,
                    38, WhiskerTheme.DeepBrown);
                ko.fontStyle = FontStyles.Bold;
                WhiskerTheme.Anchor((RectTransform)ko.transform,
                    new Vector2(0f, 0.15f), new Vector2(1f, 0.45f), Vector2.zero, Vector2.zero);

                var en = WhiskerTheme.MakeText(card.transform, "En", titles[i].Item2,
                    28, WhiskerTheme.WarmBrown);
                WhiskerTheme.Anchor((RectTransform)en.transform,
                    new Vector2(0f, 0f), new Vector2(1f, 0.20f), Vector2.zero, Vector2.zero);

                int gain = 5 + i * 2;
                WhiskerButton.Attach(card, () => Interact(gain));
            }
        }

        private void BuildTipRow(RectTransform parent)
        {
            var tip = WhiskerTheme.MakeText(parent, "Tip",
                "💡 매일 다른 방법으로 교감하면 더 많은 보상을 받을 수 있어요!  ♥",
                26, WhiskerTheme.Cream);
            WhiskerTheme.Anchor((RectTransform)tip.transform,
                new Vector2(0f, 0.10f), new Vector2(1f, 0.13f), Vector2.zero, Vector2.zero);
        }
    }
}
