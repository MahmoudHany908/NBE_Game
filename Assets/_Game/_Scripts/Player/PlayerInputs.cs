using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerInputs : MonoBehaviour
{
    [Header("Aim")]
    [SerializeField] private float aimDeadzone = 0.15f;

    private PlayerInputActions inputActions;

    public Vector3 CurrentAimDirection { get; private set; } = Vector3.forward;
    public Vector3 LastAimDirection { get; private set; } = Vector3.forward;

    public bool HasAimDirection { get; private set; }
    public bool HasLastAimDirection { get; private set; }

    private void Awake()
    {
        inputActions = new PlayerInputActions();
    }

    private void OnEnable()
    {
        if (inputActions == null)
            inputActions = new PlayerInputActions();

        inputActions.Player.Enable();
    }

    private void OnDisable()
    {
        inputActions?.Player.Disable();
    }

    private void Update()
    {
        Vector2 aimInput = inputActions.Player.AttackDirection.ReadValue<Vector2>();
        Vector3 dir = new Vector3(aimInput.x, 0f, aimInput.y);

        if (dir.sqrMagnitude > aimDeadzone * aimDeadzone)
        {
            dir = dir.normalized;

            CurrentAimDirection = dir;
            LastAimDirection = dir;

            HasAimDirection = true;
            HasLastAimDirection = true;
        }
        else
        {
            CurrentAimDirection = Vector3.zero;
            HasAimDirection = false;
        }
    }

    internal Vector3 MoveDirection()
    {
        Vector2 inputVector = inputActions.Player.Move.ReadValue<Vector2>();
        return new Vector3(inputVector.x, 0f, inputVector.y).normalized;
    }

    public Vector3 AttackDirection()
    {
        return CurrentAimDirection;
    }
}