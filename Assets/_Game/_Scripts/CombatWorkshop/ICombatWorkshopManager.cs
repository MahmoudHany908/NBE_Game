using System.Collections.Generic;

public interface ICombatWorkshopManager
{
    bool IsUnlocked(string characterId);
    bool CanUnlock(string characterId);
    bool TryUnlock(string characterId);
    
    bool IsEquipped(string characterId);
    bool TryEquip(string characterId);
    bool TryUnequip(string characterId);

    List<SupportCharacterSO> GetUnlockedCharacters();
    List<SupportCharacterSO> GetEquippedCharacters();
}