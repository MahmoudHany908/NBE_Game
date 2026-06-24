using UnityEngine;
using UnityEngine.UI;
public class SettingsMenu : MonoBehaviour
{
    [Header("Settings Menu")]
    [SerializeField] private PanelUI settingsMenu;
    [Space(25)]
    //sound settings
    [SerializeField] private Toggle volumeToggle;
    [SerializeField] private Slider masterVolumeSlider;
    [SerializeField] private Slider musicVolumeSlider;
    [SerializeField] private Slider sfxVolumeSlider;
    [SerializeField] private Slider ambientVolumeSlider;
    //checkbox mute settings





    // [Header("Buttons")]
    // [SerializeField] private Button settingsButton;
    // [SerializeField] private Button closeButton;

    private void OnEnable()
    {
        // add listeners

        // buttons
        // settingsButton.onClick.AddListener(OpenSettingsMenu);
        // closeButton.onClick.AddListener(CloseSettingsMenu);

        // toggle
        volumeToggle.onValueChanged.AddListener(OnMuteTogglePressed);

        //sliders
        masterVolumeSlider.onValueChanged.AddListener(OnMasterVolumeSliderChange);
        musicVolumeSlider.onValueChanged.AddListener(OnMusicVolumeSliderChange);
        sfxVolumeSlider.onValueChanged.AddListener(OnSFXSliderChange);
        ambientVolumeSlider.onValueChanged.AddListener(OnAmbientSliderChange);

    }
    private void OnDisable()
    {
        //remove listeners

        //buttons
        // settingsButton.onClick.RemoveListener(OpenSettingsMenu);
        // closeButton.onClick.RemoveListener(CloseSettingsMenu);

        //toggle
        volumeToggle.onValueChanged.RemoveListener(OnMuteTogglePressed);

        // sliders
        masterVolumeSlider.onValueChanged.RemoveListener(OnMasterVolumeSliderChange);
        musicVolumeSlider.onValueChanged.RemoveListener(OnMusicVolumeSliderChange);
        sfxVolumeSlider.onValueChanged.RemoveListener(OnSFXSliderChange);
        ambientVolumeSlider.onValueChanged.RemoveListener(OnAmbientSliderChange);



    }



    // private void OpenSettingsMenu()
    // {
    //     settingsMenu.Show();
    // }
    // private void CloseSettingsMenu()
    // {
    //     settingsMenu.Hide();  
    // }

    private void OnMuteTogglePressed(bool mute)
    {

    }

    private void OnMasterVolumeSliderChange(float volume)
    {

    }

    private void OnMusicVolumeSliderChange(float volume)
    {

    }


    private void OnSFXSliderChange(float volume)
    {

    }

    private void OnAmbientSliderChange(float volume)
    {

    }
}
