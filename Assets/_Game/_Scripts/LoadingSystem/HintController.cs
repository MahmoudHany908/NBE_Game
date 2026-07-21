using TMPro;
using UnityEngine;
using UnityEngine.UI;
using VaultSurvivors.LoadingSystem.ScriptableObjects;

namespace VaultSurvivors.LoadingSystem
{
    public class HintController : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private TMP_Text _titleText;
        [SerializeField] private TMP_Text _descriptionText;
        [SerializeField] private Image _iconImage;

        [Header("Data")]
        [SerializeField] private HintDatabase _database;

        private static HintData _lastShownHint;

        public void ShowRandomHint()
        {
            if (_database == null)
            {
                Debug.LogWarning("[HintController] HintDatabase is missing.");
                return;
            }

            HintData newHint = _database.GetRandomHint(_lastShownHint);
            
            if (newHint != null)
            {
                _lastShownHint = newHint;
                UpdateVisuals(newHint);
            }
        }

        public void HideHint()
        {
            if (_titleText != null) _titleText.text = string.Empty;
            if (_descriptionText != null) _descriptionText.text = string.Empty;
            if (_iconImage != null) _iconImage.gameObject.SetActive(false);
        }

        private void UpdateVisuals(HintData data)
        {
            if (_titleText != null)
            {
                _titleText.text = data.HintTitle;
            }

            if (_descriptionText != null)
            {
                _descriptionText.text = data.Description;
            }

            if (_iconImage != null)
            {
                if (data.Icon != null)
                {
                    _iconImage.sprite = data.Icon;
                    _iconImage.gameObject.SetActive(true);
                }
                else
                {
                    _iconImage.gameObject.SetActive(false);
                }
            }
        }
    }
}
