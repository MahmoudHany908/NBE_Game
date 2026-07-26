using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VContainer;

public class GateLoadoutUIController : MonoBehaviour
{
    private IEventPublisher _eventPublisher;
    private IEventSubscriber _eventSubscriber;

    [Header("Main Panel")]
    [SerializeField] private GameObject _panelContainer;
    [SerializeField] private Button _closeBtn;
    [SerializeField] private Button _startRunBtn;
    
    [Header("Target Scene for Run")]
    [SerializeField] private string targetSceneName = "ArenaScene";

    [Header("UI Element Instantiation")]
    [SerializeField] private Transform _equippedCharactersContainer;
    [SerializeField] private GameObject _equippedCharacterElementPrefab;
    [SerializeField] private TMP_Text _emptyLoadoutWarning;
    
    [Header("Data")]
    [SerializeField] private SupportCharacterRegistrySO _registry;

    [Inject]
    public void Construct(IEventPublisher eventPublisher, IEventSubscriber eventSubscriber)
    {
        _eventPublisher = eventPublisher;
        _eventSubscriber = eventSubscriber;
    }

    private void Awake()
    {
        if (_panelContainer != null)
        {
            _panelContainer.SetActive(false);
        }
    }

    private void OnEnable()
    {
        if (_closeBtn != null) _closeBtn.onClick.AddListener(CloseUI);
        if (_startRunBtn != null) _startRunBtn.onClick.AddListener(OnStartRunClicked);
    }

    private void OnDisable()
    {
        if (_closeBtn != null) _closeBtn.onClick.RemoveListener(CloseUI);
        if (_startRunBtn != null) _startRunBtn.onClick.RemoveListener(OnStartRunClicked);
    }

    private void Start()
    {
        _eventSubscriber?.Subscribe<Event_WorkshopStateUpdated>(OnStateUpdated);
        _eventSubscriber?.Subscribe<Command_OpenGateLoadoutUI>(OnOpenUI);
        _eventSubscriber?.Subscribe<Command_CloseGateLoadoutUI>(OnCloseUI);
        _eventSubscriber?.Subscribe<Command_CloseAllMenus>(OnCloseAllMenus);
    }

    private void OnDestroy()
    {
        _eventSubscriber?.Unsubscribe<Event_WorkshopStateUpdated>(OnStateUpdated);
        _eventSubscriber?.Unsubscribe<Command_OpenGateLoadoutUI>(OnOpenUI);
        _eventSubscriber?.Unsubscribe<Command_CloseGateLoadoutUI>(OnCloseUI);
        _eventSubscriber?.Unsubscribe<Command_CloseAllMenus>(OnCloseAllMenus);
    }

    private void OnCloseAllMenus(Command_CloseAllMenus cmd)
    {
        OnCloseUI(new Command_CloseGateLoadoutUI());
    }

    private void OnOpenUI(Command_OpenGateLoadoutUI cmd)
    {
        if (_panelContainer != null && !_panelContainer.activeSelf)
        {
            _eventPublisher?.Publish(new Command_CloseAllMenus());
            _panelContainer.SetActive(true);
            _eventPublisher?.Publish(new Event_MenuStateChanged { isOpen = true });

            _eventPublisher?.Publish(new Command_RequestWorkshopState());
        }
    }

    private void OnCloseUI(Command_CloseGateLoadoutUI cmd)
    {
        if (_panelContainer != null && _panelContainer.activeSelf)
        {
            _panelContainer.SetActive(false);
            _eventPublisher?.Publish(new Event_MenuStateChanged { isOpen = false });
        }
    }

    private void CloseUI()
    {
        OnCloseUI(null);
    }

    private void OnStartRunClicked()
    {
        _eventPublisher?.Publish(new ChangeStateCommand
        {
            TargetSceneName = targetSceneName,
            TargetState = new ArenaState()
        });
        
        CloseUI();
    }

    private void OnStateUpdated(Event_WorkshopStateUpdated evt)
    {
        if (_registry == null || _equippedCharactersContainer == null || _equippedCharacterElementPrefab == null)
            return;

        // Clear existing elements
        foreach (Transform child in _equippedCharactersContainer)
        {
            Destroy(child.gameObject);
        }

        bool hasEquipped = evt.EquippedIds.Count > 0;
        if (_emptyLoadoutWarning != null)
        {
            _emptyLoadoutWarning.gameObject.SetActive(!hasEquipped);
        }

        // Generate UI for equipped characters
        foreach (var charId in evt.EquippedIds)
        {
            var charData = _registry.Characters.Find(c => c.CharacterId == charId);
            if (charData != null)
            {
                var go = Instantiate(_equippedCharacterElementPrefab, _equippedCharactersContainer);
                var element = go.GetComponent<GateLoadoutCharacterUIElement>();
                if (element != null)
                {
                    element.Setup(charData);
                }
            }
        }
    }
}
