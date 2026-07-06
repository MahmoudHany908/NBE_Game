using UnityEngine;

public interface ITargetProvider
{
    public GameObject GetTarget();
    public bool HasTarget();
    public event System.Action<GameObject> TargetChanged;

}
