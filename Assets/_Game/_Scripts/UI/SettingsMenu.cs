using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using VContainer; 

public class SettingsMenu : MonoBehaviour
{
    [SerializeField] private UIDocument document;

    private VisualElement rootContainer;
    private VisualElement backgroundOverlay;
    private VisualElement settingsPanel;

    private Button closeButton;
    private Toggle volumeToggle;
    private Slider masterVolumeSlider, musicVolumeSlider, sfxVolumeSlider, ambientVolumeSlider;

    private IEventPublisher _eventPublisher;

    [Inject]
    public void Construct(IEventPublisher eventPublisher)
    {
        _eventPublisher = eventPublisher;
    }

    private void Awake()
    {
        var root = document.rootVisualElement;

        rootContainer = root.Q<VisualElement>("root-container");
        backgroundOverlay = root.Q<VisualElement>("background-overlay");
        settingsPanel = root.Q<VisualElement>("settings-panel");
        closeButton = root.Q<Button>("close-button");

        volumeToggle = root.Q<Toggle>("volume-toggle");
        masterVolumeSlider = root.Q<Slider>("master-volume");
        musicVolumeSlider = root.Q<Slider>("music-volume");
        sfxVolumeSlider = root.Q<Slider>("sfx-volume");
        ambientVolumeSlider = root.Q<Slider>("ambient-volume");
    }

    // Eventually we might want to replace this direct 
    // SceneManager call with an event that the GameStateManager listens to!
    public void LoadAreana(string name)
    {
        SceneManager.LoadScene(name);
    }

    private void OnEnable()
    {
        // Register new close events
        closeButton.clicked += CloseSettingsMenu;
        backgroundOverlay.RegisterCallback<ClickEvent>(evt => CloseSettingsMenu());

        // Register control events
        volumeToggle.RegisterValueChangedCallback(OnMuteTogglePressed);
        masterVolumeSlider.RegisterValueChangedCallback(OnMasterVolumeSliderChange);
        musicVolumeSlider.RegisterValueChangedCallback(OnMusicVolumeSliderChange);
        sfxVolumeSlider.RegisterValueChangedCallback(OnSFXSliderChange);
        ambientVolumeSlider.RegisterValueChangedCallback(OnAmbientSliderChange);

        // Ensure the menu is closed by default when this script enables
        CloseSettingsMenu();
    }

    private void OnDisable()
    {
        closeButton.clicked -= CloseSettingsMenu;
        backgroundOverlay.UnregisterCallback<ClickEvent>(evt => CloseSettingsMenu());

        volumeToggle.UnregisterValueChangedCallback(OnMuteTogglePressed);
        masterVolumeSlider.UnregisterValueChangedCallback(OnMasterVolumeSliderChange);
        musicVolumeSlider.UnregisterValueChangedCallback(OnMusicVolumeSliderChange);
        sfxVolumeSlider.UnregisterValueChangedCallback(OnSFXSliderChange);
        ambientVolumeSlider.UnregisterValueChangedCallback(OnAmbientSliderChange);
    }

    public void OpenSettingsMenu()
    {
        rootContainer.style.display = DisplayStyle.Flex;
    }

    public void CloseSettingsMenu()
    {
        rootContainer.style.display = DisplayStyle.None;
    }

    private void OnMuteTogglePressed(ChangeEvent<bool> evt) => _eventPublisher?.Publish(new OnMuteTogglePressedEvent { newValue = evt.newValue });
    private void OnMasterVolumeSliderChange(ChangeEvent<float> evt) => _eventPublisher?.Publish(new OnMasterVolumeChangedEvent { newValue = evt.newValue });
    private void OnMusicVolumeSliderChange(ChangeEvent<float> evt) => _eventPublisher?.Publish(new OnMusicVolumeChangedEvent { newValue = evt.newValue });
    private void OnSFXSliderChange(ChangeEvent<float> evt) => _eventPublisher?.Publish(new OnSFXSliderChangeEvent { newValue = evt.newValue });
    private void OnAmbientSliderChange(ChangeEvent<float> evt) => _eventPublisher?.Publish(new OnAmbientSliderChangeEvent { newValue = evt.newValue });

}

public struct OnMuteTogglePressedEvent : IGameEvent { public bool newValue; }
public struct OnMasterVolumeChangedEvent : IGameEvent { public float newValue; }
public struct OnMusicVolumeChangedEvent : IGameEvent { public float newValue; }
public struct OnSFXSliderChangeEvent : IGameEvent { public float newValue; }
public struct OnAmbientSliderChangeEvent : IGameEvent { public float newValue; }