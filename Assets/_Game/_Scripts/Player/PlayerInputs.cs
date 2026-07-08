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
    }

    private void OnEnable()
    {
        if (inputActions == null)
        {
            inputActions = new PlayerInputActions();
        }
        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions?.Player.Disable();
    }



    internal Vector3 MoveDirection()
    {
        Vector2 inputVector = inputActions.Player.Move.ReadValue<Vector2>();

        return new Vector3(inputVector.x, 0, inputVector.y).normalized;
    }


}
