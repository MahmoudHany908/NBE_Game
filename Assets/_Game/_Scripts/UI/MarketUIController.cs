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
        public Image resourceIcon;
        public TextMeshProUGUI nameText;
        public TextMeshProUGUI priceText;
        public Image arrowImage;
        public Image priceFrameBg;
        public Button sellButton;
    }

    public class MarketUIController : MonoBehaviour, IDisposable
    {
        [Header("Main Panel")]
        [SerializeField] private GameObject _panelContainer;
        [SerializeField] private Button _closeBtn;

        [Header("HUD Buttons To Hide")]
        [SerializeField] private GameObject _hudButton1;
        [SerializeField] private GameObject _hudButton2;

        [Header("Resource UI")]
        [SerializeField] private MarketResourceUI foodUI;
        [SerializeField] private MarketResourceUI energyUI;
        [SerializeField] private MarketResourceUI electronicsUI;

        [SerializeField] private Color priceUpColor = new Color32(13, 249, 108, 255);
        [SerializeField] private Color priceDownColor = new Color32(255, 119, 71, 255);

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

        // (Bindings moved to Start to avoid wiping out GlobalUIButtonAudio listeners)

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

            if (foodUI != null && foodUI.sellButton != null)
                foodUI.sellButton.onClick.AddListener(() => OnSellClicked(ResourceType.Food));
            if (energyUI != null && energyUI.sellButton != null)
                energyUI.sellButton.onClick.AddListener(() => OnSellClicked(ResourceType.Energy));
            if (electronicsUI != null && electronicsUI.sellButton != null)
                electronicsUI.sellButton.onClick.AddListener(() => OnSellClicked(ResourceType.Electronics));

            if (quantityModal != null) quantityModal.Close();
            if (confirmModal != null) confirmModal.Close();
            if (_closeBtn != null)
            {
                // Note: Keep this if the close button isn't globally audio tagged
                _closeBtn.onClick.RemoveAllListeners();
                _closeBtn.onClick.AddListener(() => OnCloseUI(null));
            }
        }

        private void OnOpenUI(Command_OpenMarketUI cmd)
        {
            if (_panelContainer != null && !_panelContainer.activeSelf)
            {
                // First close any other open menus (this will call OnCloseUI)
                _eventPublisher?.Publish(new Command_CloseAllMenus());

                // Now ensure the root object is active BEFORE turning on the panel
                if (!gameObject.activeSelf)
                {
                    gameObject.SetActive(true);
                }

                _panelContainer.SetActive(true);
                _eventPublisher?.Publish(new Event_MenuStateChanged { isOpen = true });

                if (_hudButton1 != null) _hudButton1.SetActive(false);
                if (_hudButton2 != null) _hudButton2.SetActive(false);

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

                if (_hudButton1 != null) _hudButton1.SetActive(true);
                if (_hudButton2 != null) _hudButton2.SetActive(true);
            }

            // Turn the whole GameObject back off to prevent invisible raycast blockers
            gameObject.SetActive(false);
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
            Color dimmedColor = new Color32(102, 102, 102, 255);

            ApplyRowInteractability(foodUI, Mathf.FloorToInt(data.resources.food) > 0, dimmedColor);
            ApplyRowInteractability(energyUI, Mathf.FloorToInt(data.resources.energy) > 0, dimmedColor);
            ApplyRowInteractability(electronicsUI, Mathf.FloorToInt(data.resources.electronics) > 0, dimmedColor);
        }

        private void ApplyRowInteractability(MarketResourceUI uiItem, bool canSell, Color dimmedColor)
        {
            Color activeGreen = new Color32(13, 249, 108, 255);

            if (uiItem.sellButton != null)
            {
                uiItem.sellButton.interactable = canSell;

                Image btnImage = uiItem.sellButton.GetComponent<Image>();
                if (btnImage != null)
                    btnImage.color = canSell ? activeGreen : dimmedColor;

                TMP_Text btnText = uiItem.sellButton.GetComponentInChildren<TMP_Text>();
                if (btnText != null)
                    btnText.color = canSell ? activeGreen : dimmedColor;
            }

            if (!canSell)
            {
                // Dim the entire row
                if (uiItem.resourceIcon != null)
                    uiItem.resourceIcon.color = dimmedColor;
                if (uiItem.nameText != null)
                    uiItem.nameText.color = dimmedColor;
                if (uiItem.priceText != null)
                    uiItem.priceText.color = dimmedColor;
                if (uiItem.arrowImage != null)
                    uiItem.arrowImage.color = dimmedColor;
                if (uiItem.priceFrameBg != null)
                {
                    Color frameDimmed = dimmedColor;
                    frameDimmed.a = 50f / 255f;
                    uiItem.priceFrameBg.color = frameDimmed;
                }
            }
            else
            {
                // Restore sellable row
                if (uiItem.resourceIcon != null)
                    uiItem.resourceIcon.color = activeGreen;
                if (uiItem.nameText != null)
                    uiItem.nameText.color = activeGreen;
            }
        }

        private void UpdateUIItem(MarketResourceUI uiItem, float currentPrice, float previousPrice)
        {
            if (uiItem.priceText == null || uiItem.arrowImage == null) return;

            uiItem.priceText.text = $"{currentPrice:F1}";

            if (currentPrice >= previousPrice)
            {
                uiItem.priceText.color = priceUpColor;
                uiItem.arrowImage.color = priceUpColor;
                uiItem.arrowImage.gameObject.SetActive(true);
                uiItem.arrowImage.transform.rotation = Quaternion.Euler(0, 0, 0);
                if (uiItem.priceFrameBg != null)
                {
                    Color frameColor = priceUpColor;
                    frameColor.a = 50f / 255f;
                    uiItem.priceFrameBg.color = frameColor;
                }
            }
            else
            {
                uiItem.priceText.color = priceDownColor;
                uiItem.arrowImage.color = priceDownColor;
                uiItem.arrowImage.gameObject.SetActive(true);
                uiItem.arrowImage.transform.rotation = Quaternion.Euler(0, 0, 180);
                if (uiItem.priceFrameBg != null)
                {
                    Color frameColor = priceDownColor;
                    frameColor.a = 50f / 255f;
                    uiItem.priceFrameBg.color = frameColor;
                }
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
