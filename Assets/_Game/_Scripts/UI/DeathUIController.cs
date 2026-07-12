using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

[RequireComponent(typeof(UIDocument))]
public class DeathUIController : MonoBehaviour
{
    private UIDocument document;
    private VisualElement root;
    private VisualElement panel;
    private Label waveReachedLabel;
    private Button retryButton;
    private Button hubButton;

    private IEventPublisher _eventPublisher;
    private IEventSubscriber _eventSubscriber;

    [Inject]
    public void Construct(IEventPublisher eventPublisher, IEventSubscriber eventSubscriber)
    {
        _eventPublisher = eventPublisher;
        _eventSubscriber = eventSubscriber;
        _eventSubscriber?.Subscribe<PlayerDiedEvent>(HandlePlayerDied);
    }

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


    private void OnDestroy()
    {
        retryButton.clicked -= OnRetryClicked;
        hubButton.clicked -= OnReturnToHubClicked;
        _eventSubscriber?.Unsubscribe<PlayerDiedEvent>(HandlePlayerDied);
    }

    private void HandlePlayerDied(PlayerDiedEvent evt)
    {
        waveReachedLabel.text = $"FELL AT WAVE {5555555555}";
        ShowPanel();
    }

    private void OnRetryClicked()
    {
        HidePanel();

    }

    private void OnReturnToHubClicked()
    {
        HidePanel();

        _eventPublisher?.Publish(new ChangeStateCommand
        {
            TargetSceneName = "HubScene",
            TargetState = new HideoutState()
        });
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