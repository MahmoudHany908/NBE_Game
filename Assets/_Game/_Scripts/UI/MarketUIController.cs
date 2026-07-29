using System;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VContainer;
using VaultSurvivors.Core.Market;
using VaultSurvivors.Core.Production;

namespace VaultSurvivors.UI
{
    [System.Serializable]
    public class MarketResourceUI
    {
        public ResourceType resourceType;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI priceText;
        public Image arrowImage;
        public Button sellButton;
    }

    public class MarketUIController : MonoBehaviour, IDisposable
    {
        [Header("Main Panel")]
        [SerializeField] private GameObject _panelContainer;
        [SerializeField] private Button _closeBtn;

        [Header("Resource UI")]
        [SerializeField] private MarketResourceUI foodUI;
        [SerializeField] private MarketResourceUI energyUI;
        [SerializeField] private MarketResourceUI electronicsUI;
        
        [SerializeField] private Color priceUpColor = new Color(13, 249, 108);
        [SerializeField] private Color priceDownColor = new Color(255, 71, 47);

        [Header("Modals")]
        [SerializeField] private MarketQuantityModal quantityModal;
        [SerializeField] private MarketConfirmModal confirmModal;

        private IEventPublisher _eventPublisher;
        private IEventSubscriber _eventSubscriber;
        private ISaveSystem _saveSystem;

        private float _lastKnownFoodPrice;
        private float _lastKnownEnergyPrice;
        private float _lastKnownElectronicsPrice;

        private bool _isConstructed = false;

        [Inject]
        public void Construct(IEventPublisher eventPublisher, IEventSubscriber eventSubscriber, ISaveSystem saveSystem)
        {
            if (_isConstructed) return;
            _isConstructed = true;

            _eventPublisher = eventPublisher;
            _eventSubscriber = eventSubscriber;
            _saveSystem = saveSystem;

            // Subscribe here! VContainer injects this even if the GameObject is inactive.
            if (_eventSubscriber != null)
            {
                _eventSubscriber.Subscribe<Event_MarketPricesUpdated>(OnPricesUpdated);
                _eventSubscriber.Subscribe<Command_OpenMarketUI>(OnOpenUI);
                _eventSubscriber.Subscribe<Command_CloseMarketUI>(OnCloseUI);
                _eventSubscriber.Subscribe<Command_CloseAllMenus>(OnCloseAllMenus);
            }
        }

        private void Awake()
        {
            if (_panelContainer != null) _panelContainer.SetActive(false);
        }

        private void OnEnable()
        {
            if (foodUI != null && foodUI.sellButton != null)
                foodUI.sellButton.onClick.AddListener(() => OnSellClicked(ResourceType.Food));
            if (energyUI != null && energyUI.sellButton != null)
                energyUI.sellButton.onClick.AddListener(() => OnSellClicked(ResourceType.Energy));
            if (electronicsUI != null && electronicsUI.sellButton != null)
                electronicsUI.sellButton.onClick.AddListener(() => OnSellClicked(ResourceType.Electronics));
        }

        private void OnDisable()
        {
            if (foodUI != null && foodUI.sellButton != null)
                foodUI.sellButton.onClick.RemoveAllListeners();
            if (energyUI != null && energyUI.sellButton != null)
                energyUI.sellButton.onClick.RemoveAllListeners();
            if (electronicsUI != null && electronicsUI.sellButton != null)
                electronicsUI.sellButton.onClick.RemoveAllListeners();
        }

        /// <summary>
        /// Called by VContainer when the LifetimeScope shuts down.
        /// Unlike OnDestroy(), this is guaranteed to run even if
        /// the GameObject was never activated during its lifetime.
        /// </summary>
        public void Dispose()
        {
            if (_eventSubscriber != null)
            {
                _eventSubscriber.Unsubscribe<Event_MarketPricesUpdated>(OnPricesUpdated);
                _eventSubscriber.Unsubscribe<Command_OpenMarketUI>(OnOpenUI);
                _eventSubscriber.Unsubscribe<Command_CloseMarketUI>(OnCloseUI);
                _eventSubscriber.Unsubscribe<Command_CloseAllMenus>(OnCloseAllMenus);
            }
            _isConstructed = false;
        }

        private void Start()
        {
            if (foodUI.nameText != null) foodUI.nameText.text = "Food";
            if (energyUI.nameText != null) energyUI.nameText.text = "Energy";
            if (electronicsUI.nameText != null) electronicsUI.nameText.text = "Electronics";

            if (quantityModal != null) quantityModal.Close();
            if (confirmModal != null) confirmModal.Close();
            if (_closeBtn != null)
            {
                _closeBtn.onClick.RemoveAllListeners();
                _closeBtn.onClick.AddListener(() => OnCloseUI(null));
            }
        }

