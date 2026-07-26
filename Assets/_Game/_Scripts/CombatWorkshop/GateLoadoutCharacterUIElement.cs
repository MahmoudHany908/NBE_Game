using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GateLoadoutCharacterUIElement : MonoBehaviour
{
    [SerializeField] private Image _characterIcon;
    [SerializeField] private TMP_Text _characterName;

    public void Setup(SupportCharacterSO data)
    {
        if (_characterIcon != null) _characterIcon.sprite = data.Icon;
        if (_characterName != null) _characterName.text = data.DisplayName;
    }
}
