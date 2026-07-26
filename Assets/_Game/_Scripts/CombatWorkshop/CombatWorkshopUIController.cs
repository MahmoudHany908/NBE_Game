using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VContainer;

[Serializable]
public class CharacterUIMapping
{
    public string CharacterId; // e.g. "Doctor", "Seele", etc.
    
    [Header("Phase 1: Selection Lists")]
    public GameObject UnlockedGameObject;
    public GameObject LockedGameObject;
    [Tooltip("The 'LVL X REQUIRED' texts inside the Locked card")]
    public TextMeshProUGUI LevelRequiredText1;
    public TextMeshProUGUI LevelRequiredText2;

    [Header("Phase 2: Details Screen")]
    public GameObject DetailArtGameObject;
    public GameObject DetailDescriptionGameObject;
}

public class CombatWorkshopUIController : MonoBehaviour
{
    private IEventPublisher _eventPublisher;
    private IEventSubscriber _eventSubscriber;

    [Header("Main Canvas")]
    [SerializeField] private GameObject _panelContainer;
    [SerializeField] private Button _backBtn;

    [Header("Data")]
    [SerializeField] private SupportCharacterRegistrySO _registry;

    [Header("Phase 1: Character Selection")]
    [SerializeField] private GameObject _characterSelectionPanel;
    [Tooltip("Map each character ID to its specific hand-made GameObjects in your scene")]
    [SerializeField] private List<CharacterUIMapping> _characterMappings = new List<CharacterUIMapping>();

    [Header("Phase 2: Character Unlock / Details")]
    [SerializeField] private GameObject _characterDetailsPanel;
    [SerializeField] private TMP_Text _detailCost;
    [SerializeField] private Button _unlockBtn;

    private Event_WorkshopStateUpdated _lastState;
    private SupportCharacterSO _currentlyViewedCharacter;

    [Inject]
    public void Construct(IEventPublisher eventPublisher, IEventSubscriber eventSubscriber)
    {
        _eventPublisher = eventPublisher;
        _eventSubscriber = eventSubscriber;
    }

    private void Awake()
    {
        if (_panelContainer != null) _panelContainer.SetActive(false);
    }

    private void OnEnable()
    {
        if (_backBtn != null) _backBtn.onClick.AddListener(OnBackClicked);
        if (_unlockBtn != null) _unlockBtn.onClick.AddListener(OnUnlockClicked);
    }

    private void OnDisable()
    {
        if (_backBtn != null) _backBtn.onClick.RemoveListener(OnBackClicked);
        if (_unlockBtn != null) _unlockBtn.onClick.RemoveListener(OnUnlockClicked);
    }

    private void Start()
    {
        _eventSubscriber?.Subscribe<Event_WorkshopStateUpdated>(OnStateUpdated);
        _eventSubscriber?.Subscribe<Command_OpenCombatWorkshopUI>(OnOpenUI);
        _eventSubscriber?.Subscribe<Command_CloseCombatWorkshopUI>(OnCloseUI);
        _eventSubscriber?.Subscribe<Command_CloseAllMenus>(OnCloseAllMenus);

        // Wire up buttons and set static level text from the registry immediately
        foreach (var mapping in _characterMappings)
        {
            if (mapping.UnlockedGameObject != null)
            {
                Button btn = mapping.UnlockedGameObject.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => OpenDetailsPhase(mapping.CharacterId));
                }
            }

            if (mapping.LockedGameObject != null)
            {
                Button btn = mapping.LockedGameObject.GetComponent<Button>();
                if (btn != null)
                {
                    btn.onClick.RemoveAllListeners();
                    btn.onClick.AddListener(() => OpenDetailsPhase(mapping.CharacterId));
                }
            }

