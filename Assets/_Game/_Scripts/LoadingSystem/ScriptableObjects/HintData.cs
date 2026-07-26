using UnityEngine;

namespace VaultSurvivors.LoadingSystem.ScriptableObjects
{
    [CreateAssetMenu(fileName = "NewHintData", menuName = "Vault Survivors/Loading System/Hint Data")]
    public class HintData : ScriptableObject
    {
        [SerializeField] private string hintTitle = "Hint";
        [SerializeField, TextArea(2, 4)] private string description;
        [SerializeField] private Sprite icon;

        public string HintTitle => hintTitle;
        public string Description => description;
        public Sprite Icon => icon;
    }
}
