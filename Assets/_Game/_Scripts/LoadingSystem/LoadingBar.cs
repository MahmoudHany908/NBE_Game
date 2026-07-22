using UnityEngine;
using UnityEngine.UI;

namespace VaultSurvivors.LoadingSystem
{
    public class LoadingBar : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField, Tooltip("Image used for the loading fill progress")] 
        private Image _fillImage;

        [Header("Settings")]
        [SerializeField, Range(1f, 10f)] 
        private float _lerpSpeed = 5f;

        private float _targetProgress = 0f;
        private float _currentProgress = 0f;

        public bool IsComplete => _currentProgress >= 1f;

        private void Update()
        {
            if (_currentProgress != _targetProgress)
            {
                _currentProgress = Mathf.MoveTowards(_currentProgress, _targetProgress, Time.unscaledDeltaTime * _lerpSpeed);
                
                // Snap if very close
                if (_targetProgress >= 1f && _currentProgress >= 0.99f)
                {
                    _currentProgress = 1f;
                }

                UpdateVisuals();
            }
        }

        public void SetTargetProgress(float normalizedProgress)
        {
            _targetProgress = Mathf.Clamp01(normalizedProgress);
        }

        public void ResetProgress()
        {
            _targetProgress = 0f;
            _currentProgress = 0f;
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            if (_fillImage != null)
            {
                _fillImage.fillAmount = _currentProgress;
            }
        }
    }
}
