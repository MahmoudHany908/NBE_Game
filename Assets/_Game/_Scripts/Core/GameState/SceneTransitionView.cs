using UnityEngine;
using UnityEngine.UIElements;
using VContainer;

public class SceneTransitionView : MonoBehaviour
{
    [Header("Transition Settings")]
    [SerializeField] private string targetSceneName = "OsamaCombat";
    [SerializeField] private bool isTransitioningToArena = true;

    [Header("UI Reference")]
    [SerializeField] private UIDocument _document;

    private Button _transitionButton;
    private IEventPublisher _eventPublisher;

    [Inject]
    public void Construct(IEventPublisher eventPublisher)
    {
        _eventPublisher = eventPublisher;
    }


    private void OnEnable()
    {
        if (_document == null) return;

        var root = _document.rootVisualElement;

        _transitionButton = root.Q<Button>("transition-button");

        if (_transitionButton != null)
        {
            _transitionButton.clicked += OnTransitionButtonClicked;
        }
    }

    private void OnDisable()
    {
        if (_transitionButton != null)
        {
            _transitionButton.clicked -= OnTransitionButtonClicked;
        }
    }

    private void OnTransitionButtonClicked()
    {
        IGameState nextState = isTransitioningToArena ? new ArenaState() : new HideoutState();

        VaultSurvivors.LoadingSystem.SceneLoader.Load(targetSceneName, nextState);
    }
}