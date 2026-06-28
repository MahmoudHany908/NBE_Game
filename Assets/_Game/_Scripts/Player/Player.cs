using UnityEngine;


[RequireComponent(typeof(PlayerInputs), typeof(PlayerMovement), typeof(PlayerVisuals))]
public class Player : MonoBehaviour
{
    private PlayerMovement playerMovement;
    private PlayerInputs playerInputs;
    private PlayerVisuals playerVisuals;
    private PlayerReference playerReference;
    
    private Rigidbody rb;



    void Awake()
    {
        playerMovement = GetComponent<PlayerMovement>();
        playerInputs = GetComponent<PlayerInputs>();
        playerVisuals = GetComponent<PlayerVisuals>();
        playerReference = GetComponent<PlayerReference>();
        rb = GetComponent<Rigidbody>();
    }







    public PlayerMovement GetPlayerMovement() => playerMovement;
    public PlayerVisuals GetPlayerVisuals() => playerVisuals;
    public PlayerInputs GetPlayerInputs() => playerInputs;
    public PlayerReference GetPlayerReference() => playerReference;
    public Rigidbody GetPlayerRigidBody() => rb;

}
