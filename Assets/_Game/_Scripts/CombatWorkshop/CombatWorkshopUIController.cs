using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VContainer;

public class CombatWorkshopUIController : MonoBehaviour
{
    private IEventPublisher _eventPublisher;
    private IEventSubscriber _eventSubscriber;

    [Header("Main Panel")]
    [SerializeField] private GameObject _panelContainer;
    [SerializeField] private Button _closeBtn;

    [Header("Data")]
    [SerializeField] private SupportCharacterRegistrySO _registry;

    [Header("UI Element Instantiation")]
    [SerializeField] private Transform _characterListContainer;
    [SerializeField] private GameObject _characterElementPrefab; // Prefab with a script to handle individual character display

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
    }

    private void OnDisable()
    {
        if (_closeBtn != null) _closeBtn.onClick.RemoveListener(CloseUI);
    }

    private void Start()
    {
        _eventSubscriber?.Subscribe<Event_WorkshopStateUpdated>(OnStateUpdated);
        _eventSubscriber?.Subscribe<Command_OpenCombatWorkshopUI>(OnOpenUI);
        _eventSubscriber?.Subscribe<Command_CloseCombatWorkshopUI>(OnCloseUI);
        _eventSubscriber?.Subscribe<Command_CloseAllMenus>(OnCloseAllMenus);
    }

    private void OnDestroy()
    {
        _eventSubscriber?.Unsubscribe<Event_WorkshopStateUpdated>(OnStateUpdated);
        _eventSubscriber?.Unsubscribe<Command_OpenCombatWorkshopUI>(OnOpenUI);
        _eventSubscriber?.Unsubscribe<Command_CloseCombatWorkshopUI>(OnCloseUI);
        _eventSubscriber?.Unsubscribe<Command_CloseAllMenus>(OnCloseAllMenus);
    }

    private void OnCloseAllMenus(Command_CloseAllMenus cmd)
    {
        OnCloseUI(new Command_CloseCombatWorkshopUI());
    }

    private void OnOpenUI(Command_OpenCombatWorkshopUI cmd)
    {
        if (_panelContainer != null && !_panelContainer.activeSelf)
        {
            _eventPublisher?.Publish(new Command_CloseAllMenus());
            _panelContainer.SetActive(true);
            _eventPublisher?.Publish(new Event_MenuStateChanged { isOpen = true });

            // Ask for immediate data
            _eventPublisher?.Publish(new Command_RequestWorkshopState());
        }
    }

    private void OnCloseUI(Command_CloseCombatWorkshopUI cmd)
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

    private void OnStateUpdated(Event_WorkshopStateUpdated evt)
    {
        if (_registry == null || _characterListContainer == null || _characterElementPrefab == null)
        {
            Debug.LogWarning("[CombatWorkshopUIController] Missing UI references for generating character list.");
            return;
        }

        // Clear existing elements
        foreach (Transform child in _characterListContainer)
        {
            Destroy(child.gameObject);
        }

        // Generate UI for each character
        foreach (var characterData in _registry.Characters)
        {
            var go = Instantiate(_characterElementPrefab, _characterListContainer);
            // Assuming the prefab has a script like CombatWorkshopCharacterUIElement
            var element = go.GetComponent<CombatWorkshopCharacterUIElement>();
            if (element != null)
            {
                bool isUnlocked = evt.UnlockedIds.Contains(characterData.CharacterId);
                bool isEquipped = evt.EquippedIds.Contains(characterData.CharacterId);
                bool canEquipMore = evt.EquippedIds.Count < _registry.MaxLoadoutSlots;

                element.Setup(characterData, isUnlocked, isEquipped, canEquipMore, _eventPublisher);
            }
        }
    }
}
