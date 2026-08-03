using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VContainer;
using VaultSurvivors.Core.Production;

public class FactoryUIController : MonoBehaviour
{
    private IEventPublisher _eventPublisher;
    private IEventSubscriber _eventSubscriber;

    [Header("Main Panel")]
    [SerializeField] private GameObject _panelContainer;
    [SerializeField] private Button _closeBtn;

    [Header("HUD Buttons To Hide")]
    [SerializeField] private GameObject _hudButton1;
    [SerializeField] private GameObject _hudButton2;

    [Header("Active Production Info")]
    [SerializeField] private TMP_Text _factoryLevelLabel;
    [SerializeField] private TMP_Text _activeResourceNameLabel;
    [SerializeField] private TMP_Text _rateLabel;
    [SerializeField] private TMP_Text _capacityLabel;
    [SerializeField] private TMP_Text _accumulatedLabel;

    [Header("Action Buttons")]
    [SerializeField] private Button _collectBtn;
    [SerializeField] private Button _upgradeBtn;
    [SerializeField] private TMP_Text _collectBtnText;
    [SerializeField] private TMP_Text _upgradeBtnText;

    [Header("Switch Buttons (Phase 3)")]
    [SerializeField] private Button _switchEnergyBtn;
    [SerializeField] private Button _switchFoodBtn;
    [SerializeField] private Button _switchElectronicsBtn;

    [Header("Switch Icons To Dim")]
    [SerializeField] private Image _energyIconImg;
    [SerializeField] private Image _foodIconImg;
    [SerializeField] private Image _electronicsIconImg;

    [Header("Switch Popup Modal (Phase 3)")]
    [SerializeField] private GameObject _switchPopup;
    [SerializeField] private TMP_Text _switchPopupText;
    [SerializeField] private Button _confirmSwitchBtn;
    [SerializeField] private Button _cancelSwitchBtn;

