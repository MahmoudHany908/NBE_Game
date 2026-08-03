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
        private int _sliderStep;
        private bool _isUpdatingFromInput;

        public void Setup(MarketUIController uiController, ResourceType resource, float currentPrice, int maxAmount)
        {
            _uiController = uiController;
            _currentResource = resource;
            _currentPrice = currentPrice;
            _maxAmount = maxAmount;
            _isUpdatingFromInput = false;

            // Tiered step sizes based on max amount
            if (_maxAmount <= 50)
                _sliderStep = 1;
            else if (_maxAmount <= 500)
                _sliderStep = 5;
            else if (_maxAmount <= 5000)
                _sliderStep = 50;
            else
                _sliderStep = 100;

            quantitySlider.wholeNumbers = true;
            quantitySlider.minValue = 0;
            quantitySlider.maxValue = _maxAmount;
            quantitySlider.value = 0;

            quantityInputField.text = "0";
            quantityInputField.contentType = TMP_InputField.ContentType.IntegerNumber;

            UpdateExpectedReturn(0);

            quantitySlider.onValueChanged.RemoveAllListeners();
            quantitySlider.onValueChanged.AddListener(OnSliderValueChanged);

            quantityInputField.onValueChanged.RemoveAllListeners();
            quantityInputField.onEndEdit.RemoveAllListeners();
            quantityInputField.onValueChanged.AddListener(OnInputFieldValueChanged);
            quantityInputField.onEndEdit.AddListener(OnInputFieldEndEdit);

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
            // Don't overwrite the input field while the user is typing
            if (_isUpdatingFromInput) return;

            // Snap to nearest step boundary
            int rawValue = Mathf.RoundToInt(value);
            int snapped = Mathf.RoundToInt((float)rawValue / _sliderStep) * _sliderStep;
            _selectedAmount = Mathf.Clamp(snapped, 0, _maxAmount);

            // Update slider position without re-triggering
            quantitySlider.onValueChanged.RemoveListener(OnSliderValueChanged);
            quantitySlider.value = _selectedAmount;
            quantitySlider.onValueChanged.AddListener(OnSliderValueChanged);

            quantityInputField.text = _selectedAmount.ToString();
            UpdateExpectedReturn(_selectedAmount);
        }

        /// <summary>
        /// While typing, update the selected amount and expected return live,
        /// but don't clamp or overwrite the text so the user can type freely.
        /// </summary>
        private void OnInputFieldValueChanged(string value)
        {
            if (int.TryParse(value, out int parsedValue))
            {
                _selectedAmount = Mathf.Clamp(parsedValue, 0, _maxAmount);

                // Move the slider without it overwriting the input field
                _isUpdatingFromInput = true;
                quantitySlider.value = _selectedAmount;
                _isUpdatingFromInput = false;

                UpdateExpectedReturn(_selectedAmount);
            }
        }

        /// <summary>
        /// When the user finishes typing (clicks away or presses Enter),
        /// clamp the value and finalize the input field text.
        /// </summary>
        private void OnInputFieldEndEdit(string value)
        {
            if (int.TryParse(value, out int parsedValue))
            {
                _selectedAmount = Mathf.Clamp(parsedValue, 0, _maxAmount);
            }
            else
            {
                _selectedAmount = 0;
            }

            quantityInputField.text = _selectedAmount.ToString();

            _isUpdatingFromInput = true;
            quantitySlider.value = _selectedAmount;
            _isUpdatingFromInput = false;

            UpdateExpectedReturn(_selectedAmount);
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
