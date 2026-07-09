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

    private void OnEnable()
    {
        waveSpawner.OnAllSquadsSpawned += HandleAllSquadsSpawned;
        EventBus.Subscribe<ExtractionDecisionEvent>(HandleExtractionDecision);
    }

    private void OnDisable()
    {
        waveSpawner.OnAllSquadsSpawned -= HandleAllSquadsSpawned;
        EventBus.Unsubscribe<ExtractionDecisionEvent>(HandleExtractionDecision);
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

        EventBus.Publish(new WaveClearedEvent(currentWave));

        bool isMilestone = milestoneInterval > 0 && currentWave > 0 && currentWave % milestoneInterval == 0;
        if (isMilestone)
        {
            waitingOnDecision = true;
            EventBus.Publish(new ExtractionAvailableEvent(currentWave));
            // Loop pauses here — HandleExtractionDecision resumes it.
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
            // TODO: hook up to your actual hub-transition (scene load / scope teardown).
            // e.g. hubTransitionService.ReturnToHub();
        }
        else
        {
            AdvanceToNextWave();
        }
    }
}