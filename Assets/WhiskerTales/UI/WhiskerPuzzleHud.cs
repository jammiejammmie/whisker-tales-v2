using System.Collections;
using GameVanilla.Game.Common;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace WhiskerTales.UI
{
    /// <summary>
    /// Overlay HUD for WhiskerGameScene per the puzzle mockup (게임.png):
    ///   Top 18%   : back arrow + Whisker Tales mark + Korean/English copy band
    ///   Center 52%: GameBoard area, with side panels (Stage/Moves/Goal + Boosters/Settings)
    ///               and a cat placeholder at bottom-left
    ///   Bottom 12%: Shop / CatRoom / Home / Gallery / Friends nav bar
    ///
    /// The HUD is injected at runtime by WhiskerScreenBootstrap, so the .unity scene
    /// itself stays minimal. Stage/Moves/Goal numbers are sampled from the Kit
    /// GameBoard each frame via reflection-free public hooks so the HUD stays in
    /// sync without modifying Vendor code.
    /// </summary>
    public sealed class WhiskerPuzzleHud : MonoBehaviour
    {
        private TextMeshProUGUI stageLabel;
        private TextMeshProUGUI movesLabel;
        private TextMeshProUGUI goal1Label;
        private TextMeshProUGUI goal2Label;
        private GameBoard board;
        private int lastStage = -1;
        private int lastMoves = -1;

        private void Start()
        {
            BuildHud();
            StartCoroutine(WaitForBoard());
        }

        private IEnumerator WaitForBoard()
        {
            while (board == null)
            {
                board = FindObjectOfType<GameBoard>();
                yield return null;
            }
        }

        private void Update()
        {
            if (board == null) return;
            int level = board.level != null ? board.level.id : 0;
            if (level != lastStage)
            {
                lastStage = level;
                if (stageLabel != null) stageLabel.text = level.ToString();
            }
            int moves = board.currentLimit;
            if (moves != lastMoves)
            {
                lastMoves = moves;
                if (movesLabel != null) movesLabel.text = moves.ToString();
            }
        }

        private void BuildHud()
        {
            var canvas = WhiskerScreenBootstrap.CreateScreenCanvas("WhiskerHudCanvas");
            canvas.sortingOrder = 100; // above the GameBoard sprites
            var canvasRt = (RectTransform)canvas.transform;

            BuildTopBar(canvasRt);
            BuildLeftPanel(canvasRt);
            BuildRightPanel(canvasRt);
            BuildBottomNav(canvasRt);
            BuildCatPlaceholder(canvasRt);
        }

        // Top 18% of the screen.
        private void BuildTopBar(RectTransform parent)
        {
            var top = WhiskerTheme.MakePanel(parent, "TopBar", new Color(0, 0, 0, 0));
            var rt = (RectTransform)top.transform;
            WhiskerTheme.Anchor(rt, new Vector2(0f, 0.82f), new Vector2(1f, 1f), Vector2.zero, Vector2.zero);

            // Back arrow (top-left)
            var back = WhiskerTheme.MakePanel(top.transform, "Back", WhiskerTheme.PanelTint);
            var backRt = (RectTransform)back.transform;
            back.GetComponent<Image>().sprite = WhiskerTheme.RoundedSprite;
            back.GetComponent<Image>().type = Image.Type.Sliced;
            WhiskerTheme.Anchor(backRt, new Vector2(0f, 0.5f), new Vector2(0f, 0.5f),
                new Vector2(36f, -54f), new Vector2(36f + 108f, 54f));
            var backArrow = WhiskerTheme.MakeText(back.transform, "Arrow", "←", 72, WhiskerTheme.DeepBrown);
            WhiskerTheme.Stretch((RectTransform)backArrow.transform);
            WhiskerButton.Attach(back, () => WhiskerScreens.Go(WhiskerScreens.NabiRoom));

            // Logo (text placeholder)
            var logo = WhiskerTheme.MakeText(top.transform, "Logo", "Whisker Tales",
                64, WhiskerTheme.Cream, TextAlignmentOptions.Left);
            logo.fontStyle = FontStyles.Bold;
            WhiskerTheme.Anchor((RectTransform)logo.transform,
                new Vector2(0f, 0.55f), new Vector2(0.6f, 0.95f),
                new Vector2(180f, 0f), new Vector2(0f, 0f));

            // Korean copy + English copy (sentimental band)
            var koCopy = WhiskerTheme.MakeText(top.transform, "KoreanCopy",
                "오늘 당신의 시간은 어떤 빛깔이었나요?", 44, WhiskerTheme.Cream);
            WhiskerTheme.Anchor((RectTransform)koCopy.transform,
                new Vector2(0f, 0.18f), new Vector2(1f, 0.50f), Vector2.zero, Vector2.zero);

            var enCopy = WhiskerTheme.MakeText(top.transform, "EnglishCopy",
                "What color was your day today?", 32, WhiskerTheme.Cream);
            enCopy.fontStyle = FontStyles.Italic;
            WhiskerTheme.Anchor((RectTransform)enCopy.transform,
                new Vector2(0f, 0.02f), new Vector2(1f, 0.22f), Vector2.zero, Vector2.zero);
        }

        // Left side: Stage / Moves / Goal panel
        private void BuildLeftPanel(RectTransform parent)
        {
            var panel = WhiskerTheme.MakePanel(parent, "LeftPanel", WhiskerTheme.PanelTint);
            var img = panel.GetComponent<Image>();
            img.sprite = WhiskerTheme.RoundedSprite;
            img.type = Image.Type.Sliced;
            var rt = (RectTransform)panel.transform;
            WhiskerTheme.Anchor(rt, new Vector2(0f, 0.30f), new Vector2(0f, 0.62f),
                new Vector2(20f, 0f), new Vector2(180f, 0f));

            // Stage block
            WhiskerTheme.MakeText(panel.transform, "StageCap", "STAGE", 28, WhiskerTheme.WarmBrown).rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 20f, 40f);
            stageLabel = WhiskerTheme.MakeText(panel.transform, "StageNum", "127", 52, WhiskerTheme.DeepBrown);
            stageLabel.fontStyle = FontStyles.Bold;
            stageLabel.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 56f, 60f);

            // Moves block
            WhiskerTheme.MakeText(panel.transform, "MovesCap", "MOVES", 28, WhiskerTheme.WarmBrown).rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 140f, 40f);
            movesLabel = WhiskerTheme.MakeText(panel.transform, "MovesNum", "18", 52, WhiskerTheme.DeepBrown);
            movesLabel.fontStyle = FontStyles.Bold;
            movesLabel.rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 176f, 60f);

            // Goal block
            WhiskerTheme.MakeText(panel.transform, "GoalCap", "GOAL", 28, WhiskerTheme.WarmBrown).rectTransform.SetInsetAndSizeFromParentEdge(RectTransform.Edge.Top, 260f, 40f);
            goal1Label = MakeGoalRow(panel.transform, 300f, WhiskerTheme.SunsetPink, "16");
            goal2Label = MakeGoalRow(panel.transform, 360f, new Color(0.39f, 0.69f, 0.85f), "8");
        }

        private TextMeshProUGUI MakeGoalRow(Transform parent, float topInset, Color iconColor, string num)
        {
            var icon = WhiskerTheme.MakeCircle(parent, "GoalIcon", 44f, iconColor);
            var iconRt = (RectTransform)icon.transform;
            iconRt.anchorMin = new Vector2(0f, 1f);
            iconRt.anchorMax = new Vector2(0f, 1f);
            iconRt.anchoredPosition = new Vector2(40f, -topInset - 22f);
            var label = WhiskerTheme.MakeText(parent, "GoalNum", num, 36, WhiskerTheme.DeepBrown, TextAlignmentOptions.Left);
            label.fontStyle = FontStyles.Bold;
            var lr = (RectTransform)label.transform;
            lr.anchorMin = new Vector2(0f, 1f);
            lr.anchorMax = new Vector2(1f, 1f);
            lr.anchoredPosition = new Vector2(0f, -topInset);
            lr.sizeDelta = new Vector2(-90f, 44f);
            lr.pivot = new Vector2(0.5f, 1f);
            label.margin = new Vector4(90f, 0f, 10f, 0f);
            return label;
        }

        // Right side: 3 boosters + settings button
        private void BuildRightPanel(RectTransform parent)
        {
            float[] ys = { 0.55f, 0.46f, 0.37f, 0.28f };
            Color[] colors = {
                new Color(0.40f, 0.55f, 0.80f),
                new Color(0.95f, 0.74f, 0.30f),
                new Color(0.95f, 0.55f, 0.40f),
                WhiskerTheme.WarmBrown,
            };
            string[] caps = { "3", "2", "3", "" };
            string[] labels = { "B", "B", "B", "⚙" };
            for (int i = 0; i < 4; i++)
            {
                var btn = WhiskerTheme.MakeCircle(parent, $"Right{i}", 130f, colors[i]);
                var rt = (RectTransform)btn.transform;
                rt.anchorMin = new Vector2(1f, ys[i]);
                rt.anchorMax = new Vector2(1f, ys[i]);
                rt.anchoredPosition = new Vector2(-90f, 0f);
                var glyph = WhiskerTheme.MakeText(btn.transform, "Glyph", labels[i], 48, WhiskerTheme.Cream);
                WhiskerTheme.Stretch((RectTransform)glyph.transform);
                if (!string.IsNullOrEmpty(caps[i]))
                {
                    var badge = WhiskerTheme.MakeCircle(btn.transform, "Badge", 44f, new Color(0.95f, 0.30f, 0.30f));
                    var brt = (RectTransform)badge.transform;
                    brt.anchorMin = new Vector2(1f, 0f);
                    brt.anchorMax = new Vector2(1f, 0f);
                    brt.anchoredPosition = new Vector2(8f, 4f);
                    var bn = WhiskerTheme.MakeText(badge.transform, "N", caps[i], 28, WhiskerTheme.Cream);
                    bn.fontStyle = FontStyles.Bold;
                    WhiskerTheme.Stretch((RectTransform)bn.transform);
                }
                WhiskerButton.Attach(btn, () => { /* booster hook placeholder */ });
            }
        }

        // Bottom-left: cat placeholder (white circle per spec)
        private void BuildCatPlaceholder(RectTransform parent)
        {
            var cat = WhiskerTheme.MakeCircle(parent, "CatPlaceholder", 220f, new Color(1f, 1f, 1f, 0.95f));
            var rt = (RectTransform)cat.transform;
            rt.anchorMin = new Vector2(0f, 0.12f);
            rt.anchorMax = new Vector2(0f, 0.12f);
            rt.anchoredPosition = new Vector2(140f, 80f);
        }

        private void BuildBottomNav(RectTransform parent)
        {
            WhiskerBottomNavBar.AttachTo(parent, WhiskerBottomNavBar.Tab.Puzzle);
        }
    }
}
