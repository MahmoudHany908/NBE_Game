using UnityEngine;
using UnityEngine.InputSystem;

public class TESTING : MonoBehaviour
{
    [SerializeField] private Effect vfx;
    [SerializeField] private Projectile projectile;

    [SerializeField] private SoundData soundData;

    private void Update()
    {
        if (Keyboard.current.qKey.isPressed)
        {
            var vfxInstance = PoolManager.Instance.Spawn(vfx.gameObject, transform.position + Vector3.right, Quaternion.identity);
     
        }
        if (Keyboard.current.eKey.isPressed)
        {
            var projectileInstance = PoolManager.Instance.Spawn(projectile.gameObject, transform.position + Vector3.left, Quaternion.identity);
        }



        if(Keyboard.current.fKey.wasPressedThisFrame)
        {
            AudioManager.Instance.PlaySoundAtPosition(soundData, transform.position);
        }

    }
}
