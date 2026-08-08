using UnityEngine;
using UnityEngine.SceneManagement;

namespace VaultSurvivors.LoadingSystem
{
    public class LoadingScreenController : MonoBehaviour
    {
        public static LoadingScreenController Instance { get; private set; }

        [Header("UI References")]
        [SerializeField] private CanvasGroup _canvasGroup;
        [SerializeField] private LoadingBar _loadingBar;
        [SerializeField] private LoadingTextAnimator _textAnimator;
        [SerializeField] private HintController _hintController;
        [SerializeField] private TapToContinueController _tapController;

        [Header("Settings")]
        [SerializeField] private float _fadeDuration = 0.5f;
        [SerializeField] private float _minimumLoadTime = 1.0f;

        private bool _isLoading = false;

        public float MinimumLoadTime => _minimumLoadTime;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;

            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.blocksRaycasts = false;
                _canvasGroup.interactable = false;
            }

            if (_tapController != null)
                _tapController.Disable();
        }

        // ─────────────────────────────────────────────────────────────
        //  PUBLIC ENTRY POINT  (called by SceneLoader)
        //  Running on DontDestroyOnLoad ensures async survives scene transitions
        // ─────────────────────────────────────────────────────────────

        /// <summary>Run the full loading sequence. showHints=false hides the hint panel.</summary>
        public async Awaitable RunLoad(string targetScene, IGameState targetState, bool showHints)
        {
            if (_isLoading)
            {
                Debug.LogWarning("[LoadingScreenController] A load operation is already in progress. Ignoring new request.");
                return;
            }
            _isLoading = true;

            // 1. Show loading screen
            await ShowAsync(showHints);

            // 2. Notify state machine
            var gsm = Object.FindAnyObjectByType<GameStateManager>();
            gsm?.ChangeState(new LoadingState());

            // 3. Begin background load (paused at 90%)
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(targetScene);
            if (asyncLoad == null)
            {
                Debug.LogError($"[LoadingScreenController] Scene '{targetScene}' could not be loaded. Is it in Build Settings?");
                await HideAsync();
                return;
            }

            asyncLoad.allowSceneActivation = false;

            float loadStartTime = Time.unscaledTime;

            // 4. Drive progress bar (0 → 90% maps to 0 → 100% in UI)
            while (asyncLoad.progress < 0.9f)
            {
                float normalizedProgress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
                UpdateProgress(normalizedProgress);
                await Awaitable.NextFrameAsync();
            }

            // Push bar visually to 100%
            UpdateProgress(1f);

            // 5. Wait for bar animation + minimum display time
            while (!IsProgressVisualComplete() ||
                   (Time.unscaledTime - loadStartTime) < _minimumLoadTime)
            {
                await Awaitable.NextFrameAsync();
            }

            // 6. Show "Tap to Continue" and wait for player input
            await WaitForTapAsync();

            // 7. Activate the scene (Unity swaps scenes here)
            asyncLoad.allowSceneActivation = true;
            while (!asyncLoad.isDone)
            {
                await Awaitable.NextFrameAsync();
            }

            // 8. Unload stale assets from the old scene
            var unloadOp = Resources.UnloadUnusedAssets();
            while (!unloadOp.isDone)
            {
                await Awaitable.NextFrameAsync();
            }

            // 9. Fade out and hide loading screen
            await HideAsync();

            // 10. Transition game state (re-find GSM — it may be in new scene)
            if (targetState != null)
            {
                gsm = Object.FindAnyObjectByType<GameStateManager>();
                if (gsm != null)
                    gsm.ChangeState(targetState);
                else
                    Debug.LogWarning("[LoadingScreenController] GameStateManager not found after scene load.");
            }

            _isLoading = false;
        }

        // ─────────────────────────────────────────────────────────────
        //  INTERNAL HELPERS
        // ─────────────────────────────────────────────────────────────

        private async Awaitable ShowAsync(bool showHints)
        {
            if (_loadingBar != null)
                _loadingBar.ResetProgress();

            if (showHints && _hintController != null)
                _hintController.ShowRandomHint();
            else if (_hintController != null)
                _hintController.HideHint();

            if (_textAnimator != null)
                _textAnimator.StartAnimation();

            if (_canvasGroup != null)
                await UIFadeUtility.FadeIn(_canvasGroup, _fadeDuration);
        }

        private async Awaitable WaitForTapAsync()
        {
            if (_tapController == null) return;

            bool tapped = false;
            void OnTapped() { tapped = true; }

            _tapController.OnTapDetected += OnTapped;
            _tapController.Enable();

            while (!tapped)
            {
                await Awaitable.NextFrameAsync();
            }

            _tapController.OnTapDetected -= OnTapped;
        }

        private async Awaitable HideAsync()
        {
            if (_tapController != null)
                _tapController.Disable();

            if (_canvasGroup != null)
                await UIFadeUtility.FadeOut(_canvasGroup, _fadeDuration);

            if (_textAnimator != null)
                _textAnimator.StopAnimation();
        }

        private void UpdateProgress(float progress)
        {
            if (_loadingBar != null)
                _loadingBar.SetTargetProgress(progress);
        }

        private bool IsProgressVisualComplete()
        {
            return _loadingBar == null || _loadingBar.IsComplete;
        }
    }
}
