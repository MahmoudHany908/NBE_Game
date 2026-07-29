using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VaultSurvivors.Core.Production;
using VaultSurvivors.Core.Market;
using VContainer;

namespace VaultSurvivors.UI
{
    public class MarketConfirmModal : MonoBehaviour
    {
        [SerializeField] private TextMeshProUGUI confirmMessageText;
        [SerializeField] private Button confirmButton;
        [SerializeField] private Button cancelButton;

        private IEventPublisher _eventPublisher;
        private MarketUIController _uiController;

        private ResourceType _currentResource;
        private int _amount;

        [Inject]
        public void Construct(IEventPublisher eventPublisher)
        {
            _eventPublisher = eventPublisher;
        }

        public void Setup(MarketUIController uiController, ResourceType resource, int amount, float totalGold)
        {
            _uiController = uiController;
            _currentResource = resource;
            _amount = amount;

            confirmMessageText.text = $"Are you sure you want to sell {_amount} {_currentResource} for {totalGold:F1} Gold?";

            confirmButton.onClick.RemoveAllListeners();
            confirmButton.onClick.AddListener(OnConfirmClicked);

            cancelButton.onClick.RemoveAllListeners();
            cancelButton.onClick.AddListener(Close);

            gameObject.SetActive(true);
        }

        private void OnConfirmClicked()
        {
            _eventPublisher.Publish(new Command_SellResource(_currentResource, _amount));
            _uiController.CloseAllModals();
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }
    }
}