    private ResourceType _currentActiveResource = ResourceType.Energy;
    private ResourceType _pendingSwitchResource;
    private int _pendingCollectAmount;

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
        if (_switchPopup != null) _switchPopup.SetActive(false);
    }

    private void OnEnable()
    {
        if (_collectBtn != null) _collectBtn.onClick.AddListener(OnCollectClicked);
        if (_upgradeBtn != null) _upgradeBtn.onClick.AddListener(OnUpgradeClicked);
        if (_closeBtn != null) _closeBtn.onClick.AddListener(CloseUI);

        if (_switchEnergyBtn != null) _switchEnergyBtn.onClick.AddListener(() => ShowSwitchPopup(ResourceType.Energy));
        if (_switchFoodBtn != null) _switchFoodBtn.onClick.AddListener(() => ShowSwitchPopup(ResourceType.Food));
        if (_switchElectronicsBtn != null) _switchElectronicsBtn.onClick.AddListener(() => ShowSwitchPopup(ResourceType.Electronics));

        if (_confirmSwitchBtn != null) _confirmSwitchBtn.onClick.AddListener(OnConfirmSwitch);
        if (_cancelSwitchBtn != null) _cancelSwitchBtn.onClick.AddListener(HideSwitchPopup);
    }

    private void OnDisable()
    {
        if (_collectBtn != null) _collectBtn.onClick.RemoveListener(OnCollectClicked);
        if (_upgradeBtn != null) _upgradeBtn.onClick.RemoveListener(OnUpgradeClicked);
        if (_closeBtn != null) _closeBtn.onClick.RemoveListener(CloseUI);

        if (_switchEnergyBtn != null) _switchEnergyBtn.onClick.RemoveAllListeners();
        if (_switchFoodBtn != null) _switchFoodBtn.onClick.RemoveAllListeners();
        if (_switchElectronicsBtn != null) _switchElectronicsBtn.onClick.RemoveAllListeners();

        if (_confirmSwitchBtn != null) _confirmSwitchBtn.onClick.RemoveListener(OnConfirmSwitch);
        if (_cancelSwitchBtn != null) _cancelSwitchBtn.onClick.RemoveListener(HideSwitchPopup);
    }

    private void Start()
    {
        _eventSubscriber?.Subscribe<Event_FactoryStateUpdated>(OnStateUpdated);
        _eventSubscriber?.Subscribe<Command_OpenFactoryUI>(OnOpenUI);
        _eventSubscriber?.Subscribe<Command_CloseFactoryUI>(OnCloseUI);
        _eventSubscriber?.Subscribe<Command_CloseAllMenus>(OnCloseAllMenus);
    }

    private void OnDestroy()
    {
        _eventSubscriber?.Unsubscribe<Event_FactoryStateUpdated>(OnStateUpdated);
        _eventSubscriber?.Unsubscribe<Command_OpenFactoryUI>(OnOpenUI);
        _eventSubscriber?.Unsubscribe<Command_CloseFactoryUI>(OnCloseUI);
        _eventSubscriber?.Unsubscribe<Command_CloseAllMenus>(OnCloseAllMenus);
    }

    private void OnCloseAllMenus(Command_CloseAllMenus cmd)
    {
        OnCloseUI(new Command_CloseFactoryUI());
    }

    private void OnOpenUI(Command_OpenFactoryUI cmd)
    {
        if (_panelContainer != null && !_panelContainer.activeSelf)
        {
            _eventPublisher?.Publish(new Command_CloseAllMenus());
            _panelContainer.SetActive(true);
            _eventPublisher?.Publish(new Event_MenuStateChanged { isOpen = true });

            if (_hudButton1 != null) _hudButton1.SetActive(false);
            if (_hudButton2 != null) _hudButton2.SetActive(false);

            _eventPublisher?.Publish(new Command_RequestFactoryState());
        }
    }

    private void OnCloseUI(Command_CloseFactoryUI cmd)
    {
        if (_panelContainer != null && _panelContainer.activeSelf)
        {
            _panelContainer.SetActive(false);
            _eventPublisher?.Publish(new Event_MenuStateChanged { isOpen = false });

            if (_hudButton1 != null) _hudButton1.SetActive(true);
            if (_hudButton2 != null) _hudButton2.SetActive(true);
        }
    }

    private void CloseUI()
    {
        OnCloseUI(null);
    }

    private void OnStateUpdated(Event_FactoryStateUpdated evt)
    {
        _currentActiveResource = evt.ActiveResource;

        if (_factoryLevelLabel != null)
            _factoryLevelLabel.text = $"LV {evt.GlobalLevel}";

        if (_activeResourceNameLabel != null)
            _activeResourceNameLabel.text = $"{evt.ActiveResource} Production";

        if (_rateLabel != null)
            _rateLabel.text = $"Rate: {evt.ProductionRate:F1}/s";

        if (_capacityLabel != null)
            _capacityLabel.text = $"Max Capacity: {Mathf.FloorToInt(evt.MaxCapacity)}";

        int amount = Mathf.FloorToInt(evt.CurrentAccumulation);
        _pendingCollectAmount = amount;

        if (_accumulatedLabel != null)
            _accumulatedLabel.text = $"{amount} Collected";

        if (_collectBtn != null)
        {
            if (_collectBtnText != null) _collectBtnText.text = $"Collect";
            _collectBtn.interactable = amount > 0;
        }

        if (_upgradeBtn != null)
        {
            if (_upgradeBtnText != null) _upgradeBtnText.text = $"Upgrade   [{evt.UpgradeCost}]";
            _upgradeBtn.interactable = evt.WalletGold >= evt.UpgradeCost;
        }

        Color dimmedColor = new Color(0.4f, 0.4f, 0.4f, 1f);

        if (_energyIconImg != null)
            _energyIconImg.color = (evt.ActiveResource == ResourceType.Energy) ? dimmedColor : Color.white;
        if (_foodIconImg != null)
            _foodIconImg.color = (evt.ActiveResource == ResourceType.Food) ? dimmedColor : Color.white;
        if (_electronicsIconImg != null)
            _electronicsIconImg.color = (evt.ActiveResource == ResourceType.Electronics) ? dimmedColor : Color.white;

        if (_switchEnergyBtn != null)
            _switchEnergyBtn.interactable = (evt.ActiveResource != ResourceType.Energy);
        if (_switchFoodBtn != null)
            _switchFoodBtn.interactable = (evt.ActiveResource != ResourceType.Food);
        if (_switchElectronicsBtn != null)
            _switchElectronicsBtn.interactable = (evt.ActiveResource != ResourceType.Electronics);
    }

    // Phase 3 Modal Logic
    private void ShowSwitchPopup(ResourceType target)
    {
        if (_switchPopup == null) return;

        _pendingSwitchResource = target;
        if (_switchPopupText != null)
        {
            _switchPopupText.text = $"Collect {_pendingCollectAmount} {_currentActiveResource} and switch production to {target}?";
        }
        _switchPopup.SetActive(true);
    }

    private void HideSwitchPopup()
    {
        if (_switchPopup != null) _switchPopup.SetActive(false);
    }

    private void OnConfirmSwitch()
    {
        _eventPublisher?.Publish(new Command_CollectFactory());
        _eventPublisher?.Publish(new Command_SetProductionResource(_pendingSwitchResource));
        HideSwitchPopup();
    }

    private void OnCollectClicked()
    {
        _eventPublisher?.Publish(new Command_CollectFactory());
    }

    private void OnUpgradeClicked()
    {
        _eventPublisher?.Publish(new Command_UpgradeFactory());
    }
}
