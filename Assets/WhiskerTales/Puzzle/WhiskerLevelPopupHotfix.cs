using GameVanilla.Game.Popups;
using GameVanilla.Game.Scenes;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace WhiskerTales.Puzzle
{
    /// <summary>
    /// Suppresses the Kit's level-start popup (`LevelGoalsPopup`, opened from
    /// `GameScene.Start` via `Resources.LoadAsync("Popups/LevelGoalsPopup")`).
    /// The popup auto-kills itself after 2.4 s and then calls
    /// `gameScene.StartGame()`. We want zero on-screen popup at the start of
    /// a stage, but the StartGame() call still needs to fire — otherwise the
    /// suggested-match coroutine and (for time-mode) the countdown never start.
    ///
    /// Approach (reflective, Vendor-untouched):
    ///   1. Detect the popup the frame after it spawns.
    ///   2. Stop its AutoKill coroutine and call `Close()` + `StartGame()`
    ///      immediately, so the player drops straight into gameplay.
    ///   3. As a belt-and-suspenders measure, also slap a CanvasGroup with
    ///      alpha=0 on the popup root so even the single-frame flash is
    ///      invisible if Close() takes a frame to tear the GameObject down.
    ///
    /// Same RuntimeInitializeOnLoadMethod + sceneLoaded pattern as
    /// `WhiskerGameBoardHotfix`. State resets per scene load so a second
    /// stage entered without an app restart still gets the suppression.
    /// </summary>
    public sealed class WhiskerLevelPopupHotfix : MonoBehaviour
    {
        private static bool bootstrapped;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Bootstrap()
        {
            if (bootstrapped) return;
            bootstrapped = true;

            var go = new GameObject("__WhiskerLevelPopupHotfix");
            Object.DontDestroyOnLoad(go);
            go.AddComponent<WhiskerLevelPopupHotfix>();
        }

        private bool armed;
        private bool dismissed;

        private void Start()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            ArmIfGameScene(SceneManager.GetActiveScene());
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            ArmIfGameScene(scene);
        }

        private void ArmIfGameScene(Scene scene)
        {
            if (!scene.IsValid()) return;
            if (scene.name != "WhiskerGameScene")
            {
                armed = false;
                dismissed = false;
                return;
            }
            armed = true;
            dismissed = false;
        }

        private void Update()
        {
            if (!armed || dismissed) return;

            var popup = FindObjectOfType<LevelGoalsPopup>(true);
            if (popup == null) return;

            // Make the popup invisible immediately in case Close() needs a frame.
            var cg = popup.GetComponent<CanvasGroup>();
            if (cg == null) cg = popup.gameObject.AddComponent<CanvasGroup>();
            cg.alpha = 0f;
            cg.blocksRaycasts = false;
            cg.interactable = false;

            // Stop the AutoKill WaitForSeconds(2.4f) and dismiss now, then
            // start the stage so the game doesn't hang on an invisible popup.
            popup.StopAllCoroutines();
            var parent = popup.parentScene as GameScene;
            popup.Close();
            if (parent != null) parent.StartGame();

            dismissed = true;
        }
    }
}
