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

    [Header("Text Fields")]
    [SerializeField] private TMP_Text _headerLabel; // Equivalent to global factory title
    [SerializeField] private TMP_Text _rateLabel;
    [SerializeField] private TMP_Text _capacityLabel;

    [Header("Resource Amounts")]
    [SerializeField] private TMP_Text _goldAmount;
    [SerializeField] private TMP_Text _foodAmount;
    [SerializeField] private TMP_Text _energyAmount;
    [SerializeField] private TMP_Text _electronicsAmount;

    [Header("Buttons")]
    [SerializeField] private Button _collectBtn;
    [SerializeField] private Button _upgradeBtn;
    [SerializeField] private Button _closeBtn;

    [Header("Resource Selection (Optional UGUI Buttons)")]
    [SerializeField] private Button _selectEnergyBtn;
    [SerializeField] private Button _selectFoodBtn;
    [SerializeField] private Button _selectElectronicsBtn;
    [SerializeField] private Button _selectGoldBtn;

    private ResourceType _currentActiveResource = ResourceType.Energy;
    private bool _isUpdatingFromEvent = false;

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
            _panelContainer.SetActive(false); // Hide initially
        }
    }

    private void OnEnable()
    {
        if (_collectBtn != null) _collectBtn.onClick.AddListener(OnCollectClicked);
        if (_upgradeBtn != null) _upgradeBtn.onClick.AddListener(OnUpgradeClicked);
        if (_closeBtn != null) _closeBtn.onClick.AddListener(CloseUI);

        if (_selectEnergyBtn != null) _selectEnergyBtn.onClick.AddListener(() => OnResourceSelected(ResourceType.Energy));
        if (_selectFoodBtn != null) _selectFoodBtn.onClick.AddListener(() => OnResourceSelected(ResourceType.Food));
        if (_selectElectronicsBtn != null) _selectElectronicsBtn.onClick.AddListener(() => OnResourceSelected(ResourceType.Electronics));
        if (_selectGoldBtn != null) _selectGoldBtn.onClick.AddListener(() => OnResourceSelected(ResourceType.Gold));
    }

    private void OnDisable()
    {
        if (_collectBtn != null) _collectBtn.onClick.RemoveListener(OnCollectClicked);
        if (_upgradeBtn != null) _upgradeBtn.onClick.RemoveListener(OnUpgradeClicked);
        if (_closeBtn != null) _closeBtn.onClick.RemoveListener(CloseUI);

        if (_selectEnergyBtn != null) _selectEnergyBtn.onClick.RemoveAllListeners();
        if (_selectFoodBtn != null) _selectFoodBtn.onClick.RemoveAllListeners();
        if (_selectElectronicsBtn != null) _selectElectronicsBtn.onClick.RemoveAllListeners();
        if (_selectGoldBtn != null) _selectGoldBtn.onClick.RemoveAllListeners();
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
        Debug.Log("[FactoryUIController] Received Command_OpenFactoryUI.");
        if (_panelContainer != null && !_panelContainer.activeSelf)
        {
            _eventPublisher?.Publish(new Command_CloseAllMenus());
            _panelContainer.SetActive(true);
            _eventPublisher?.Publish(new Event_MenuStateChanged { isOpen = true });
        }
    }

    private void OnCloseUI(Command_CloseFactoryUI cmd)
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

    private void OnStateUpdated(Event_FactoryStateUpdated evt)
    {
        _isUpdatingFromEvent = true;

        if (_headerLabel != null)
            _headerLabel.text = $"GLOBAL FACTORY (LEVEL {evt.GlobalLevel})";

        if (_rateLabel != null)
            _rateLabel.text = $"Production Rate: {evt.ProductionRate:F1}/s";

        if (_capacityLabel != null)
            _capacityLabel.text = $"Max Storage: {Mathf.FloorToInt(evt.MaxCapacity)}";

        if (_goldAmount != null) _goldAmount.text = Mathf.FloorToInt(evt.WalletGold).ToString();
        if (_foodAmount != null) _foodAmount.text = Mathf.FloorToInt(evt.WalletFood).ToString();
        if (_energyAmount != null) _energyAmount.text = Mathf.FloorToInt(evt.WalletEnergy).ToString();
        if (_electronicsAmount != null) _electronicsAmount.text = Mathf.FloorToInt(evt.WalletElectronics).ToString();

        if (_collectBtn != null)
        {
            int amount = Mathf.FloorToInt(evt.CurrentAccumulation);
            var collectText = _collectBtn.GetComponentInChildren<TMP_Text>();
            if (collectText != null) collectText.text = $"Collect ({amount} {evt.ActiveResource})";
            _collectBtn.interactable = amount > 0;
        }

        if (_upgradeBtn != null)
        {
            var upgradeText = _upgradeBtn.GetComponentInChildren<TMP_Text>();
            if (upgradeText != null) upgradeText.text = $"Upgrade Factory (Cost: {evt.UpgradeCost} Gold)";
            _upgradeBtn.interactable = evt.WalletGold >= evt.UpgradeCost;
        }

        _currentActiveResource = evt.ActiveResource;
        
        // Visual indication for the active resource can be added here
        // E.g., modifying button colors based on _currentActiveResource

        _isUpdatingFromEvent = false;
    }

    private void OnResourceSelected(ResourceType resourceType)
    {
        if (_isUpdatingFromEvent) return;
        
        _eventPublisher?.Publish(new Command_SetProductionResource(resourceType));
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
