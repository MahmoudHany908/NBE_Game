using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using VContainer;

public class GameStateManager : MonoBehaviour, IGameStateManager
{
    private IGameState _currentState;
    private IEventPublisher _eventPublisher;
    private IEventSubscriber _eventSubscriber;
    private ISaveSystem _saveSystem;

    [Inject]
    public void Construct(IEventPublisher eventPublisher, IEventSubscriber eventSubscriber, ISaveSystem saveSystem)
    {
        //Debug.Log("[GameStateManager] Construct called! Injecting dependencies.");
        _eventPublisher = eventPublisher;
        _eventSubscriber = eventSubscriber;
        _saveSystem = saveSystem;

        // Subscribe to incoming UI commands immediately
        _eventSubscriber.Subscribe<ChangeStateCommand>(HandleChangeStateCommand);
        //Debug.Log("[GameStateManager] Successfully subscribed to ChangeStateCommand.");
    }

    private void OnDestroy()
    {
        // Always clean up subscriptions to prevent memory leaks!
        _eventSubscriber?.Unsubscribe<ChangeStateCommand>(HandleChangeStateCommand);
    }

    private void Start()
    {
        ChangeState(new BootState(_saveSystem));
    }

    private void Update()
    {
        _currentState?.UpdateState(this);
    }

    // --- Command Handler ---
    private void HandleChangeStateCommand(ChangeStateCommand command)
    {
        //Debug.Log($"[GameState] Received command to load: {command.TargetSceneName}");

        if (!string.IsNullOrEmpty(command.TargetSceneName))
        {
            LoadSceneAndChangeState(command.TargetSceneName, command.TargetState);
        }
        else
        {
            ChangeState(command.TargetState);
        }
    }

    public void ChangeState(IGameState newState)
    {
        _currentState?.ExitState(this);

        _currentState = newState;
        _currentState.EnterState(this);

        _eventPublisher?.Publish(new GameStateChangedEvent { NewState = _currentState });

        //Debug.Log($"[GameState] Transitioned to: {_currentState.GetType().Name}");
    }

    public void LoadSceneAndChangeState(string sceneName, IGameState stateAfterLoading)
    {
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
