using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

[RequireComponent(typeof(Button))]
public class SceneToggleButton : MonoBehaviour
{
    [Header("Scene Names")]
    [Tooltip("The exact name of your City/Hideout scene.")]
    public string citySceneName = "00_Boot_Sandbox";

    [Tooltip("The exact name of your Arena scene.")]
    public string arenaSceneName = "02_Arena_Sandbox";

    [Header("UI Text Reference")]
    [SerializeField] private TextMeshProUGUI buttonText;

    private Button _button;
    private bool _isInCity;

    private PanelUI _scenePanel;

    private void Awake()
    {
        _button = GetComponent<Button>();
    }

    private void Start()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        if (currentSceneName == citySceneName)
        {
            _isInCity = true;
            if (buttonText != null) buttonText.text = "Enter Arena";
        }
        else if (currentSceneName == arenaSceneName)
        {
            _isInCity = false;
            if (buttonText != null) buttonText.text = "Return to City";
        }
        else
        {
            _isInCity = true;
            if (buttonText != null) buttonText.text = "Travel";
        }

        if (_button != null)
        {
            _button.onClick.AddListener(OnButtonClicked);
        }


        _scenePanel = FindObjectOfType<PanelUI>(true);
        if (_scenePanel != null)
        {
            _scenePanel.ShowStarted += HideButton;
            _scenePanel.HideCompleted += ShowButton;
        }
    }

    private void OnDestroy()
    {
        if (_button != null)
        {
            _button.onClick.RemoveListener(OnButtonClicked);
        }

        if (_scenePanel != null)
        {
            _scenePanel.ShowStarted -= HideButton;
            _scenePanel.HideCompleted -= ShowButton;
        }
    }

    private void HideButton()
    {
        gameObject.SetActive(false);
    }

    private void ShowButton()
    {
        gameObject.SetActive(true);
    }

    private void OnButtonClicked()
    {
        GameStateManager stateManager = FindObjectOfType<GameStateManager>();

        if (stateManager != null)
        {
            if (_isInCity)
            {
                stateManager.LoadSceneAndChangeState(arenaSceneName, new ArenaState());
            }
            else
            {
                stateManager.LoadSceneAndChangeState(citySceneName, new HideoutState());
            }
        }
    }
}