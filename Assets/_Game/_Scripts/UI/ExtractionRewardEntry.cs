using TMPro;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A single reward entry in the extraction summary panel.
/// </summary>
public class ExtractionRewardEntry : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI label;
    [SerializeField] private Image icon;

    public void Initialize(string text, Sprite iconSprite = null)
    {
        if (label != null)
            label.text = text;

        if (icon != null)
        {
            if (iconSprite != null)
            {
                icon.sprite = iconSprite;
                icon.gameObject.SetActive(true);
            }
            else
            {
                icon.gameObject.SetActive(false);
            }
        }
    }
}
