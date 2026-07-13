using UnityEngine;
using UnityEngine.UIElements;
using VContainer;
using VaultSurvivors.Core.Production;

[RequireComponent(typeof(UIDocument))]
public class FactoryUIController : MonoBehaviour
{
    private IEventPublisher _eventPublisher;
    private IEventSubscriber _eventSubscriber;

    private UIDocument _uiDocument;
    private VisualElement _root;
    private VisualElement _panelContainer;

    private Label _headerLabel;
    private Label _rateLabel;
    private Label _capacityLabel;
    private Label _walletLabel;

    private RadioButton _radioEnergy;
    private RadioButton _radioFood;
    private RadioButton _radioElectronics;
    private RadioButton _radioGold;

    private Button _collectBtn;
    private Button _upgradeBtn;
    private Button _closeBtn;

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
        _uiDocument = GetComponent<UIDocument>();
        if (_uiDocument == null) return;

        _root = _uiDocument.rootVisualElement;
        
        _panelContainer = _root.Q<VisualElement>(className: "factory-panel-container");
        if (_panelContainer != null)
        {
            _panelContainer.style.display = DisplayStyle.None; // Hide initially
            Debug.Log("[FactoryUIController] Found _panelContainer and hid it initially.");
        }
        else
        {
            Debug.LogError("[FactoryUIController] Could not find VisualElement with class 'factory-panel-container'!");
        }

        _headerLabel = _root.Q<Label>("header-label");
        _rateLabel = _root.Q<Label>("rate-label");
        _capacityLabel = _root.Q<Label>("capacity-label");
        _walletLabel = _root.Q<Label>("wallet-label");

        _radioEnergy = _root.Q<RadioButton>("radio-energy");
        _radioFood = _root.Q<RadioButton>("radio-food");
        _radioElectronics = _root.Q<RadioButton>("radio-electronics");
        _radioGold = _root.Q<RadioButton>("radio-gold");

        _collectBtn = _root.Q<Button>("collect-btn");
        _upgradeBtn = _root.Q<Button>("upgrade-btn");
        _closeBtn = _root.Q<Button>("close-btn");

        _radioEnergy?.RegisterValueChangedCallback(evt => OnResourceRadioChanged(evt, ResourceType.Energy));
        _radioFood?.RegisterValueChangedCallback(evt => OnResourceRadioChanged(evt, ResourceType.Food));
        _radioElectronics?.RegisterValueChangedCallback(evt => OnResourceRadioChanged(evt, ResourceType.Electronics));
        _radioGold?.RegisterValueChangedCallback(evt => OnResourceRadioChanged(evt, ResourceType.Gold));

        if (_collectBtn != null) _collectBtn.clicked += OnCollectClicked;
        if (_upgradeBtn != null) _upgradeBtn.clicked += OnUpgradeClicked;
        if (_closeBtn != null) _closeBtn.clicked += CloseUI;
    }

    private void Start()
    {
        _eventSubscriber?.Subscribe<Event_FactoryStateUpdated>(OnStateUpdated);
        _eventSubscriber?.Subscribe<Command_OpenFactoryUI>(OnOpenUI);
        _eventSubscriber?.Subscribe<Command_CloseFactoryUI>(OnCloseUI);
    }

    private void OnDestroy()
    {
        _eventSubscriber?.Unsubscribe<Event_FactoryStateUpdated>(OnStateUpdated);
        _eventSubscriber?.Unsubscribe<Command_OpenFactoryUI>(OnOpenUI);
        _eventSubscriber?.Unsubscribe<Command_CloseFactoryUI>(OnCloseUI);
    }

    private void OnOpenUI(Command_OpenFactoryUI cmd)
    {
        Debug.Log("[FactoryUIController] Received Command_OpenFactoryUI.");
        if (_panelContainer != null)
        {
            _panelContainer.style.display = DisplayStyle.Flex;
            Debug.Log("[FactoryUIController] Set _panelContainer display to Flex.");
        }
    }

    private void OnCloseUI(Command_CloseFactoryUI cmd)
    {
        if (_panelContainer != null)
            _panelContainer.style.display = DisplayStyle.None;
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

        if (_walletLabel != null)
            _walletLabel.text = $"Wallet: {evt.WalletGold} Gold";

        if (_collectBtn != null)
        {
            int amount = Mathf.FloorToInt(evt.CurrentAccumulation);
            _collectBtn.text = $"Collect ({amount} {evt.ActiveResource})";
            _collectBtn.SetEnabled(amount > 0);
        }

        if (_upgradeBtn != null)
        {
            _upgradeBtn.text = $"Upgrade Factory (Cost: {evt.UpgradeCost} Gold)";
            _upgradeBtn.SetEnabled(evt.WalletGold >= evt.UpgradeCost);
        }

        _currentActiveResource = evt.ActiveResource;
        
        if (_radioEnergy != null) _radioEnergy.value = (_currentActiveResource == ResourceType.Energy);
        if (_radioFood != null) _radioFood.value = (_currentActiveResource == ResourceType.Food);
        if (_radioElectronics != null) _radioElectronics.value = (_currentActiveResource == ResourceType.Electronics);
        if (_radioGold != null) _radioGold.value = (_currentActiveResource == ResourceType.Gold);

        _isUpdatingFromEvent = false;
    }

    private void OnResourceRadioChanged(ChangeEvent<bool> evt, ResourceType resourceType)
    {
        if (_isUpdatingFromEvent || !evt.newValue) return;
        
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
