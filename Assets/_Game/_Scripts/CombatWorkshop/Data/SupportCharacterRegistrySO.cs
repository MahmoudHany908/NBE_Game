using System.Collections.Generic;
using UnityEngine;

namespace DefaultNamespace.Data
{
    [CreateAssetMenu(fileName = "SupportCharacterRegistry", menuName = "CombatWorkshop/Support Character Registry")]
    public class SupportCharacterRegistrySO:ScriptableObject
    {
        public List<SupportCharacterSO> Characters = new List<SupportCharacterSO>();
    }
}