            // Set the level text immediately from the SO — no need to wait for state event
            if (_registry != null)
            {
                var charData = _registry.Characters.Find(c => c.CharacterId == mapping.CharacterId);
                if (charData != null)
                {
                    string reqText = $"LVL {charData.RequiredHideoutLevel} REQUIRED";
                    if (mapping.LevelRequiredText1 != null) mapping.LevelRequiredText1.text = reqText;
                    if (mapping.LevelRequiredText2 != null) mapping.LevelRequiredText2.text = reqText;
                }
            }
        }
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
        // If a global close (like ESC) happens, we just act like a back button press
        // which will either go back a step, or close the UI completely.
        if (_panelContainer != null && _panelContainer.activeSelf)
        {
            OnBackClicked();
        }
    }

    private void OnOpenUI(Command_OpenCombatWorkshopUI cmd)
    {
        if (_panelContainer != null && !_panelContainer.activeSelf)
        {
            _eventPublisher?.Publish(new Command_CloseAllMenus());
            _panelContainer.SetActive(true);
            _eventPublisher?.Publish(new Event_MenuStateChanged { isOpen = true });

            OpenSelectionPhase();

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

    public void OnBackClicked()
    {
        if (_characterDetailsPanel != null && _characterDetailsPanel.activeSelf)
        {
            // We are in details phase, go back to selection
            OpenSelectionPhase();
        }
        else
        {
            // We are in selection phase, close entirely
            OnCloseUI(null);
        }
    }

    private void OpenSelectionPhase()
    {
        if (_characterSelectionPanel != null) _characterSelectionPanel.SetActive(true);
        if (_characterDetailsPanel != null) _characterDetailsPanel.SetActive(false);
        _currentlyViewedCharacter = null;
    }

    private void OpenDetailsPhase(string characterId)
    {
        _currentlyViewedCharacter = _registry.Characters.Find(c => c.CharacterId == characterId);
        if (_currentlyViewedCharacter == null) return;

        if (_characterSelectionPanel != null) _characterSelectionPanel.SetActive(false);
        if (_characterDetailsPanel != null) _characterDetailsPanel.SetActive(true);

        // Turn on the correct Art & Description GameObjects
        foreach (var mapping in _characterMappings)
        {
            bool isCurrent = (mapping.CharacterId == characterId);
            
            if (mapping.DetailArtGameObject != null) 
                mapping.DetailArtGameObject.SetActive(isCurrent);
                
            if (mapping.DetailDescriptionGameObject != null)
                mapping.DetailDescriptionGameObject.SetActive(isCurrent);
        }

        RefreshDetailsView();
    }

    private void RefreshDetailsView()
    {
        if (_currentlyViewedCharacter == null || _lastState == null) return;

        bool isUnlocked = _lastState.UnlockedIds.Contains(_currentlyViewedCharacter.CharacterId);

        if (_detailCost != null)
        {
            if (isUnlocked)
            {
                _detailCost.text = "Unlocked";
            }
            else if (_currentlyViewedCharacter.UnlockCost != null && _currentlyViewedCharacter.UnlockCost.Count > 0)
            {
                _detailCost.text = $"Requires: {_currentlyViewedCharacter.UnlockCost[0].Amount} {_currentlyViewedCharacter.UnlockCost[0].Type}";
            }
            else
            {
                _detailCost.text = "Free to Unlock";
            }
        }

        if (_unlockBtn != null) _unlockBtn.gameObject.SetActive(!isUnlocked);
    }

    private void OnUnlockClicked()
    {
        if (_currentlyViewedCharacter != null)
        {
            _eventPublisher?.Publish(new Command_UnlockCharacter(_currentlyViewedCharacter.CharacterId));
        }
    }

    private void OnStateUpdated(Event_WorkshopStateUpdated evt)
    {
        _lastState = evt;

        // Toggle the hand-built UI game objects based on the mapping
        foreach (var mapping in _characterMappings)
        {
            bool isAvailableToView = false;
            
            var charData = _registry?.Characters.Find(c => c.CharacterId == mapping.CharacterId);
            if (charData != null)
            {
                isAvailableToView = evt.PlayerLevel >= charData.RequiredHideoutLevel;
            }

            if (mapping.UnlockedGameObject != null)
                mapping.UnlockedGameObject.SetActive(isAvailableToView);
            
            if (mapping.LockedGameObject != null)
                mapping.LockedGameObject.SetActive(!isAvailableToView);
        }

        // If we are currently viewing details, refresh the button states (e.g. it just unlocked)
        if (_characterDetailsPanel != null && _characterDetailsPanel.activeSelf)
        {
            RefreshDetailsView();
        }
    }
}
