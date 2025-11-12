using UnityEngine;
using System;


public class InputManager : MonoBehaviour
{
    private PlayerInput playerInput;
    public PlayerInput.OnFootActions onFoot;

    private PlayerMotor motor;
    private PlayerLook look;

    private Manananggal mn;
    private Inventory inventory;
    private Player player;

    void Awake()
    {
        playerInput = new PlayerInput();
        mn = FindAnyObjectByType<Manananggal>();
        onFoot = playerInput.OnFoot;
        motor = GetComponent<PlayerMotor>();
        look = GetComponent<PlayerLook>();
        player = FindAnyObjectByType<Player>();
        inventory = player.GetComponent<Inventory>();
        onFoot.Jump.performed += ctx => motor.Jump();

        onFoot.Crouch.started += ctx => motor.Crouch(true);
        onFoot.Crouch.canceled += ctx => motor.Crouch(false);

        onFoot.Sprint.started += ctx => motor.Sprint(true);
        onFoot.Sprint.canceled += ctx => motor.Sprint(false);

        onFoot.Use.performed += ctx => inventory.UseSelected(player);


        onFoot.DEBUG.performed += ctx => mn.animDebug();
    }
    void Update()
    {

    }
    void FixedUpdate()
    {
        motor.ProcessMove(onFoot.Movement.ReadValue<Vector2>());
    }

    private void LateUpdate()
    {
        look.ProcessLook(onFoot.Look.ReadValue<Vector2>());       
        float scroll = onFoot.Scroll.ReadValue<float>();
        if (Mathf.Abs(scroll) > 0.1f)
        {
            motor.Scroll(scroll);
        }
    }

    private void OnEnable()
    {
        onFoot.Enable();
    }

    private void OnDisable()
    {
        onFoot.Disable();
    }
}

