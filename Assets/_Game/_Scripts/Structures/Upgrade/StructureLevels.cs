using System.Collections.Generic;
using Alchemy.Inspector;
using UnityEngine;
using UnityEngine.UIElements;

[CreateAssetMenu(fileName = "Structure Levels", menuName = "Structures/Structure Levels")]
public class StructureLevels : ScriptableObject
{
    [ListViewSettings(
        ShowFoldoutHeader = true,
        ShowAlternatingRowBackgrounds = AlternatingRowBackground.All)]
    [LabelText("Structure Levels")]
    public List<StructureLevelSO> structureLevels;
}