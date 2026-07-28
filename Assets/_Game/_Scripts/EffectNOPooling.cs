using UnityEngine;

public class EffectNOPooling : MonoBehaviour
{
    [SerializeField] private float lifeTime = 1;

    private void Start()
    {
        Destroy(gameObject, lifeTime);
    }
}
