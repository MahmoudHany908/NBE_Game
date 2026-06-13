using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(PlayerInputs), typeof(PlayerMovement), typeof(PlayerVisuals))]
public class Player : MonoBehaviour
{

    private PlayerMovement playerMovement;
    private PlayerInputs playerInputs;
    private PlayerVisuals playerVisuals;

    private Rigidbody rb;
    public static Player Instance { get; private set; }


    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    void Start()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerInputs = GetComponent<PlayerInputs>();
        playerVisuals = GetComponent<PlayerVisuals>();

        rb = GetComponent<Rigidbody>();
    }

    public PlayerMovement GetPlayerMovement()
    {
        return playerMovement;
    }
    public PlayerVisuals GetPlayerVisuals()
    {
        return playerVisuals;
    }
    public PlayerInputs GetPlayerInputs()
    {
        return playerInputs;
    }

    public Rigidbody GetPlayerRigidBody()
    {
        return rb;
    }



}
