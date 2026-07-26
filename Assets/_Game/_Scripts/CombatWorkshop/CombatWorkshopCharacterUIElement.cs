using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CombatWorkshopCharacterUIElement : MonoBehaviour
{
    [SerializeField] private Image _characterIcon;
    [SerializeField] private TMP_Text _characterName;
    [SerializeField] private TMP_Text _characterDescription;
    [SerializeField] private TMP_Text _costText;
    
    [SerializeField] private Button _unlockBtn;
    [SerializeField] private Button _equipBtn;
    [SerializeField] private Button _unequipBtn;

    private string _characterId;
    private IEventPublisher _eventPublisher;

    public void Setup(SupportCharacterSO data, bool isUnlocked, bool isEquipped, bool canEquipMore, IEventPublisher eventPublisher)
    {
        _characterId = data.CharacterId;
        _eventPublisher = eventPublisher;

        if (_characterIcon != null) _characterIcon.sprite = data.Icon;
        if (_characterName != null) _characterName.text = data.DisplayName;
        if (_characterDescription != null) _characterDescription.text = "";
        
        if (_costText != null)
        {
            if (data.UnlockCost != null && data.UnlockCost.Count > 0)
            {
                _costText.text = $"Cost: {data.UnlockCost[0].Amount} {data.UnlockCost[0].Type}";
            }
            else
            {
                _costText.text = "Free";
            }
        }

        // Configure Buttons
        if (_unlockBtn != null)
        {
            _unlockBtn.gameObject.SetActive(!isUnlocked);
            _unlockBtn.onClick.RemoveAllListeners();
            _unlockBtn.onClick.AddListener(OnUnlockClicked);
        }

        if (_equipBtn != null)
        {
            _equipBtn.gameObject.SetActive(isUnlocked && !isEquipped);
            _equipBtn.interactable = canEquipMore;
            _equipBtn.onClick.RemoveAllListeners();
            _equipBtn.onClick.AddListener(OnEquipClicked);
        }

        if (_unequipBtn != null)
        {
            _unequipBtn.gameObject.SetActive(isUnlocked && isEquipped);
            _unequipBtn.onClick.RemoveAllListeners();
            _unequipBtn.onClick.AddListener(OnUnequipClicked);
        }
    }

    private void OnUnlockClicked()
    {
        _eventPublisher?.Publish(new Command_UnlockCharacter(_characterId));
    }

    private void OnEquipClicked()
    {
        _eventPublisher?.Publish(new Command_EquipCharacter(_characterId));
    }

    private void OnUnequipClicked()
    {
        _eventPublisher?.Publish(new Command_UnequipCharacter(_characterId));
    }
}
