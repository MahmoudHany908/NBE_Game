using UnityEngine;


[RequireComponent(typeof(PlayerInputs), typeof(PlayerMovement), typeof(PlayerVisuals))]
public class Player : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private PlayerInputs playerInputs;
    private PlayerVisuals playerVisuals;


    private Rigidbody rb;



    void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerInputs = GetComponent<PlayerInputs>();
        playerVisuals = GetComponent<PlayerVisuals>();
        rb = GetComponent<Rigidbody>();
    }







    public PlayerMovement GetPlayerMovement() => playerMovement;
    public PlayerVisuals GetPlayerVisuals() => playerVisuals;
    public PlayerInputs GetPlayerInputs() => playerInputs;
    public Rigidbody GetPlayerRigidBody() => rb;

}
