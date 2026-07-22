using UnityEngine;

namespace VaultSurvivors.LoadingSystem
{
    /// <summary>
    /// Controls the Title Screen sequence:
    /// 1. Plays a fake loading bar animation.
    /// 2. Shows "Tap to Continue".
    /// 3. On tap: uses SceneLoader (no hints) to load the Hub with the full Loading UI.
    /// </summary>
    public class TitleScreenController : MonoBehaviour
    {
        [Header("UI Components")]
        [SerializeField] private CanvasGroup _loadingUIGroup;
        [SerializeField] private LoadingBar _loadingBar;
        [SerializeField] private TapToContinueController _tapController;

        [Header("Settings")]
        [SerializeField] private float _fakeLoadingDuration = 2.0f;
        [SerializeField] private float _uiFadeDuration = 0.5f;

        private void Start()
        {
            StartTitleSequence();
        }

        private async void StartTitleSequence()
        {
            // 1. Reset state
            if (_tapController != null) _tapController.Disable();
            if (_loadingBar != null) _loadingBar.ResetProgress();

            if (_loadingUIGroup != null)
            {
                _loadingUIGroup.alpha = 1f;
                _loadingUIGroup.gameObject.SetActive(true);
            }

            // 2. Play fake progress bar
            float timer = 0f;
            while (timer < _fakeLoadingDuration)
            {
                timer += Time.unscaledDeltaTime;
                float progress = Mathf.Clamp01(timer / _fakeLoadingDuration);

                if (_loadingBar != null)
                    _loadingBar.SetTargetProgress(progress);

                await Awaitable.NextFrameAsync();
            }

            // 3. Wait for bar animation to visually reach 100%
            if (_loadingBar != null) _loadingBar.SetTargetProgress(1f);
            while (_loadingBar != null && !_loadingBar.IsComplete)
            {
                await Awaitable.NextFrameAsync();
            }

            // 4. Show "Tap to Continue"
            if (_tapController != null)
            {
                _tapController.OnTapDetected += HandleTapDetected;
                _tapController.Enable();
            }
            else
            {
                // No tap controller assigned — skip straight to Hub
                HandleTapDetected();
            }
        }

        private async void HandleTapDetected()
        {
            if (_tapController != null)
                _tapController.OnTapDetected -= HandleTapDetected;

            if (_tapController != null)
                _tapController.Disable();

            // Fade out the fake loading bar UI before loading the next scene
            if (_loadingUIGroup != null)
            {
                await UIFadeUtility.FadeOut(_loadingUIGroup, _uiFadeDuration);
                if (_loadingUIGroup != null)
                    _loadingUIGroup.gameObject.SetActive(false);
            }

            Debug.Log("[TitleScreenController] Tap detected — loading Hub with loading screen (no hints).");

            // Use LoadNoHints so the Loading Screen shows but the hint panel stays empty
            SceneLoader.LoadNoHints("HubScene", new HideoutState());
        }
    }
}
