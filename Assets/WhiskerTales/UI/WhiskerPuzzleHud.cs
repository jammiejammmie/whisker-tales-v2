using System.Collections;
using GameVanilla.Game.Common;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WhiskerTales.UI
{
    /// <summary>
    /// Overlay HUD for WhiskerGameScene, matching the WT_Puzzle_MainBoard_v1
    /// mockup:
    ///   - Top center : Whisker Tales script logo + 위스커 테일즈 sub
    ///   - Top left   : 스테이지 / number card
    ///   - Top right  : 이동횟수 / number card + settings gear
    ///   - Top middle : 목표 rope-hung sign with paw + count
    ///   - Right edge : compact booster column (3 boosters + settings)
    ///   - Center     : board (Kit's board, no overlay)
    ///   - Bottom     : nabi companion placeholder + BottomNavBar (Puzzle active)
    ///
    /// Also hides Kit's leftover HUD: the pink Moves/Goal bar and the Girl
    /// portrait. Hiding is non-destructive — GameUi keeps its component refs
    /// alive so GameBoard's gameUi.SetLimit/SetGoals/... calls stay no-op safe.
    /// </summary>
    public sealed class WhiskerPuzzleHud : MonoBehaviour
    {
        private TextMeshProUGUI stageLabel;
        private TextMeshProUGUI movesLabel;
        private TextMeshProUGUI goalLabel;
        private GameBoard board;
        private int lastStage = -1;
        private int lastMoves = -1;

        private void Start()
        {
            HideKitHud();
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

        private static void HideKitHud()
        {
            // GameUi + GameUICanvas are now disabled at the scene level
            // (WhiskerGameScene.unity m_IsActive: 0). Keep this method as a
            // belt-and-suspenders runtime guard: anyone enabling the Kit HUD
            // in the editor will still get it hidden when entering Play.
            var kitUi = FindObjectOfType<GameUi>(true);
            if (kitUi != null && kitUi.gameObject.activeSelf)
            {
                kitUi.gameObject.SetActive(false);
            }
        }

        private void BuildHud()
        {
            var canvas = WhiskerScreenBootstrap.CreateScreenCanvas("WhiskerHudCanvas");
            canvas.sortingOrder = 100;
            var root = (RectTransform)canvas.transform;

            BuildBackgroundTint(root);
            BuildTopRow(root);
            BuildGoalSign(root);
            BuildRightColumn(root);
            BuildNabiCompanion(root);
            BuildBottomNav(root);
        }

        // Subtle warm overlay so the Kit's neutral background reads as a
        // moody hanok interior. Sits behind everything else.
        private void BuildBackgroundTint(RectTransform parent)
        {
            var tint = WhiskerTheme.MakePanel(parent, "HanokTint",
                new Color(0.16f, 0.10f, 0.07f, 0.30f));
            WhiskerTheme.Stretch((RectTransform)tint.transform);
            tint.GetComponent<Image>().raycastTarget = false;
            tint.transform.SetAsFirstSibling();
        }

        // Top: Logo (center), 스테이지 card (left), 이동횟수 card + gear (right).
        private void BuildTopRow(RectTransform parent)
        {
            // Center logo block.
            var logo = WhiskerTheme.MakeText(parent, "Logo", "Whisker Tales",
                72, WhiskerTheme.Cream);
            logo.fontStyle = FontStyles.Bold;
            WhiskerTheme.Anchor((RectTransform)logo.transform,
                new Vector2(0.25f, 0.90f), new Vector2(0.75f, 0.97f),
                Vector2.zero, Vector2.zero);

            var sub = WhiskerTheme.MakeText(parent, "LogoSub", "✦ 위스커 테일즈 ✦",
                26, new Color(0.92f, 0.85f, 0.70f, 0.95f));
            WhiskerTheme.Anchor((RectTransform)sub.transform,
                new Vector2(0.25f, 0.86f), new Vector2(0.75f, 0.90f),
                Vector2.zero, Vector2.zero);

            // Left stage card.
            stageLabel = BuildCornerCard(parent, "StageCard",
                "스테이지", "127",
                new Vector2(0f, 0.86f), new Vector2(0f, 0.94f),
                new Vector2(28f, 0f), new Vector2(260f, 0f));

            // Right moves card.
            movesLabel = BuildCornerCard(parent, "MovesCard",
                "이동 횟수", "24",
                new Vector2(1f, 0.86f), new Vector2(1f, 0.94f),
                new Vector2(-260f, 0f), new Vector2(-90f, 0f));

            // Settings gear (right of the moves card).
            var gear = WhiskerTheme.MakeCircle(parent, "Settings",
                72f, new Color(0.30f, 0.21f, 0.16f, 0.90f));
            var gearRt = (RectTransform)gear.transform;
            gearRt.anchorMin = new Vector2(1f, 0.88f);
            gearRt.anchorMax = new Vector2(1f, 0.88f);
            gearRt.anchoredPosition = new Vector2(-50f, 0f);
            var gearGlyph = WhiskerTheme.MakeText(gear.transform, "G", "⚙",
                40, WhiskerTheme.Cream);
            WhiskerTheme.Stretch((RectTransform)gearGlyph.transform);
            WhiskerButton.Attach(gear, () => { /* settings popup hook */ });
        }

        private TextMeshProUGUI BuildCornerCard(RectTransform parent, string name,
            string caption, string number,
            Vector2 anchorMin, Vector2 anchorMax,
            Vector2 offsetMin, Vector2 offsetMax)
        {
            var card = WhiskerTheme.MakePanel(parent, name, WhiskerTheme.PanelTint);
            var img = card.GetComponent<Image>();
            img.sprite = WhiskerTheme.RoundedSprite;
            img.type = Image.Type.Sliced;
            WhiskerTheme.Anchor((RectTransform)card.transform,
                anchorMin, anchorMax, offsetMin, offsetMax);

            var cap = WhiskerTheme.MakeText(card.transform, "Cap", caption,
                26, WhiskerTheme.WarmBrown);
            WhiskerTheme.Anchor((RectTransform)cap.transform,
                new Vector2(0f, 0.55f), new Vector2(1f, 0.95f),
                Vector2.zero, Vector2.zero);

            var num = WhiskerTheme.MakeText(card.transform, "Num", number,
                56, WhiskerTheme.DeepBrown);
            num.fontStyle = FontStyles.Bold;
            WhiskerTheme.Anchor((RectTransform)num.transform,
                new Vector2(0f, 0.05f), new Vector2(1f, 0.60f),
                Vector2.zero, Vector2.zero);
            return num;
        }

        // Rope-hung "목표" sign in the top middle (between the corner cards).
        private void BuildGoalSign(RectTransform parent)
        {
            var sign = WhiskerTheme.MakePanel(parent, "GoalSign",
                WhiskerTheme.BoardPaper);
            var img = sign.GetComponent<Image>();
            img.sprite = WhiskerTheme.RoundedSprite;
            img.type = Image.Type.Sliced;
            WhiskerTheme.Anchor((RectTransform)sign.transform,
                new Vector2(0.28f, 0.78f), new Vector2(0.72f, 0.85f),
                Vector2.zero, Vector2.zero);

            // Two rope strands above the sign.
            for (int i = 0; i < 2; i++)
            {
                var rope = WhiskerTheme.MakePanel(sign.transform, $"Rope{i}",
                    new Color(0.56f, 0.42f, 0.30f, 1f));
                var rt = (RectTransform)rope.transform;
                rt.anchorMin = new Vector2(0.20f + i * 0.60f, 1f);
                rt.anchorMax = new Vector2(0.20f + i * 0.60f, 1f);
                rt.sizeDelta = new Vector2(6f, 36f);
                rt.anchoredPosition = new Vector2(0f, 18f);
                rope.GetComponent<Image>().raycastTarget = false;
            }

            var capLabel = WhiskerTheme.MakeText(sign.transform, "Cap", "목표",
                30, WhiskerTheme.WarmBrown);
            capLabel.fontStyle = FontStyles.Bold;
            WhiskerTheme.Anchor((RectTransform)capLabel.transform,
                new Vector2(0f, 0.55f), new Vector2(1f, 0.95f),
                Vector2.zero, Vector2.zero);

            var paw = WhiskerTheme.MakeText(sign.transform, "Paw", "🐾",
                42, WhiskerTheme.WarmBrown);
            WhiskerTheme.Anchor((RectTransform)paw.transform,
                new Vector2(0.30f, 0.05f), new Vector2(0.50f, 0.55f),
                Vector2.zero, Vector2.zero);

            goalLabel = WhiskerTheme.MakeText(sign.transform, "Count", "28",
                40, WhiskerTheme.DeepBrown, TextAlignmentOptions.Left);
            goalLabel.fontStyle = FontStyles.Bold;
            WhiskerTheme.Anchor((RectTransform)goalLabel.transform,
                new Vector2(0.50f, 0.05f), new Vector2(0.80f, 0.55f),
                Vector2.zero, Vector2.zero);
        }

        // Right edge: 3 compact booster buttons. Settings already lives in the
        // top row, so this column is purely boosters.
        private void BuildRightColumn(RectTransform parent)
        {
            Color[] colors = {
                new Color(0.40f, 0.55f, 0.80f),
                new Color(0.95f, 0.74f, 0.30f),
                new Color(0.95f, 0.55f, 0.40f),
            };
            string[] caps = { "3", "2", "3" };
            float[] ys   = { 0.62f, 0.54f, 0.46f };

            for (int i = 0; i < 3; i++)
            {
                var btn = WhiskerTheme.MakeCircle(parent, $"Booster{i}", 96f, colors[i]);
                var rt = (RectTransform)btn.transform;
                rt.anchorMin = new Vector2(1f, ys[i]);
                rt.anchorMax = new Vector2(1f, ys[i]);
                rt.anchoredPosition = new Vector2(-72f, 0f);

                var glyph = WhiskerTheme.MakeText(btn.transform, "G", "✦", 40, WhiskerTheme.Cream);
                WhiskerTheme.Stretch((RectTransform)glyph.transform);

                var badge = WhiskerTheme.MakeCircle(btn.transform, "Badge", 38f,
                    new Color(0.92f, 0.30f, 0.30f));
                var brt = (RectTransform)badge.transform;
                brt.anchorMin = new Vector2(1f, 0f);
                brt.anchorMax = new Vector2(1f, 0f);
                brt.anchoredPosition = new Vector2(8f, 4f);
                var bn = WhiskerTheme.MakeText(badge.transform, "N", caps[i], 24, WhiskerTheme.Cream);
                bn.fontStyle = FontStyles.Bold;
                WhiskerTheme.Stretch((RectTransform)bn.transform);

                WhiskerButton.Attach(btn, () => { /* booster hook */ });
            }
        }

        // Center-bottom: nabi companion placeholder (white circle until art ships).
        // Sits just above the BottomNavBar in the strip below the board.
        private void BuildNabiCompanion(RectTransform parent)
        {
            var cat = WhiskerTheme.MakeCircle(parent, "NabiCompanion",
                200f, new Color(1f, 1f, 1f, 0.92f));
            var rt = (RectTransform)cat.transform;
            rt.anchorMin = new Vector2(0.5f, 0.14f);
            rt.anchorMax = new Vector2(0.5f, 0.14f);
            rt.anchoredPosition = new Vector2(0f, 0f);
        }

        private void BuildBottomNav(RectTransform parent)
        {
            WhiskerBottomNavBar.AttachTo(parent, WhiskerBottomNavBar.Tab.Puzzle);
        }
    }
}
