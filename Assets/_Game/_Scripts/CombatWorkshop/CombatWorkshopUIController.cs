using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VContainer;
using System.Collections;

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
}

public class CombatWorkshopUIController : MonoBehaviour
{
    private IEventPublisher _eventPublisher;
    private IEventSubscriber _eventSubscriber;
    private ISaveSystem _saveSystem;

    [Header("Main Canvas")]
    [SerializeField] private GameObject _panelContainer;
    [SerializeField] private Button _backBtn;

    [Header("Data")]
    [SerializeField] private SupportCharacterRegistrySO _registry;

    [Header("Phase 1: Character Selection")]
    [SerializeField] private GameObject _characterSelectionPanel;
    [Tooltip("Parent container for all unlockable character cards (must always be active).")]
    [SerializeField] private GameObject _unlockedHerosContainer;
    [Tooltip("Parent container for all locked character cards (must always be active).")]
    [SerializeField] private GameObject _lockedHerosContainer;
    [SerializeField] private TextMeshProUGUI _popupText;
    [Tooltip("Map each character ID to its specific hand-made GameObjects in your scene")]
    [SerializeField] private List<CharacterUIMapping> _characterMappings = new List<CharacterUIMapping>();
    
    [Header("Phase 2: Character Unlock / Details")]
    [SerializeField] private GameObject _characterDetailsPanel;
    [SerializeField] private GameObject _CharacterArtContainer;
    [SerializeField] private GameObject _DescriptionContainer;
    [SerializeField] private GameObject _MaterialsRequiredContainer;
    [SerializeField] private GameObject _MaterialTitleContainer;

    [Header("Character Description")]
    [SerializeField] private TextMeshProUGUI _txtAbilityTitle;
    [SerializeField] private TextMeshProUGUI _txtCharacterDescription;
    [SerializeField] private TextMeshProUGUI _txtCharacterStats;
    
    [SerializeField] private TMP_Text _detailCost;
    [SerializeField] private Button _unlockBtn;

    private Event_WorkshopStateUpdated _lastState;
    private SupportCharacterSO _currentlyViewedCharacter;
    private Dictionary<string, SupportCharacterSO> _characterLookup;

    [Inject]
    public void Construct(IEventPublisher eventPublisher, IEventSubscriber eventSubscriber, ISaveSystem saveSystem)
    {
        _eventPublisher = eventPublisher;
        _eventSubscriber = eventSubscriber;
        _saveSystem = saveSystem;
    }

