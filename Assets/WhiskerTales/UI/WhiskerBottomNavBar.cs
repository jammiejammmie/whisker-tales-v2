using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace WhiskerTales.UI
{
    /// <summary>
    /// Canonical 4-tab bottom navigation (Puzzle / Nabi / Shop / Sleep).
    /// Visual contract matches the Sleep/NabiRoom mockups: dark matte warm-tone
    /// rounded panel, golden glow on the active tab, inactive tabs at 65% opacity.
    /// Reused across every Whisker screen via BottomNavBar.prefab so the bar
    /// never drifts between screens.
    /// </summary>
    public sealed class WhiskerBottomNavBar : MonoBehaviour
    {
        public enum Tab { Puzzle = 0, Nabi = 1, Shop = 2, Sleep = 3 }

        [SerializeField] private Tab activeTab = Tab.Puzzle;

        // Mockup-derived constants (Sleep/NabiRoom shots).
        private const float BarHeightFraction = 0.085f;
        private const float BarSideMargin     = 36f;
        private const float BarBottomMargin   = 32f;
        private const float IconDiameter      = 92f;
        private const float IconYOffset       = 22f;
        private const float LabelYOffset      = -38f;
        private const float InactiveAlpha     = 0.65f;
        private const float GlowDiameter      = 132f;

        private static readonly Color BarColor    = new Color(0.18f, 0.13f, 0.10f, 0.92f);
        private static readonly Color GlowColor   = new Color(0.98f, 0.84f, 0.52f, 0.45f);
        private static readonly Color IconActive  = new Color(0.98f, 0.92f, 0.78f, 1f);
        private static readonly Color LabelActive = new Color(0.98f, 0.92f, 0.78f, 1f);
        private static readonly Color IconIdle    = new Color(0.86f, 0.78f, 0.66f, 1f);
        private static readonly Color LabelIdle   = new Color(0.82f, 0.74f, 0.62f, 1f);

        private static readonly string[] Glyphs = { "▰", "🐾", "🛍", "☾" };
        private static readonly string[] Labels = { "Puzzle", "Nabi", "Shop", "Sleep" };

        private bool built;

        private void Awake()
        {
            EnsureBuilt();
        }

        public void SetActiveTab(Tab tab)
        {
            activeTab = tab;
            if (!built) return;
            ApplyActiveState();
        }

        /// <summary>
        /// Spawns the canonical bottom nav into the given canvas. Loads the
        /// prefab from Resources when available; otherwise builds the same
        /// hierarchy procedurally so screens never miss the bar in the editor
        /// build pipeline. Returns the spawned bar component.
        /// </summary>
        public static WhiskerBottomNavBar AttachTo(RectTransform canvasRoot, Tab active)
        {
            var prefab = Resources.Load<GameObject>("BottomNavBar");
            GameObject go;
            if (prefab != null)
            {
                go = Instantiate(prefab, canvasRoot, false);
                go.name = "BottomNavBar";
            }
            else
            {
                go = new GameObject("BottomNavBar", typeof(RectTransform));
                go.transform.SetParent(canvasRoot, false);
                go.AddComponent<WhiskerBottomNavBar>();
            }
            var bar = go.GetComponent<WhiskerBottomNavBar>();
            bar.SetActiveTab(active);
            return bar;
        }

        private void EnsureBuilt()
        {
            if (built) return;
            built = true;

            var rt = (RectTransform)transform;
            rt.anchorMin = new Vector2(0f, 0f);
            rt.anchorMax = new Vector2(1f, BarHeightFraction);
            rt.offsetMin = new Vector2(BarSideMargin, BarBottomMargin);
            rt.offsetMax = new Vector2(-BarSideMargin, BarBottomMargin);

            var panel = GetComponent<Image>();
            if (panel == null) panel = gameObject.AddComponent<Image>();
            panel.color = BarColor;
            panel.sprite = WhiskerTheme.RoundedSprite;
            panel.type = Image.Type.Sliced;
            panel.raycastTarget = true;

            for (int i = 0; i < 4; i++)
            {
                BuildSlot((Tab)i, i);
            }
            ApplyActiveState();
        }

        private void BuildSlot(Tab tab, int index)
        {
            float t = (index + 0.5f) / 4f;
            var slot = WhiskerTheme.MakePanel(transform, $"Tab_{tab}", new Color(0, 0, 0, 0));
            var sr = (RectTransform)slot.transform;
            WhiskerTheme.Anchor(sr,
                new Vector2(t - 0.125f, 0f), new Vector2(t + 0.125f, 1f),
                Vector2.zero, Vector2.zero);

            // Glow (only visible on active tab).
            var glow = WhiskerTheme.MakeCircle(slot.transform, "Glow", GlowDiameter, GlowColor);
            var glowRt = (RectTransform)glow.transform;
            glowRt.anchorMin = new Vector2(0.5f, 0.5f);
            glowRt.anchorMax = new Vector2(0.5f, 0.5f);
            glowRt.anchoredPosition = new Vector2(0f, IconYOffset);
            glow.GetComponent<Image>().raycastTarget = false;

            // Icon glyph (placeholder until per-tab sprites are wired).
            var icon = WhiskerTheme.MakeText(slot.transform, "Icon", Glyphs[index], 56, IconIdle);
            var iconRt = (RectTransform)icon.transform;
            iconRt.anchorMin = new Vector2(0f, 0.5f);
            iconRt.anchorMax = new Vector2(1f, 0.5f);
            iconRt.offsetMin = new Vector2(0f, IconYOffset - 38f);
            iconRt.offsetMax = new Vector2(0f, IconYOffset + 38f);

            // Label.
            var lbl = WhiskerTheme.MakeText(slot.transform, "Label", Labels[index], 28, LabelIdle);
            var lblRt = (RectTransform)lbl.transform;
            lblRt.anchorMin = new Vector2(0f, 0f);
            lblRt.anchorMax = new Vector2(1f, 0.4f);
            lblRt.offsetMin = new Vector2(0f, 0f);
            lblRt.offsetMax = new Vector2(0f, 0f);

            int captured = index;
            WhiskerButton.Attach(slot, () => OnTabClicked((Tab)captured));
        }

        private void OnTabClicked(Tab tab)
        {
            if (tab == activeTab) return;
            string scene = SceneForTab(tab);
            if (string.IsNullOrEmpty(scene))
            {
                // Shop has no scene yet. Visually mark it active so the user gets
                // tactile feedback, but stay on the current screen.
                SetActiveTab(tab);
                return;
            }
            WhiskerScreens.Go(scene);
        }

        private static string SceneForTab(Tab tab)
        {
            switch (tab)
            {
                case Tab.Puzzle: return WhiskerScreens.Puzzle;
                case Tab.Nabi:   return WhiskerScreens.NabiRoom;
                case Tab.Sleep:  return WhiskerScreens.SleepMode;
                default:         return null;
            }
        }

        private void ApplyActiveState()
        {
            for (int i = 0; i < 4; i++)
            {
                var slot = transform.Find($"Tab_{(Tab)i}");
                if (slot == null) continue;
                bool isActive = (Tab)i == activeTab;

                var glow = slot.Find("Glow");
                if (glow != null)
                {
                    var img = glow.GetComponent<Image>();
                    var c = GlowColor;
                    c.a = isActive ? 0.55f : 0f;
                    img.color = c;
                }

                var iconText = slot.Find("Icon")?.GetComponent<TextMeshProUGUI>();
                if (iconText != null)
                {
                    var c = isActive ? IconActive : IconIdle;
                    if (!isActive) c.a *= InactiveAlpha;
                    iconText.color = c;
                }

                var labelText = slot.Find("Label")?.GetComponent<TextMeshProUGUI>();
                if (labelText != null)
                {
                    var c = isActive ? LabelActive : LabelIdle;
                    if (!isActive) c.a *= InactiveAlpha;
                    labelText.color = c;
                    labelText.fontStyle = isActive ? FontStyles.Bold : FontStyles.Normal;
                }
            }
        }
    }
}
