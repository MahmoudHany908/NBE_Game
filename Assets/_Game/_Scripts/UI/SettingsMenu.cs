using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using VContainer; 

public class SettingsMenu : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Button closeButton;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Slider ambientVolumeSlider;
    [SerializeField] private Toggle volumeToggle;

    private IEventPublisher _eventPublisher;
    private IEventSubscriber _eventSubscriber;
    private ISaveSystem _saveSystem;

    [Inject]
    public void Construct(IEventPublisher eventPublisher, IEventSubscriber eventSubscriber, ISaveSystem saveSystem)
    {
        _eventPublisher = eventPublisher;
        _eventSubscriber = eventSubscriber;
        _saveSystem = saveSystem;
    }

    private void Start()
    {
        _eventSubscriber?.Subscribe<Command_CloseAllMenus>(OnCloseAllMenus);
        
        if (_saveSystem != null)
        {
            var data = _saveSystem.Load();
            if (data?.audio != null)
            {
                var audioData = data.audio;
                if (volumeToggle != null) volumeToggle.SetIsOnWithoutNotify(audioData.isMuted);
                if (masterVolumeSlider != null) masterVolumeSlider.SetValueWithoutNotify(audioData.masterVolume);
                if (musicVolumeSlider != null) musicVolumeSlider.SetValueWithoutNotify(audioData.musicVolume);
                if (sfxVolumeSlider != null) sfxVolumeSlider.SetValueWithoutNotify(audioData.sfxVolume);
                if (ambientVolumeSlider != null) ambientVolumeSlider.SetValueWithoutNotify(audioData.ambientVolume);
            }
        }
    }
    
    private void OnDestroy()
    {
        _eventSubscriber?.Unsubscribe<Command_CloseAllMenus>(OnCloseAllMenus);
    }

    private void OnCloseAllMenus(Command_CloseAllMenus cmd)
    {
        CloseSettingsMenu();
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
        if (closeButton != null) closeButton.onClick.AddListener(CloseSettingsMenu);

        // Register control events
        if (volumeToggle != null) volumeToggle.onValueChanged.AddListener(OnMuteTogglePressed);
        if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeSliderChange);
        if (musicVolumeSlider != null) musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeSliderChange);
        if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.AddListener(OnSFXSliderChange);
        if (ambientVolumeSlider != null) ambientVolumeSlider.onValueChanged.AddListener(OnAmbientSliderChange);
    }

    private void OnDisable()
    {
        if (closeButton != null) closeButton.onClick.RemoveListener(CloseSettingsMenu);

        if (volumeToggle != null) volumeToggle.onValueChanged.RemoveListener(OnMuteTogglePressed);
        if (masterVolumeSlider != null) masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeSliderChange);
        if (musicVolumeSlider != null) musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeSliderChange);
        if (sfxVolumeSlider != null) sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXSliderChange);
        if (ambientVolumeSlider != null) ambientVolumeSlider.onValueChanged.RemoveListener(OnAmbientSliderChange);
    }

    public void OpenSettingsMenu()
    {
        if (gameObject.activeSelf) return;
        _eventPublisher?.Publish(new Command_CloseAllMenus());
        gameObject.SetActive(true);
        _eventPublisher?.Publish(new Event_MenuStateChanged { isOpen = true });
    }

    public void CloseSettingsMenu()
    {
        if (!gameObject.activeSelf) return;
        gameObject.SetActive(false);
        _eventPublisher?.Publish(new Event_MenuStateChanged { isOpen = false });
    }

    private void OnMuteTogglePressed(bool newValue) => _eventPublisher?.Publish(new OnMuteTogglePressedEvent { newValue = newValue });
    private void OnMasterVolumeSliderChange(float newValue) => _eventPublisher?.Publish(new OnMasterVolumeChangedEvent { newValue = newValue });
    private void OnMusicVolumeSliderChange(float newValue) => _eventPublisher?.Publish(new OnMusicVolumeChangedEvent { newValue = newValue });
    private void OnSFXSliderChange(float newValue) => _eventPublisher?.Publish(new OnSFXSliderChangeEvent { newValue = newValue });
    private void OnAmbientSliderChange(float newValue) => _eventPublisher?.Publish(new OnAmbientSliderChangeEvent { newValue = newValue });

}

public struct OnMuteTogglePressedEvent : IGameEvent { public bool newValue; }
public struct OnMasterVolumeChangedEvent : IGameEvent { public float newValue; }
public struct OnMusicVolumeChangedEvent : IGameEvent { public float newValue; }
public struct OnSFXSliderChangeEvent : IGameEvent { public float newValue; }
public struct OnAmbientSliderChangeEvent : IGameEvent { public float newValue; }