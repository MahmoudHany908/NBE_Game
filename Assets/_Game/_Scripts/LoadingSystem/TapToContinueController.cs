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

        private void Awake()
        {
            Disable();
        }

        public void Enable()
        {
            _isEnabled = true;
            if (_canvasGroup != null)
            {
                _canvasGroup.gameObject.SetActive(true);
            }
        }

        public void Disable()
        {
            _isEnabled = false;
            if (_canvasGroup != null)
            {
                _canvasGroup.gameObject.SetActive(false);
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
                // Smoothly ping-pong alpha between 0 and 1
                float alpha = Mathf.PingPong(Time.unscaledTime * _fadeSpeed, 1f);
                
                // Add a small offset to prevent it from going completely invisible for too long
                _canvasGroup.alpha = Mathf.Lerp(0.2f, 1f, alpha);
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
