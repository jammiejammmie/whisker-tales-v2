using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace WhiskerTales.UI
{
    /// <summary>
    /// Scene names for the 4 Whisker screens, plus a runtime bootstrap that injects
    /// the appropriate controller MonoBehaviour into each minimal scene on load.
    ///
    /// Scenes only contain Camera + EventSystem + a Canvas. Controllers build the
    /// full UI in code so the .unity files stay small and reviewable.
    /// </summary>
    public static class WhiskerScreens
    {
        public const string Puzzle    = "WhiskerGameScene";
        public const string Loading   = "WhiskerLoadingScene";
        public const string NabiRoom  = "WhiskerNabiRoomScene";
        public const string SleepMode = "WhiskerSleepModeScene";

        public static void Go(string scene) { SceneManager.LoadScene(scene); }
    }

    /// <summary>
    /// Auto-runs at app launch. Listens for scene loads and, for each Whisker
    /// screen, ensures (a) there is a UICanvas with CanvasScaler set for S26,
    /// (b) an EventSystem exists, and (c) the screen-specific controller is
    /// attached. Mirrors the WhiskerGameBoardHotfix pattern.
    /// </summary>
    public sealed class WhiskerScreenBootstrap : MonoBehaviour
    {
        private static bool bootstrapped;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Boot()
        {
            if (bootstrapped) return;
            bootstrapped = true;
            var go = new GameObject("__WhiskerScreenBootstrap");
            Object.DontDestroyOnLoad(go);
            go.AddComponent<WhiskerScreenBootstrap>();
        }

        private void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            OnSceneLoaded(SceneManager.GetActiveScene(), LoadSceneMode.Single);
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            switch (scene.name)
            {
                // Puzzle HUD overlay disabled — Kit's original GameUICanvas
                // handles the puzzle HUD in the stable v1 build. Re-enable
                // here once the Whisker overlay is ready to ship.
                // case WhiskerScreens.Puzzle:
                //     EnsureController<WhiskerPuzzleHud>("__WhiskerPuzzleHud");
                //     break;
                case WhiskerScreens.Loading:
                    EnsureCanvasAndController<WhiskerLoadingScreen>("LoadingRoot");
                    break;
                case WhiskerScreens.NabiRoom:
                    EnsureCanvasAndController<WhiskerNabiRoomScreen>("NabiRoomRoot");
                    break;
                case WhiskerScreens.SleepMode:
                    EnsureCanvasAndController<WhiskerSleepModeScreen>("SleepModeRoot");
                    break;
            }
        }

        private static T EnsureController<T>(string goName) where T : MonoBehaviour
        {
            var existing = Object.FindObjectOfType<T>();
            if (existing != null) return existing;
            var go = new GameObject(goName);
            return go.AddComponent<T>();
        }

        private static T EnsureCanvasAndController<T>(string goName) where T : MonoBehaviour
        {
            EnsureEventSystem();
            return EnsureController<T>(goName);
        }

        public static void EnsureEventSystem()
        {
            if (Object.FindObjectOfType<EventSystem>() != null) return;
            var go = new GameObject("EventSystem");
            go.AddComponent<EventSystem>();
            go.AddComponent<StandaloneInputModule>();
        }

        public static Canvas CreateScreenCanvas(string name, Color? clearColor = null)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            var canvas = go.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            canvas.sortingOrder = 0;
            var scaler = go.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 2340f);
            scaler.screenMatchMode = CanvasScaler.ScreenMatchMode.MatchWidthOrHeight;
            scaler.matchWidthOrHeight = 0.5f;
            scaler.referencePixelsPerUnit = 100f;

            if (clearColor.HasValue) EnsureSolidCameraClear(clearColor.Value);
            return canvas;
        }

        private static void EnsureSolidCameraClear(Color color)
        {
            var cam = Camera.main;
            if (cam == null) return;
            cam.clearFlags = CameraClearFlags.SolidColor;
            cam.backgroundColor = color;
        }
    }
}
