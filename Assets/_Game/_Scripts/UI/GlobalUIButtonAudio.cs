using UnityEngine;
using UnityEngine.UI;
using VContainer;
using System.Collections.Generic;

public class GlobalUIButtonAudio : MonoBehaviour
{
    [Header("Audio Settings")]
    [Tooltip("The sound to play when any generic button is clicked (e.g. Settings, Back).")]
    public SoundData genericClickSound;
    
    [Header("Special Neon Sounds")]
    [Tooltip("The sound to play when the Upgrade button is clicked.")]
    public SoundData upgradeClickSound;

    [Tooltip("The sound to play when the Unlock button is clicked.")]
    public SoundData unlockClickSound;

    [Tooltip("The sound to play when a Sell button is clicked.")]
    public SoundData sellClickSound;

    private AudioManager _audioManager;

    [Inject]
    public void Construct(AudioManager audioManager)
    {
        _audioManager = audioManager;
    }

    private void Start()
    {
        WireUpAllButtons();
    }

    private void WireUpAllButtons()
    {
        // Find ALL buttons in the entire scene, including inactive ones (like in hidden panels)
        Button[] allButtons = Object.FindObjectsByType<Button>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        
        int wiredCount = 0;
        int specialCount = 0;

        foreach (Button btn in allButtons)
        {
            SpecialButtonAudioTag tag = btn.GetComponent<SpecialButtonAudioTag>();

            if (tag != null)
            {
                btn.onClick.AddListener(() =>
                {
                    switch (tag.buttonType)
                    {
                        case SpecialButtonType.Upgrade: PlaySound(upgradeClickSound); break;
                        case SpecialButtonType.Unlock: PlaySound(unlockClickSound); break;
                        case SpecialButtonType.Sell: PlaySound(sellClickSound); break;
                    }
                });
                specialCount++;
            }
            else
            {
                btn.onClick.AddListener(() => PlaySound(genericClickSound));
            }
            
            wiredCount++;
        }

        Debug.Log($"[GlobalUIButtonAudio] Successfully wired up {wiredCount} buttons for SFX ({specialCount} are special Neon buttons).");
    }

    private void PlaySound(SoundData data)
    {
        if (data == null) return;

        if (_audioManager != null)
        {
            // Play at camera position since UI sounds are usually 2D/stereo, 
            // but the AudioManager expects a Vector3 for pooled 3D sources.
            Vector3 pos = Camera.main != null ? Camera.main.transform.position : Vector3.zero;
            _audioManager.PlaySoundAtPosition(data, pos);
        }
        else
        {
            Debug.LogWarning("[GlobalUIButtonAudio] Tried to play a button sound but AudioManager was not injected!");
        }
    }
}
