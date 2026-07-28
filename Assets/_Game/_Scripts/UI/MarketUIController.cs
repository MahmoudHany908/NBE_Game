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

    public class MarketUIController : MonoBehaviour
    {
        [SerializeField] private MarketResourceUI foodUI;
        [SerializeField] private MarketResourceUI energyUI;
        [SerializeField] private MarketResourceUI electronicsUI;
        
        [SerializeField] private Color priceUpColor = Color.green;
        [SerializeField] private Color priceDownColor = Color.red;

        // You might want a reference to arrow sprites (up/down) if you change the sprite dynamically
        // Or you can just rotate a single arrow sprite by 180 degrees.
        // We'll rotate the existing arrow image.

        private IEventPublisher _eventPublisher;
        private IEventSubscriber _eventSubscriber;

        [Inject]
        public void Construct(IEventPublisher eventPublisher, IEventSubscriber eventSubscriber)
        {
            _eventPublisher = eventPublisher;
            _eventSubscriber = eventSubscriber;
        }

        private void OnEnable()
        {
            if (_eventSubscriber != null)
            {
                _eventSubscriber.Subscribe<Event_MarketPricesUpdated>(OnPricesUpdated);
            }
            
            // Wire up sell buttons
            foodUI.sellButton.onClick.AddListener(() => OnSellClicked(ResourceType.Food));
            energyUI.sellButton.onClick.AddListener(() => OnSellClicked(ResourceType.Energy));
            electronicsUI.sellButton.onClick.AddListener(() => OnSellClicked(ResourceType.Electronics));
        }

        private void OnDisable()
        {
            if (_eventSubscriber != null)
            {
                _eventSubscriber.Unsubscribe<Event_MarketPricesUpdated>(OnPricesUpdated);
            }
            
            foodUI.sellButton.onClick.RemoveAllListeners();
            energyUI.sellButton.onClick.RemoveAllListeners();
            electronicsUI.sellButton.onClick.RemoveAllListeners();
        }

        private void Start()
        {
            // Set initial names
            if (foodUI.nameText != null) foodUI.nameText.text = "Food";
            if (energyUI.nameText != null) energyUI.nameText.text = "Energy";
            if (electronicsUI.nameText != null) electronicsUI.nameText.text = "Electronics";
        }

        private void OnPricesUpdated(Event_MarketPricesUpdated evt)
        {
            UpdateUIItem(foodUI, evt.FoodPrice, evt.PreviousFoodPrice);
            UpdateUIItem(energyUI, evt.EnergyPrice, evt.PreviousEnergyPrice);
            UpdateUIItem(electronicsUI, evt.ElectronicsPrice, evt.PreviousElectronicsPrice);
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
                // Point arrow UP
                uiItem.arrowImage.transform.rotation = Quaternion.Euler(0, 0, 0); 
            }
            else
            {
                uiItem.priceText.color = priceDownColor;
                uiItem.arrowImage.color = priceDownColor;
                uiItem.arrowImage.gameObject.SetActive(true);
                // Point arrow DOWN
                uiItem.arrowImage.transform.rotation = Quaternion.Euler(0, 0, 180);
            }
        }

        private void OnSellClicked(ResourceType type)
        {
            // For Phase 2, the user just wants the button there. 
            // Phase 3 will involve popping up the modal.
            Debug.Log($"[MarketUIController] Sell clicked for {type}. Modal popup will go here in Phase 3.");
        }
    }
}
