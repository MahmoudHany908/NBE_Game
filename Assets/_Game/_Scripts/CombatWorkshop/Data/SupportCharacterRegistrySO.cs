using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SupportCharacterRegistry", menuName = "CombatWorkshop/Support Character Registry")]
public class SupportCharacterRegistrySO : ScriptableObject
{
    public List<SupportCharacterSO> Characters = new List<SupportCharacterSO>();
    
    [Tooltip("Max number of support characters the player can equip for a run. Adjustable here, not a hardcoded constant.")]
    public int MaxLoadoutSlots = 2;
}