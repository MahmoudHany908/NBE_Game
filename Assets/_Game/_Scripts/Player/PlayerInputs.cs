using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputs : MonoBehaviour
{
    private Camera cam;
    
    private PlayerInputActions inputActions;
    
    private void Awake()
    {
        cam = Camera.main;
        inputActions = new PlayerInputActions();
        inputActions.Player.Enable();
    }



    internal Vector3 MoveDirection()
    {
        Vector2 inputVector = inputActions.Player.Move.ReadValue<Vector2>();

        // Simple world-relative movement (ignoring camera rotation)
        return new Vector3(inputVector.x, 0, inputVector.y).normalized;
    }


}
