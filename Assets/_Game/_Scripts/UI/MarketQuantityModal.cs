using UnityEngine;
using UnityEngine.UI;
using TMPro;
using VaultSurvivors.Core.Production;
using VaultSurvivors.Core.Market;

namespace VaultSurvivors.UI
{
    public class MarketQuantityModal : MonoBehaviour
    {
        [SerializeField] private Slider quantitySlider;
        [SerializeField] private TMP_InputField quantityInputField;
        [SerializeField] private TextMeshProUGUI expectedReturnText;
        [SerializeField] private Button nextButton;
        [SerializeField] private Button closeButton;

        private MarketUIController _uiController;
        private ResourceType _currentResource;
        private float _currentPrice;
        private int _maxAmount;
        private int _selectedAmount;

        public void Setup(MarketUIController uiController, ResourceType resource, float currentPrice, int maxAmount)
        {
            _uiController = uiController;
            _currentResource = resource;
            _currentPrice = currentPrice;
            _maxAmount = maxAmount;

            quantitySlider.minValue = 0;
            quantitySlider.maxValue = _maxAmount;
            quantitySlider.value = 0;

            quantityInputField.text = "0";

            UpdateExpectedReturn(0);

            quantitySlider.onValueChanged.RemoveAllListeners();
            quantitySlider.onValueChanged.AddListener(OnSliderValueChanged);

            quantityInputField.onValueChanged.RemoveAllListeners();
            quantityInputField.onValueChanged.AddListener(OnInputFieldValueChanged);

            nextButton.onClick.RemoveAllListeners();
            nextButton.onClick.AddListener(OnNextClicked);

            if (closeButton != null)
            {
                closeButton.onClick.RemoveAllListeners();
                closeButton.onClick.AddListener(Close);
            }

            gameObject.SetActive(true);
        }

        private void OnSliderValueChanged(float value)
        {
            _selectedAmount = Mathf.FloorToInt(value);
            quantityInputField.text = _selectedAmount.ToString();
            UpdateExpectedReturn(_selectedAmount);
        }

        private void OnInputFieldValueChanged(string value)
        {
            if (int.TryParse(value, out int parsedValue))
            {
                _selectedAmount = Mathf.Clamp(parsedValue, 0, _maxAmount);
                if (parsedValue != _selectedAmount)
                {
                    quantityInputField.text = _selectedAmount.ToString();
                }
                quantitySlider.value = _selectedAmount;
                UpdateExpectedReturn(_selectedAmount);
            }
        }

        private void UpdateExpectedReturn(int amount)
        {
            float totalGold = amount * _currentPrice;
            expectedReturnText.text = $"Expected Return: {totalGold:F1} Gold";
            nextButton.interactable = amount > 0;
        }

        private void OnNextClicked()
        {
            if (_selectedAmount > 0)
            {
                _uiController.OpenConfirmModal(_currentResource, _selectedAmount, _selectedAmount * _currentPrice);
            }
        }

        public void Close()
        {
            gameObject.SetActive(false);
        }
    }
}
