using UnityEngine;

public class PlayerVisuals : MonoBehaviour
{
    [SerializeField] private Transform playerVisualTransform;

    private Player player;
    
    void Start()
    {
        player = GetComponent<Player>();
    }

    void Update()
    {
        RotateToMovementDirection();


    }

    private void RotateToMovementDirection()
    {
        Vector3 moveDirection = player.GetPlayerInputs().MoveDirection();
        if (moveDirection.sqrMagnitude > 0.01f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(moveDirection);
            playerVisualTransform.rotation = Quaternion.Slerp(playerVisualTransform.rotation, targetRotation, Time.deltaTime * 10f);
        }
    }

}





