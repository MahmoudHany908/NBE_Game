using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VaultSurvivors.Core.Production;
using VaultSurvivors.LoadingSystem;
using VContainer;

[Serializable]
public struct RosterUIMapping
{
    public string CharacterId;
    public Button CharacterButton;
    public GameObject LockIcon;
    public GameObject CharacterArt;
}

public class GateLoadoutUIController : MonoBehaviour
{
    [Header("Core References")]
    [SerializeField] private SupportCharacterRegistrySO _registry;
    [SerializeField] private GameObject _panelContainer;
    
    [Header("Phase 1: Character Selection")]
    [SerializeField] private GameObject _characterSelectionPanel;
    [SerializeField] private Button _emptyCard1Btn;
    [SerializeField] private Image _emptyCard1Image;
    [SerializeField] private Button _emptyCard2Btn;
    [SerializeField] private Image _emptyCard2Image;
    [SerializeField] private Button _arenaBtn;
    [SerializeField] private TextMeshProUGUI _popupText; 
    [SerializeField] private GameObject _popupPanel; 

    [Header("Phase 2: Roster Selection")]
    [SerializeField] private GameObject _characterRosterPanel;
    [SerializeField] private Button _backBtn;
    [SerializeField] private Button _confirmBtn;
    [SerializeField] private List<RosterUIMapping> _rosterMappings;

    [Header("Roster Details Text")]
    [SerializeField] private TextMeshProUGUI _txtAbilityTitle;
    [SerializeField] private TextMeshProUGUI _txtCharacterDescription;
    [SerializeField] private TextMeshProUGUI _txtCharacterStats;

    private IEventPublisher _eventPublisher;
    private IEventSubscriber _eventSubscriber;

    private Event_WorkshopStateUpdated _lastState;
    private int _currentEditingSlot = 0;
    private SupportCharacterSO _currentlyViewedCharacter;
    
    // Local copy of loadout while editing
    private List<string> _currentLoadout = new List<string> { null, null };

    [Inject]
    public void Construct(IEventPublisher eventPublisher, IEventSubscriber eventSubscriber)
    {
        Debug.Log("[GateLoadoutUIController] Construct called! Dependencies injected.");
        _eventPublisher = eventPublisher;
        _eventSubscriber = eventSubscriber;
    }

    private void Awake()
    {
        if (_panelContainer != null) _panelContainer.SetActive(false);
        if (_popupPanel != null) _popupPanel.SetActive(false);
    }

    private void OnEnable()
    {
        if (_emptyCard1Btn != null) _emptyCard1Btn.onClick.AddListener(() => OnEmptyCardClicked(0));
        if (_emptyCard2Btn != null) _emptyCard2Btn.onClick.AddListener(() => OnEmptyCardClicked(1));
        if (_arenaBtn != null) _arenaBtn.onClick.AddListener(OnArenaClicked);
        if (_backBtn != null) _backBtn.onClick.AddListener(OnBackClicked);
        if (_confirmBtn != null) _confirmBtn.onClick.AddListener(OnConfirmClicked);
    }

    private void OnDisable()
    {
        if (_emptyCard1Btn != null) _emptyCard1Btn.onClick.RemoveAllListeners();
        if (_emptyCard2Btn != null) _emptyCard2Btn.onClick.RemoveAllListeners();
        if (_arenaBtn != null) _arenaBtn.onClick.RemoveListener(OnArenaClicked);
        if (_backBtn != null) _backBtn.onClick.RemoveListener(OnBackClicked);
        if (_confirmBtn != null) _confirmBtn.onClick.RemoveListener(OnConfirmClicked);
    }

    private void Start()
    {
        Debug.Log($"[GateLoadoutUIController] Start called! _eventSubscriber is null? {_eventSubscriber == null}");
        
        _eventSubscriber?.Subscribe<Event_WorkshopStateUpdated>(OnStateUpdated);
        _eventSubscriber?.Subscribe<Command_OpenGateLoadoutUI>(OnOpenUI);
        _eventSubscriber?.Subscribe<Command_CloseGateLoadoutUI>(OnCloseUI);
        _eventSubscriber?.Subscribe<Command_CloseAllMenus>(OnCloseAllMenus);

        // Setup character roster buttons
        foreach (var mapping in _rosterMappings)
        {
            if (mapping.CharacterButton != null)
            {
                mapping.CharacterButton.onClick.RemoveAllListeners();
                string charId = mapping.CharacterId;
                mapping.CharacterButton.onClick.AddListener(() => SelectRosterCharacter(charId));
            }
        }
    }

