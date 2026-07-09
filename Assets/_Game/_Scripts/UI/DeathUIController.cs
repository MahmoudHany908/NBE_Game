using UnityEngine;
using UnityEngine.UIElements;


[RequireComponent(typeof(UIDocument))]
public class DeathUIController : MonoBehaviour
{

    private UIDocument document;
    private VisualElement root;
    private VisualElement panel;
    private Label waveReachedLabel;
    private Button retryButton;
    private Button hubButton;

    private void Awake()
    {
        document = GetComponent<UIDocument>();
        root = document.rootVisualElement;

        panel = root.Q<VisualElement>("death-panel");
        waveReachedLabel = root.Q<Label>("wave-reached-label");
        retryButton = root.Q<Button>("retry-button");
        hubButton = root.Q<Button>("hub-button");

        retryButton.clicked += OnRetryClicked;
        hubButton.clicked += OnReturnToHubClicked;

        HidePanel();
    }

    private void OnEnable()
    {
        EventBus.Subscribe<PlayerDiedEvent>(HandlePlayerDied);
    }

    private void OnDisable()
    {
        EventBus.Unsubscribe<PlayerDiedEvent>(HandlePlayerDied);
    }

    private void OnDestroy()
    {
        retryButton.clicked -= OnRetryClicked;
        hubButton.clicked -= OnReturnToHubClicked;
    }

    private void HandlePlayerDied(PlayerDiedEvent evt)
    {
        waveReachedLabel.text = $"FELL AT WAVE {evt.WaveReached}";
        ShowPanel();
    }

    private void OnRetryClicked()
    {
        HidePanel();
        // TODO: hook up to your arena scope reset / scene reload
        // e.g. arenaLifetimeScope.RestartRun();
    }

    private void OnReturnToHubClicked()
    {
        HidePanel();
        // TODO: hook up to your hub-transition service
        // e.g. hubTransitionService.ReturnToHub();
    }

    private void ShowPanel()
    {
        panel.RemoveFromClassList("hidden");
        Time.timeScale = 0f;
    }

    private void HidePanel()
    {
        if (!panel.ClassListContains("hidden"))
            panel.AddToClassList("hidden");
        Time.timeScale = 1f;
    }
}