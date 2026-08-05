using System.Collections;
using TMPro;
using UnityEngine;
using VContainer;

public class WaveMilestoneController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private WaveSpawner waveSpawner;
    [SerializeField] private TextMeshProUGUI countdownText;

    [Header("Milestone Config")]
    [SerializeField] private int milestoneInterval = 5;

    [Header("Timing")]
    [SerializeField] private float initialDelay = 5f;
    [SerializeField] private float preparationTime = 10f;

    public int CurrentWave { get; private set; }

    private bool waitingOnDecision = false;
    private bool squadsFinishedSpawning = false;

    private IEventPublisher _eventPublisher;
    private IEventSubscriber _eventSubscriber;

    [Inject]
    public void Construct(IEventPublisher eventPublisher, IEventSubscriber eventSubscriber)
    {
        _eventPublisher = eventPublisher;
        _eventSubscriber = eventSubscriber;

        if (isActiveAndEnabled)
        {
            _eventSubscriber?.Subscribe<ExtractionDecisionEvent>(HandleExtractionDecision);
            _eventSubscriber?.Subscribe<PlayerDiedEvent>(HandlePlayerDied);
        }
    }

    private void Awake()
    {
        if (waveSpawner == null)
            Debug.LogError($"[{nameof(WaveMilestoneController)}] Missing required WaveSpawner reference on {gameObject.name}.", this);
    }

    private void OnEnable()
    {
        if (waveSpawner != null)
            waveSpawner.OnAllSquadsSpawned += HandleAllSquadsSpawned;

        _eventSubscriber?.Subscribe<ExtractionDecisionEvent>(HandleExtractionDecision);
        _eventSubscriber?.Subscribe<PlayerDiedEvent>(HandlePlayerDied);
    }

    private void OnDisable()
    {
        if (waveSpawner != null)
            waveSpawner.OnAllSquadsSpawned -= HandleAllSquadsSpawned;

        _eventSubscriber?.Unsubscribe<ExtractionDecisionEvent>(HandleExtractionDecision);
        _eventSubscriber?.Unsubscribe<PlayerDiedEvent>(HandlePlayerDied);
        StopAllCoroutines();
    }

    /// <summary>Call this once, e.g. when the arena scope is entered, to kick off wave 1.</summary>
    public void BeginRun()
    {
        CurrentWave = 0;
        StartCoroutine(InitialDelayThenAdvance());
    }

    private IEnumerator InitialDelayThenAdvance()
    {
        if (initialDelay > 0f)
            yield return new WaitForSeconds(initialDelay);

        AdvanceToNextWave();
    }

    private void AdvanceToNextWave()
    {
        CurrentWave++;
        squadsFinishedSpawning = false;
        StartCoroutine(PreparationThenSpawn(CurrentWave));
    }

    private IEnumerator PreparationThenSpawn(int wave)
    {
        float timeLeft = preparationTime;

        while (timeLeft > 0f)
        {
            if (countdownText != null)
                countdownText.text = "Next Wave In: " + Mathf.CeilToInt(timeLeft);

            yield return new WaitForSeconds(1f);
            timeLeft -= 1f;
        }

        if (countdownText != null)
        {
            countdownText.text = "Wave Starting!";
            yield return new WaitForSeconds(1f);
            countdownText.text = "";
        }

        waveSpawner.StartSpawningWave(wave);
    }

    private void HandleAllSquadsSpawned()
    {
        if (squadsFinishedSpawning) return; // guard against double-invoke
        squadsFinishedSpawning = true;
        StartCoroutine(WaitForWaveClear());
    }

    private IEnumerator WaitForWaveClear()
    {
        while (!waveSpawner.AreAllEnemiesDead())
            yield return null;

        _eventPublisher?.Publish(new WaveClearedEvent(CurrentWave));

        bool isMilestone = milestoneInterval > 0 && CurrentWave > 0 && CurrentWave % milestoneInterval == 0;
        if (isMilestone)
        {
            waitingOnDecision = true;
            _eventPublisher?.Publish(new ExtractionAvailableEvent(CurrentWave));
        }
        else
        {
            AdvanceToNextWave();
        }
    }

    private void HandleExtractionDecision(ExtractionDecisionEvent evt)
    {
        if (!waitingOnDecision) return;
        waitingOnDecision = false;

        if (evt.DidExtract)
        {
            // The hub transition is handled in ExtractionUIController now
        }
        else
        {
            AdvanceToNextWave();
        }
    }

    private void HandlePlayerDied(PlayerDiedEvent evt)
    {
        StopAllCoroutines();
    }
}