using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameStateManager : MonoBehaviour
{
    public static GameStateManager Instance { get; private set; }

    // Event broadcast when the state changes (Time Manager will listen to this)
    public static event Action<IGameState> OnGameStateChanged;
    private IGameState _currentState;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }
    }

    private void Start()
    {
        ChangeState(new BootState());
    }

    private void Update()
    {
        _currentState?.UpdateState(this);
    }

    public void ChangeState(IGameState newState)
    {
        _currentState?.ExitState(this);

        _currentState = newState;
        _currentState.EnterState(this);

        OnGameStateChanged?.Invoke(_currentState);

        Debug.Log($"[GameState] Transitioned to: {_currentState.GetType().Name}");
    }
    public void LoadSceneAndChangeState(string sceneName, IGameState stateAfterLoading)
    {
        // Start the loading process on a separate timeline
        StartCoroutine(LoadSceneRoutine(sceneName, stateAfterLoading));
    }

    private IEnumerator LoadSceneRoutine(string sceneName, IGameState stateAfterLoading)
    {
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncLoad.isDone)
        {
            yield return null; 
        }
        Resources.UnloadUnusedAssets();
        ChangeState(stateAfterLoading);
    }
}