        private void OnOpenUI(Command_OpenMarketUI cmd)
        {
            // Even if the GameObject is inactive, VContainer will call this method!
            if (!gameObject.activeSelf) 
            {
                gameObject.SetActive(true);
            }

            if (_panelContainer != null && !_panelContainer.activeSelf)
            {
                _eventPublisher?.Publish(new Command_CloseAllMenus());
                _panelContainer.SetActive(true);
                _eventPublisher?.Publish(new Event_MenuStateChanged { isOpen = true });
                
                // Request prices to populate immediately
                _eventPublisher?.Publish(new Command_RequestMarketPrices());
            }
        }

        private void OnCloseUI(Command_CloseMarketUI cmd)
        {
            if (_panelContainer != null && _panelContainer.activeSelf)
            {
                CloseAllModals();
                _panelContainer.SetActive(false);
                _eventPublisher?.Publish(new Event_MenuStateChanged { isOpen = false });
            }

            // Optional: turn the whole GameObject back off if you want it fully inactive
            // gameObject.SetActive(false);
        }

        private void OnCloseAllMenus(Command_CloseAllMenus cmd)
        {
            OnCloseUI(null);
        }

        private void OnPricesUpdated(Event_MarketPricesUpdated evt)
        {
            _lastKnownFoodPrice = evt.FoodPrice;
            _lastKnownEnergyPrice = evt.EnergyPrice;
            _lastKnownElectronicsPrice = evt.ElectronicsPrice;

            UpdateUIItem(foodUI, evt.FoodPrice, evt.PreviousFoodPrice);
            UpdateUIItem(energyUI, evt.EnergyPrice, evt.PreviousEnergyPrice);
            UpdateUIItem(electronicsUI, evt.ElectronicsPrice, evt.PreviousElectronicsPrice);

            UpdateInteractability();
        }

        private void UpdateInteractability()
        {
            GameData data = _saveSystem.HasSaveFile() ? _saveSystem.Load() : new GameData();
            
            if (foodUI.sellButton != null)
                foodUI.sellButton.interactable = Mathf.FloorToInt(data.resources.food) > 0;
                
            if (energyUI.sellButton != null)
                energyUI.sellButton.interactable = Mathf.FloorToInt(data.resources.energy) > 0;
                
            if (electronicsUI.sellButton != null)
                electronicsUI.sellButton.interactable = Mathf.FloorToInt(data.resources.electronics) > 0;
        }

        private void UpdateUIItem(MarketResourceUI uiItem, float currentPrice, float previousPrice)
        {
            if (uiItem.priceText == null || uiItem.arrowImage == null) return;

            uiItem.priceText.text = $"{currentPrice:F1} Gold";

            if (currentPrice >= previousPrice)
            {
                uiItem.priceText.color = priceUpColor;
                uiItem.arrowImage.color = priceUpColor;
                uiItem.arrowImage.gameObject.SetActive(true);
                uiItem.arrowImage.transform.rotation = Quaternion.Euler(0, 0, 0); 
            }
            else
            {
                uiItem.priceText.color = priceDownColor;
                uiItem.arrowImage.color = priceDownColor;
                uiItem.arrowImage.gameObject.SetActive(true);
                uiItem.arrowImage.transform.rotation = Quaternion.Euler(0, 0, 180);
            }
        }

        private void OnSellClicked(ResourceType type)
        {
            if (quantityModal == null) return;

            float price = 0f;
            int maxAmount = 0;
            GameData data = _saveSystem.HasSaveFile() ? _saveSystem.Load() : new GameData();

            switch (type)
            {
                case ResourceType.Food:
                    price = _lastKnownFoodPrice;
                    maxAmount = Mathf.FloorToInt(data.resources.food);
                    break;
                case ResourceType.Energy:
                    price = _lastKnownEnergyPrice;
                    maxAmount = Mathf.FloorToInt(data.resources.energy);
                    break;
                case ResourceType.Electronics:
                    price = _lastKnownElectronicsPrice;
                    maxAmount = Mathf.FloorToInt(data.resources.electronics);
                    break;
            }

            quantityModal.Setup(this, type, price, maxAmount);
        }

        public void OpenConfirmModal(ResourceType type, int amount, float totalGold)
        {
            if (confirmModal == null) return;
            quantityModal.Close();
            confirmModal.Setup(this, type, amount, totalGold);
        }

        public void CloseAllModals()
        {
            if (quantityModal != null) quantityModal.Close();
            if (confirmModal != null) confirmModal.Close();

            // Refresh buttons immediately after a transaction
            UpdateInteractability();
        }
    }
}
