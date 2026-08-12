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
        [SerializeField] private GameObject[] _hintImages;

        [Header("Data")]
        [SerializeField] private HintDatabase _database;

        private static HintData _lastShownHint;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetStaticData()
        {
            _lastShownHint = null;
        }

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
            
            if (_hintImages != null)
            {
                foreach (var img in _hintImages)
                {
                    if (img != null) img.SetActive(false);
                }
            }
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

            if (_hintImages != null && _hintImages.Length > 0)
            {
                foreach (var img in _hintImages)
                {
                    if (img != null) img.SetActive(false);
                }

                int randomIndex = Random.Range(0, _hintImages.Length);
                if (_hintImages[randomIndex] != null)
                {
                    _hintImages[randomIndex].SetActive(true);
                }
            }
        }
    }
}
