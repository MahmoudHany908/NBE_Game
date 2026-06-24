using UnityEngine;


[CreateAssetMenu(fileName = "New Structure", menuName = "Structures/StructureSO")]
public class StructureSO : ScriptableObject
{



    [Header("UI")]
    public string Name;
    public string InteractionText;
    public Sprite Icon;

}