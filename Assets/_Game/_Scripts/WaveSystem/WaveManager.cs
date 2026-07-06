using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class WaveManager : MonoBehaviour
{
    [SerializeField] private float preparationTime = 10f;
    [SerializeField] private TextMeshProUGUI countdownText;

    public static WaveManager Instance { get; private set; }
    public int CurrentWave { get; private set; }
    public bool IsWaveActive { get; private set; }

    public static event Action<int> WaveStarted;
    public static event Action<int> WaveEnded;

    private WaveSpawner waveSpawner;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else { Destroy(gameObject); return; }

        waveSpawner = GetComponent<WaveSpawner>();
    }

    private IEnumerator Start()
    {
        yield return new WaitForSeconds(5f);
        StartPreparationPhase();
    }

    private void Update()
    {
        if (!IsWaveActive) return;

        if (waveSpawner.AreAllEnemiesDead())
            EndWave();
    }

    private void StartPreparationPhase()
    {
        StartCoroutine(PreparationCountdownRoutine());
    }

    private IEnumerator PreparationCountdownRoutine()
    {
        float timeLeft = preparationTime;

        while (timeLeft > 0f)
        {
            countdownText.text = "Next Wave In: " + Mathf.CeilToInt(timeLeft);
            yield return new WaitForSeconds(1f);
            timeLeft -= 1f;
        }

        countdownText.text = "Wave Starting!";
        yield return new WaitForSeconds(1f);
        countdownText.text = "";

        StartWave();
    }

    private void StartWave()
    {
        CurrentWave++;
        IsWaveActive = true;
        WaveStarted?.Invoke(CurrentWave);
        waveSpawner.StartSpawningWave(CurrentWave);
        Debug.Log($"Wave {CurrentWave} started.");
    }

    private void EndWave()
    {
        IsWaveActive = false;
        WaveEnded?.Invoke(CurrentWave);
        Debug.Log($"Wave {CurrentWave} ended.");
        StartPreparationPhase();
    }
}