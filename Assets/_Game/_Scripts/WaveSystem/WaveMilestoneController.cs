using System.Collections;
using UnityEngine;
using VContainer;

public class WaveMilestoneController : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private WaveSpawner waveSpawner;

    [Header("Milestone Config")]
    [SerializeField] private int milestoneInterval = 5;

    private int currentWave = 0;
    private bool waitingOnDecision = false;
    private bool squadsFinishedSpawning = false;

    private IEventPublisher _eventPublisher;
    private IEventSubscriber _eventSubscriber;

    [Inject]
    public void Construct(IEventPublisher eventPublisher, IEventSubscriber eventSubscriber)
    {
        _eventPublisher = eventPublisher;
        _eventSubscriber = eventSubscriber;
    }

    private void OnEnable()
    {
        waveSpawner.OnAllSquadsSpawned += HandleAllSquadsSpawned;
        _eventSubscriber?.Subscribe<ExtractionDecisionEvent>(HandleExtractionDecision);
    }

    private void OnDisable()
    {
        waveSpawner.OnAllSquadsSpawned -= HandleAllSquadsSpawned;
        _eventSubscriber?.Unsubscribe<ExtractionDecisionEvent>(HandleExtractionDecision);
    }

    /// <summary>Call this once, e.g. when the arena scope is entered, to kick off wave 1.</summary>
    public void BeginRun()
    {
        currentWave = 0;
        AdvanceToNextWave();
    }

    private void AdvanceToNextWave()
    {
        currentWave++;
        squadsFinishedSpawning = false;
        waveSpawner.StartSpawningWave(currentWave);
    }

    private void HandleAllSquadsSpawned()
    {
        squadsFinishedSpawning = true;
        StartCoroutine(WaitForWaveClear());
    }

    private IEnumerator WaitForWaveClear()
    {
        while (!waveSpawner.AreAllEnemiesDead())
            yield return null;

        _eventPublisher?.Publish(new WaveClearedEvent(currentWave));

        bool isMilestone = milestoneInterval > 0 && currentWave > 0 && currentWave % milestoneInterval == 0;
        if (isMilestone)
        {
            waitingOnDecision = true;
            _eventPublisher?.Publish(new ExtractionAvailableEvent(currentWave));
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
}