    private void OnDestroy()
    {
        _eventSubscriber?.Unsubscribe<Event_WorkshopStateUpdated>(OnStateUpdated);
        _eventSubscriber?.Unsubscribe<Command_OpenGateLoadoutUI>(OnOpenUI);
        _eventSubscriber?.Unsubscribe<Command_CloseGateLoadoutUI>(OnCloseUI);
        _eventSubscriber?.Unsubscribe<Command_CloseAllMenus>(OnCloseAllMenus);
    }

    private void OnStateUpdated(Event_WorkshopStateUpdated evt)
    {
        _lastState = evt;
        
        // Sync local loadout with actual equipped state
        _currentLoadout = new List<string> { null, null };
        for (int i = 0; i < evt.EquippedIds.Count && i < 2; i++)
        {
            _currentLoadout[i] = evt.EquippedIds[i];
        }

        RefreshEmptyCards();

        // If we are currently in phase 2, refresh the roster buttons
        if (_characterRosterPanel != null && _characterRosterPanel.activeSelf)
        {
            RefreshRosterView();
        }
    }

    private void OnOpenUI(Command_OpenGateLoadoutUI cmd)
    {
        Debug.Log($"[GateLoadoutUIController] OnOpenUI received! PanelContainer assigned? {_panelContainer != null}");
        
        if (_panelContainer != null && !_panelContainer.activeSelf)
        {
            _eventPublisher?.Publish(new Command_CloseAllMenus());
            _panelContainer.SetActive(true);
            _eventPublisher?.Publish(new Event_MenuStateChanged { isOpen = true });

            // Clear the equipped loadout so it starts fresh every time
            _eventPublisher?.Publish(new Command_SetLoadout(new List<string>()));

            OpenSelectionPhase();

            // Ask for immediate data
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

    private void OnCloseAllMenus(Command_CloseAllMenus cmd)
    {
        if (_panelContainer != null && _panelContainer.activeSelf)
        {
            OnBackClicked();
        }
    }

    public void OnBackClicked()
    {
        if (_characterRosterPanel != null && _characterRosterPanel.activeSelf)
        {
            OpenSelectionPhase();
        }
        else
        {
            OnCloseUI(null);
        }
    }

    private void OpenSelectionPhase()
    {
        if (_characterSelectionPanel != null) _characterSelectionPanel.SetActive(true);
        if (_characterRosterPanel != null) _characterRosterPanel.SetActive(false);
        RefreshEmptyCards();
    }

    private void RefreshEmptyCards()
    {
        if (_emptyCard1Image != null)
        {
            var char1 = _registry?.Characters.Find(c => c.CharacterId == _currentLoadout[0]);
            if (char1 != null && char1.CharacterPortrait != null)
            {
                _emptyCard1Image.sprite = char1.CharacterPortrait;
                _emptyCard1Image.color = Color.white;
            }
            else
            {
                _emptyCard1Image.sprite = null;
                _emptyCard1Image.color = new Color(1, 1, 1, 0); // Hide if empty
            }
        }

        if (_emptyCard2Image != null)
        {
            var char2 = _registry?.Characters.Find(c => c.CharacterId == _currentLoadout[1]);
            if (char2 != null && char2.CharacterPortrait != null)
            {
                _emptyCard2Image.sprite = char2.CharacterPortrait;
                _emptyCard2Image.color = Color.white;
            }
            else
            {
                _emptyCard2Image.sprite = null;
                _emptyCard2Image.color = new Color(1, 1, 1, 0); // Hide if empty
            }
        }
    }

    private void OnEmptyCardClicked(int slotIndex)
    {
        if (_lastState == null) return;
        
        int unlockedCount = _lastState.UnlockedIds.Count;
        
        if (unlockedCount == 0)
        {
            ShowPopup("You haven't unlocked any hero yet");
            return;
        }

        if (slotIndex == 1 && unlockedCount < 2)
        {
            ShowPopup("Only one character is unlocked");
            return;
        }

        _currentEditingSlot = slotIndex;
        OpenRosterPhase();
    }

    private void OpenRosterPhase()
    {
        if (_characterSelectionPanel != null) _characterSelectionPanel.SetActive(false);
        if (_characterRosterPanel != null) _characterRosterPanel.SetActive(true);

        _currentlyViewedCharacter = null;
        
        // Hide all character arts by default
        foreach (var mapping in _rosterMappings)
        {
            if (mapping.CharacterArt != null) mapping.CharacterArt.SetActive(false);
        }

        RefreshRosterView();

        // Optional: auto-select the current character in this slot if any
        if (_currentEditingSlot >= 0 && _currentEditingSlot < 2 && !string.IsNullOrEmpty(_currentLoadout[_currentEditingSlot]))
        {
            SelectRosterCharacter(_currentLoadout[_currentEditingSlot]);
        }
        else
        {
            // Clear texts
            if (_txtAbilityTitle != null) _txtAbilityTitle.text = "";
            if (_txtCharacterDescription != null) _txtCharacterDescription.text = "";
            if (_txtCharacterStats != null) _txtCharacterStats.text = "";
        }
    }

    private void RefreshRosterView()
    {
        if (_lastState == null) return;

        foreach (var mapping in _rosterMappings)
        {
            bool isUnlocked = _lastState.UnlockedIds.Contains(mapping.CharacterId);
            
            if (mapping.CharacterButton != null)
            {
                mapping.CharacterButton.interactable = isUnlocked; // Prevents clicking if locked
                
                var image = mapping.CharacterButton.GetComponent<Image>();
                if (image != null)
                {
                    // 50% opacity if locked, full opacity if unlocked
                    var color = image.color;
                    color.a = isUnlocked ? 1f : 0.5f;
                    image.color = color;
                }
            }

            if (mapping.LockIcon != null)
            {
                mapping.LockIcon.SetActive(!isUnlocked);
            }
        }
    }

    private void SelectRosterCharacter(string characterId)
    {
        if (_lastState == null || !_lastState.UnlockedIds.Contains(characterId)) return; // Don't allow selecting locked

        _currentlyViewedCharacter = _registry?.Characters.Find(c => c.CharacterId == characterId);
        
        if (_currentlyViewedCharacter != null)
        {
            if (_txtAbilityTitle != null) _txtAbilityTitle.text = _currentlyViewedCharacter.AbilityTitle;
            if (_txtCharacterDescription != null) _txtCharacterDescription.text = _currentlyViewedCharacter.Description;
            if (_txtCharacterStats != null) _txtCharacterStats.text = _currentlyViewedCharacter.CharacterStats;
        }

        foreach (var mapping in _rosterMappings)
        {
            if (mapping.CharacterArt != null)
            {
                mapping.CharacterArt.SetActive(mapping.CharacterId == characterId);
            }
        }
    }

    private void OnConfirmClicked()
    {
        if (_currentlyViewedCharacter != null)
        {
            _currentLoadout[_currentEditingSlot] = _currentlyViewedCharacter.CharacterId;
            
            // Clean up: if the same character is in the other slot, remove it from the other slot
            int otherSlot = _currentEditingSlot == 0 ? 1 : 0;
            if (_currentLoadout[otherSlot] == _currentlyViewedCharacter.CharacterId)
            {
                _currentLoadout[otherSlot] = null;
            }

            // Publish the updated loadout to CombatWorkshopManager
            var compactLoadout = _currentLoadout.Where(id => !string.IsNullOrEmpty(id)).ToList();
            _eventPublisher?.Publish(new Command_SetLoadout(compactLoadout));
        }

        OpenSelectionPhase();
    }

    private void OnArenaClicked()
    {
        SceneLoader.Load("ArenaScene", new ArenaState());
        Debug.Log("[GateLoadoutUIController] SceneLoader requested transition to ArenaScene.");
    }

    private Coroutine _popupCoroutine;

    private void ShowPopup(string message)
    {
        if (_popupText == null) return;

        if (_popupCoroutine != null) StopCoroutine(_popupCoroutine);

        _popupText.text = message;
        
        if (_popupPanel != null) _popupPanel.SetActive(true);
        else _popupText.gameObject.SetActive(true);
        
        _popupCoroutine = StartCoroutine(HidePopupRoutine());
    }

    private IEnumerator HidePopupRoutine()
    {
        yield return new WaitForSeconds(2.5f);
        
        if (_popupPanel != null) _popupPanel.SetActive(false);
        else if (_popupText != null) _popupText.gameObject.SetActive(false);
    }
}
