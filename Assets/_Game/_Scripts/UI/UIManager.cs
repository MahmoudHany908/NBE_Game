using UnityEngine;
using VContainer;

public class UIManager : MonoBehaviour
{
    [SerializeField] private GameObject _backgroundPanel;
    [SerializeField] private UnityEngine.UI.Button _settingsButton;
    
    private IEventSubscriber _eventSubscriber;
    private int _openMenuCount = 0;

    [Inject]
    public void Construct(IEventSubscriber eventSubscriber)
    {
        _eventSubscriber = eventSubscriber;
    }

    private void Start()
    {
        _eventSubscriber?.Subscribe<Event_MenuStateChanged>(OnMenuStateChanged);
        
        if (_backgroundPanel != null)
        {
            _backgroundPanel.SetActive(false);
        }

        UpdateSettingsButtonState();
    }

    private void OnDestroy()
    {
        _eventSubscriber?.Unsubscribe<Event_MenuStateChanged>(OnMenuStateChanged);
    }

    private void OnMenuStateChanged(Event_MenuStateChanged evt)
    {
        if (evt.isOpen)
        {
            _openMenuCount++;
        }
        else
        {
            _openMenuCount--;
        }

        if (_openMenuCount < 0) _openMenuCount = 0;

        if (_backgroundPanel != null)
        {
            _backgroundPanel.SetActive(_openMenuCount > 0);
        }

        UpdateSettingsButtonState();
    }

    private void UpdateSettingsButtonState()
    {
        if (_settingsButton != null)
        {
            _settingsButton.interactable = (_openMenuCount == 0);
        }
    }
}

public struct Event_MenuStateChanged : IGameEvent 
{ 
    public bool isOpen; 
}

public struct Command_CloseAllMenus : IGameEvent { }
