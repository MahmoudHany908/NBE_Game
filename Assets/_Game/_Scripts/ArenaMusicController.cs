using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using VContainer;

public class ArenaMusicController : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private SoundData arenaMusic;

    [Inject] private AudioManager audioManager;
    [Inject] IEventSubscriber subscriber;

    private int _openMenus = 0;

    IEnumerator Start()
    {
        yield return new WaitForSeconds(2f);
        audioManager.PlayMusic(arenaMusic);
    }

    private void OnEnable()
    {
        subscriber.Subscribe<Event_MenuStateChanged>(OnMenuStateChanged);
    }
    
    private void OnDisable()
    {
        subscriber.Unsubscribe<Event_MenuStateChanged>(OnMenuStateChanged);
        
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
                audioManager.PauseMusic();
            }
        }
        else
        {
            _openMenus--;
            if (_openMenus <= 0)
            {
                _openMenus = 0;
                audioManager.ResumeMusic();
            }
        }
    }
}
