using System;
using UnityEngine;
using UnityEngine.InputSystem;

namespace VaultSurvivors.LoadingSystem
{
    public class TapToContinueController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private CanvasGroup _canvasGroup;

        [Header("Settings")]
        [SerializeField] private float _fadeSpeed = 2f;
        
        public event Action OnTapDetected;

        private bool _isEnabled = false;

        private float _currentAnimationTime = 0f;

        private void Awake()
        {
            if (_canvasGroup == null)
            {
                _canvasGroup = GetComponent<CanvasGroup>();
                if (_canvasGroup == null)
                {
                    _canvasGroup = gameObject.AddComponent<CanvasGroup>();
                }
            }
            Disable();
        }

        public void Enable()
        {
            _isEnabled = true;
            _currentAnimationTime = 0f;
            if (_canvasGroup != null)
            {
                _canvasGroup.alpha = 0f;
                _canvasGroup.gameObject.SetActive(true);
            }
        }

        public void Disable()
        {
            _isEnabled = false;
            if (_canvasGroup != null)
            {
                _canvasGroup.gameObject.SetActive(false);
                _canvasGroup.alpha = 0f;
            }
        }

        private void Update()
        {
            if (!_isEnabled) return;

            UpdateFadeAnimation();
            CheckForInput();
        }

        private void UpdateFadeAnimation()
        {
            if (_canvasGroup != null)
            {
                _currentAnimationTime += Time.unscaledDeltaTime;
                // Smoothly ping-pong alpha between 0 and 1
                _canvasGroup.alpha = Mathf.PingPong(_currentAnimationTime * _fadeSpeed, 1f);
            }
        }

        private void CheckForInput()
        {
            bool tapDetected = false;

            // Check Mouse
            if (Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
            {
                tapDetected = true;
            }

            // Check Touch
            if (Touchscreen.current != null && Touchscreen.current.primaryTouch.press.wasPressedThisFrame)
            {
                tapDetected = true;
            }

            if (tapDetected)
            {
                Disable();
                OnTapDetected?.Invoke();
            }
        }
    }
}
