using System.Collections.Generic;

using Alchemy.Inspector;

[System.Serializable]

public class StructureLevelSO
{
    [LabelText("Upgrade Cost")]
    public int upgradeCost; //placeholder

    [LabelText("Upgrade Actions")]
    public List<UpgradeActionSO> upgradeActions;
}