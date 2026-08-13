using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class HubMusicController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SoundData hubMusic;

    [Header("Menu Ducking")]
    [Range(0f, 1f)][SerializeField] private float menuDuckingMultiplier = 0.4f;
    [SerializeField] private float duckingFadeDuration = 0.3f;

    private AudioManager audioManager;
    private IEventSubscriber subscriber;

    [Inject]
    public void Construct(AudioManager audioManager, IEventSubscriber subscriber)
    {
        this.audioManager = audioManager;
        this.subscriber = subscriber;

        // Subscribe here if OnEnable already fired before VContainer injection completed
        if (isActiveAndEnabled)
        {
            this.subscriber.Subscribe<Event_MenuStateChanged>(OnMenuStateChanged);
        }
    }
    
    private int _openMenus = 0;

    IEnumerator Start()
    {
        yield return new WaitForSeconds(2f);
        if (audioManager != null)
        {
            audioManager.PlayMusic(hubMusic);
        }
    }

    private void OnEnable()
    {
        if (subscriber != null)
        {
            subscriber.Subscribe<Event_MenuStateChanged>(OnMenuStateChanged);
        }
    }
    
    private void OnDisable()
    {
        if (subscriber != null)
        {
            subscriber.Unsubscribe<Event_MenuStateChanged>(OnMenuStateChanged);
        }
        
        if (audioManager != null)
        {
            audioManager.StopMusic();
        }
    }

    private void OnMenuStateChanged(Event_MenuStateChanged evt)
    {
        if (evt.isOpen)
        {
            _openMenus++;
            if (_openMenus == 1)
            {
                audioManager.SetMusicDucking(true, menuDuckingMultiplier, duckingFadeDuration);
            }
        }
        else
        {
            _openMenus--;
            if (_openMenus <= 0)
            {
                _openMenus = 0;
                audioManager.SetMusicDucking(false, menuDuckingMultiplier, duckingFadeDuration);
            }
        }
    }
}
