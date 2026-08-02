using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VContainer;


public class ExtractionUIController : MonoBehaviour
{
    [SerializeField] private HealthBar healthBar;

    // UGUI References. 
    // Best Practice: Assign these directly in the Unity Inspector.
    // Fallback: Awake() will attempt to auto-find them by name to mimic UI Toolkit's Q<T>() behavior.
    [SerializeField] private PanelUI panel;
    [SerializeField] private TextMeshProUGUI waveLabel;
    [SerializeField] private GameObject resourcesImage;
    [SerializeField] private Button extractButton;
    [SerializeField] private Button continueButton;

    private IEventPublisher _eventPublisher;
    private IEventSubscriber _eventSubscriber;

    [Inject]
    public void Construct(IEventPublisher eventPublisher, IEventSubscriber eventSubscriber)
    {
        _eventPublisher = eventPublisher;
        _eventSubscriber = eventSubscriber;

        if (isActiveAndEnabled)
        {
            _eventSubscriber?.Subscribe<ExtractionAvailableEvent>(HandleExtractionAvailable);
            _eventSubscriber?.Subscribe<PlayerTakeDamageEvent>(HandleHealthChangedTakeDamage);
            _eventSubscriber?.Subscribe<PlayerHealedEvent>(HandleHealthChangedTakeHeal);
        }
        panel.gameObject.SetActive(true);
        waveLabel.gameObject.SetActive(true);
        resourcesImage.SetActive(true);
        extractButton.gameObject.SetActive(true);
        continueButton.gameObject.SetActive(true);
    }

    private void Awake()
    {
        if (extractButton != null) extractButton.onClick.AddListener(OnExtractClicked);
        if (continueButton != null) continueButton.onClick.AddListener(OnContinueClicked);

        HidePanel();
    }

    private void OnEnable()
    {
        _eventSubscriber?.Subscribe<ExtractionAvailableEvent>(HandleExtractionAvailable);
        _eventSubscriber?.Subscribe<PlayerTakeDamageEvent>(HandleHealthChangedTakeDamage);
        _eventSubscriber?.Subscribe<PlayerHealedEvent>(HandleHealthChangedTakeHeal);

        if (healthBar != null) healthBar.SetHealth(1f);
    }

    private void OnDisable()
    {
        _eventSubscriber?.Unsubscribe<ExtractionAvailableEvent>(HandleExtractionAvailable);
        _eventSubscriber?.Unsubscribe<PlayerTakeDamageEvent>(HandleHealthChangedTakeDamage);
        _eventSubscriber?.Unsubscribe<PlayerHealedEvent>(HandleHealthChangedTakeHeal);
    }

    private void OnDestroy()
    {
        if (extractButton != null) extractButton.onClick.RemoveListener(OnExtractClicked);
        if (continueButton != null) continueButton.onClick.RemoveListener(OnContinueClicked);
    }

    #region Health Bar Handling
    private void HandleHealthChangedTakeDamage(PlayerTakeDamageEvent evt)
    {
        if (healthBar != null)
        {
            healthBar.SetHealth(evt.healthPercentage);
            Debug.Log($"Player took damage. New health percentage: {evt.healthPercentage}");
        }
    }

    private void HandleHealthChangedTakeHeal(PlayerHealedEvent evt)
    {
        Debug.Log($"Player HEALED. New health percentage: {evt.healthPercentage}");
        if (healthBar != null)
        {
            healthBar.SetHealth(evt.healthPercentage);
        }
    }
    #endregion

    private void HandleExtractionAvailable(ExtractionAvailableEvent evt)
    {
        if (waveLabel != null) waveLabel.text = $"WAVE {evt.WaveNumber} CLEARED";
        

        ShowPanel();
    }

    private void OnExtractClicked()
    {
        HidePanel();

        _eventPublisher?.Publish(new ExtractionDecisionEvent(didExtract: true));
        VaultSurvivors.LoadingSystem.SceneLoader.Load("HubScene", new HideoutState());
    }

    private void OnContinueClicked()
    {
        HidePanel();
        _eventPublisher?.Publish(new ExtractionDecisionEvent(didExtract: false));
    }

    private void ShowPanel()
    {
        if (panel != null) panel.Show();
        Time.timeScale = 0f;
    }

    private void HidePanel()
    {
        if (panel != null) panel.Hide();
        Time.timeScale = 1f;
    }
}