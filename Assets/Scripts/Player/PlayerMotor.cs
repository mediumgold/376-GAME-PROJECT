using UnityEngine;
using System;
using UnityEngine.TextCore.Text;


public class PlayerMotor : MonoBehaviour
{

    [SerializeField] private Player player;
    private Inventory inventory;
    private CharacterController controller;
    private Vector3 playerVelocity;
    private bool isGrounded;
    public bool crouching;
    public bool sprinting;
    public int sprintSpeed;
    public int walkSpeed;
    public bool lerpCrouch;
    public float crouchTimer;
    public float gravity = -9.8f;
    public float speed = 5f;
    public float jumpHeight = 3f;

    void Awake()
    {
        controller = GetComponent<CharacterController>();
        inventory = player != null ? player.GetComponent<Inventory>() : GetComponent<Inventory>();

    }
    void Update()
    {
        isGrounded = controller.isGrounded;

        crouchTimer += Time.deltaTime;
        float p = crouchTimer / 1;
        p *= p;
        if (crouching)
        {
            controller.height = Mathf.Lerp(controller.height, 1, p);

        }
        else
        {
            controller.height = Mathf.Lerp(controller.height, 2, p);
        }

        if(p > 1)
        {
            lerpCrouch = false;
            crouchTimer = 0f;
        }
        
    }

    //receive the inputs for our InputManager.cs and apply them to our character controller.
    public void ProcessMove(Vector2 input)
    {
        Vector3 moveDirection = Vector3.zero;
        moveDirection.x = input.x;
        moveDirection.z = input.y;
        controller.Move(transform.TransformDirection(moveDirection) * speed * Time.deltaTime);
        playerVelocity.y += gravity * Time.deltaTime;
        if (isGrounded && playerVelocity.y < 0)
        {
            playerVelocity.y = -2f;
        }
        controller.Move(playerVelocity * Time.deltaTime);
        //Debug.Log(playerVelocity.y);
    }

    public void Jump()
    {
        if (isGrounded)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -3f * gravity);
        }
    }

    public void Crouch(bool value)
    {
        if (crouching == value)
        {
            return;
        }
        crouching = value;
        crouchTimer = 0f;
        lerpCrouch = true;
    }

    public void Sprint(bool value)
    {
        sprinting = value;
        speed = sprinting ? sprintSpeed : walkSpeed;

    }

    /// <summary>
    /// delta > 0 => move up/right; delta < 0 => move down/left
    /// </summary>
    public void Scroll(float delta)
    {
        if (inventory == null) return;

        // Add a small deadzone so tiny mouse-wheel values or drifts don’t spam
        if (delta > 0.1f)
            inventory.MoveSelection(+1);
        else if (delta < -0.1f)
            inventory.MoveSelection(-1);
    }
    

}