    private void Awake()
    {
        if (_panelContainer != null) _panelContainer.SetActive(false);        
        if (_popupText != null) _popupText.gameObject.SetActive(false);
        if (_backBtn != null) _backBtn.gameObject.SetActive(true);
        if (_CharacterArtContainer != null) _CharacterArtContainer.SetActive(true);
        if (_DescriptionContainer != null) _DescriptionContainer.SetActive(true);
        if (_MaterialsRequiredContainer != null) _MaterialsRequiredContainer.SetActive(true);
        if (_MaterialTitleContainer != null) _MaterialTitleContainer.SetActive(true);


        _characterLookup = new Dictionary<string, SupportCharacterSO>();

        foreach (var character in _registry.Characters)
        {
            _characterLookup[character.CharacterId] = character;
        }
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
        _eventSubscriber?.Subscribe<Event_ShowWorkshopPopup>(OnShowWorkshopPopup);
        
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
                    btn.onClick.AddListener(() => ShowPopup("You haven't reached the required Level yet!"));
                }
            }

            // Set the level text immediately from the SO — no need to wait for state event
            if (_registry != null)
            {
                if(_characterLookup.TryGetValue(mapping.CharacterId, out SupportCharacterSO charData))
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
        _eventSubscriber?.Unsubscribe<Event_ShowWorkshopPopup>(OnShowWorkshopPopup);
    }

    private void OnShowWorkshopPopup(Event_ShowWorkshopPopup evt)
    {
        ShowPopup(evt.Message);
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
        _characterLookup.TryGetValue(characterId, out _currentlyViewedCharacter);
        if (_currentlyViewedCharacter == null) return;

        if (_characterSelectionPanel != null) _characterSelectionPanel.SetActive(false);
        if (_characterDetailsPanel != null) _characterDetailsPanel.SetActive(true);

        // Turn on the correct Art GameObjects
        foreach (var mapping in _characterMappings)
        {
            bool isCurrent = (mapping.CharacterId == characterId);
            
            if (mapping.DetailArtGameObject != null) 
                mapping.DetailArtGameObject.SetActive(isCurrent);
        }

        RefreshDetailsView();
    }

    private void RefreshDetailsView()
    {
        if (_currentlyViewedCharacter == null || _lastState == null) return;

        bool isUnlocked = _lastState.UnlockedIds.Contains(_currentlyViewedCharacter.CharacterId);
        bool canAfford = true;

        if (_txtAbilityTitle != null) _txtAbilityTitle.text = _currentlyViewedCharacter.AbilityTitle;
        if (_txtCharacterDescription != null) _txtCharacterDescription.text = _currentlyViewedCharacter.Description;
        if (_txtCharacterStats != null) _txtCharacterStats.text = _currentlyViewedCharacter.CharacterStats;
        
        if (_detailCost != null)
        {
            if (isUnlocked)
            {
                _detailCost.text = "Unlocked";
            }
            else if (_currentlyViewedCharacter.UnlockCost != null && _currentlyViewedCharacter.UnlockCost.Count > 0)
            {
                GameData data = _saveSystem != null && _saveSystem.HasSaveFile() ? _saveSystem.Load() : new GameData();
                
                _detailCost.text = string.Join("\n",
                    _currentlyViewedCharacter.UnlockCost.Select(cost =>
                    {
                        float currentAmount = 0f;
                        switch (cost.Type)
                        {
                            case VaultSurvivors.Core.Production.ResourceType.Food: currentAmount = data.resources.food; break;
                            case VaultSurvivors.Core.Production.ResourceType.Energy: currentAmount = data.resources.energy; break;
                            case VaultSurvivors.Core.Production.ResourceType.Electronics: currentAmount = data.resources.electronics; break;
                            case VaultSurvivors.Core.Production.ResourceType.Gold: currentAmount = data.resources.gold; break;
                        }
                        if (currentAmount < cost.Amount) canAfford = false;
                        return $"{cost.Amount} {cost.ResourceName}";
                    }));
            }
            else
            {
                _detailCost.text = "Free to Unlock";
            }
        }

        if (_unlockBtn != null) 
        {
            bool interactable = !isUnlocked && canAfford;
            _unlockBtn.interactable = interactable;
            
            Color dimmedColor = new Color32(102, 102, 102, 255);
            Color activeGreen = new Color32(13, 249, 108, 255);
            Color finalColor = interactable ? activeGreen : dimmedColor;

            Image btnImage = _unlockBtn.GetComponent<Image>();
            if (btnImage != null)
                btnImage.color = finalColor;

            TMP_Text btnText = _unlockBtn.GetComponentInChildren<TMP_Text>();
            if (btnText != null)
                btnText.color = finalColor;

            // Only show the button if it's NOT unlocked yet.
            _unlockBtn.gameObject.SetActive(!isUnlocked);
        }
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

        // Guarantee the two parent containers are always on — only the individual cards inside toggle.
        if (_unlockedHerosContainer != null) _unlockedHerosContainer.SetActive(true);
        if (_lockedHerosContainer   != null) _lockedHerosContainer.SetActive(true);

        // Toggle individual cards based on player level vs required level.
        // UnlockedGameObject = player meets level requirement (can view/interact with this character)
        // LockedGameObject   = player is too low level to even see this character yet
        foreach (var mapping in _characterMappings)
        {
            bool meetsLevelReq = true; // default: always show unless a level requirement blocks it
            
            if (_characterLookup.TryGetValue(mapping.CharacterId, out SupportCharacterSO charData))
            {
                meetsLevelReq = evt.PlayerLevel >= charData.RequiredHideoutLevel;
            }

            if (mapping.UnlockedGameObject != null)
                mapping.UnlockedGameObject.SetActive(meetsLevelReq);

            if (mapping.LockedGameObject != null)
                mapping.LockedGameObject.SetActive(!meetsLevelReq);
        }



        // If we are currently viewing details, refresh the button states (e.g. it just unlocked)
        if (_characterDetailsPanel != null && _characterDetailsPanel.activeSelf)
        {
            RefreshDetailsView();
        }
    }

    private Coroutine _popupCoroutine;

    public void ShowPopup(string message)
    {
        if (_popupText == null) return;

        if (_popupCoroutine != null) StopCoroutine(_popupCoroutine);

        _popupText.text = message;
        
        _popupText.gameObject.SetActive(true);
        
        _popupCoroutine = StartCoroutine(HidePopupRoutine());
    }

    private IEnumerator HidePopupRoutine()
    {
        yield return new WaitForSeconds(2.5f);
        
        if (_popupText != null) _popupText.gameObject.SetActive(false);
    }
}
