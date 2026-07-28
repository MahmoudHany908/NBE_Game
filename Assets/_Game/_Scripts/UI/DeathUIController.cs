using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using VContainer;

public class DeathUIController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private TMP_Text waveReachedLabel;
    [SerializeField] private Button retryButton;
    [SerializeField] private Button hubButton;

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
        retryButton.onClick.AddListener(OnRetryClicked);
        hubButton.onClick.AddListener(OnReturnToHubClicked);

        HidePanel();
    }

    private void OnDestroy()
    {
        retryButton.onClick.RemoveListener(OnRetryClicked);
        hubButton.onClick.RemoveListener(OnReturnToHubClicked);

        _eventSubscriber?.Unsubscribe<PlayerDiedEvent>(HandlePlayerDied);
    }

    private void HandlePlayerDied(PlayerDiedEvent evt)
    {
        waveReachedLabel.text = $"FELL AT WAVE {42}";
        ShowPanel();
    }

    private void OnRetryClicked()
    {
        HidePanel();

        Scene scene = SceneManager.GetActiveScene();
        VaultSurvivors.LoadingSystem.SceneLoader.Load(scene.name, new ArenaState());
    }

    private void OnReturnToHubClicked()
    {
        HidePanel();

        VaultSurvivors.LoadingSystem.SceneLoader.Load("HubScene", new HideoutState());
    }

    private void ShowPanel()
    {
        deathPanel.SetActive(true);
        Time.timeScale = 0f;
    }

    private void HidePanel()
    {
        deathPanel.SetActive(false);
        Time.timeScale = 1f;
    }
}