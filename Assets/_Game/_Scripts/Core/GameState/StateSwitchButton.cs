using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Button))]
public class StateSwitchButton : MonoBehaviour
{
    [Header("Transition Settings")]
    [Tooltip("Check this to go to the Arena. Uncheck it to go to the City/Hideout.")]
    public bool goToArena = true;

    [Tooltip("The exact name of the scene to load.")]
    public string targetSceneName = "02_Arena_Sandbox";

    private Button _transitionButton;

    private void Awake()
    {
        _transitionButton = GetComponent<Button>();
    }

    private void Start()
    {
        if (_transitionButton != null)
        {
            _transitionButton.onClick.AddListener(OnButtonClicked);
        }
    }

    private void OnDestroy()
    {
        if (_transitionButton != null)
        {
            _transitionButton.onClick.RemoveListener(OnButtonClicked);
        }
    }

    private void OnButtonClicked()
    {
        GameStateManager stateManager = FindObjectOfType<GameStateManager>();

        if (stateManager != null)
        {
            if (goToArena)
            {
                Debug.Log($"[Transition] Moving to Arena: {targetSceneName}");
                stateManager.LoadSceneAndChangeState(targetSceneName, new ArenaState());
            }
            else
            {
                Debug.Log($"[Transition] Moving to City/Hideout: {targetSceneName}");
                stateManager.LoadSceneAndChangeState(targetSceneName, new HideoutState());
            }
        }
        else
        {
            Debug.LogError("[StateSwitchButton] GameStateManager not found in the scene!");
        }
    }
}