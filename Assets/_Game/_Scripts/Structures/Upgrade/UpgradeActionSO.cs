using UnityEngine;

public abstract class UpgradeActionSO : ScriptableObject
{

    public abstract void Apply(StructureBase structure);

    public virtual void Remove(StructureBase structure) { }
}