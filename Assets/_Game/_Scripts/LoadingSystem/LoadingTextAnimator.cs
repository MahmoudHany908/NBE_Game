using TMPro;
using UnityEngine;

namespace VaultSurvivors.LoadingSystem
{
    public class LoadingTextAnimator : MonoBehaviour
    {
        [SerializeField] private TMP_Text _loadingText;
        [SerializeField] private float _dotCycleInterval = 0.4f;

        private bool _isAnimating = false;
        private float _timer = 0f;
        private int _dotCount = 0;

        private const string BASE_TEXT = "Loading";

        public void StartAnimation()
        {
            if (_loadingText == null) return;
            
            _isAnimating = true;
            _timer = 0f;
            _dotCount = 0;
            UpdateText();
        }

        public void StopAnimation()
        {
            _isAnimating = false;
        }

        private void Update()
        {
            if (!_isAnimating || _loadingText == null) return;

            _timer += Time.unscaledDeltaTime;

            if (_timer >= _dotCycleInterval)
            {
                _timer -= _dotCycleInterval;
                _dotCount = (_dotCount + 1) % 4; // Cycles: 0, 1, 2, 3
                UpdateText();
            }
        }

        private void UpdateText()
        {
            string dots = new string('.', _dotCount);
            _loadingText.text = BASE_TEXT + dots;
        }
    }
}
