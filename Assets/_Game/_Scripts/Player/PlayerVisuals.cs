using UnityEngine;

public class PlayerVisuals : MonoBehaviour
{
    [SerializeField] private Transform playerVisualTransform;
    [SerializeField] private Animator animator;
    private Player player;

    private bool isMoving;
    void Start()
    {
        player = GetComponent<Player>();
    }

    void Update()
    {
        RotateToMovementDirection();
        isMoving = player.GetPlayerMovement().MovementDirection.sqrMagnitude > 0.01f;

        animator.SetBool("isMoving", isMoving);

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





