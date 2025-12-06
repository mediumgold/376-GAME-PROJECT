using UnityEngine;

public class InputManager : MonoBehaviour
{
    public static InputManager Instance { get; private set; }

    private PlayerInput playerInput;
    public PlayerInput.OnFootActions onFoot;

    private PlayerMotor motor;
    private PlayerLook look;
    private Inventory inventory;
    private Player player;
    private Manananggal mn;

    [SerializeField] private GameObject crossHair;

    private bool inDialogue;
    public bool InDialogue => inDialogue;

    private bool inOptionSelect;
    public bool InOptionSelect => inOptionSelect;

    private bool inShop;
    public bool InShop => inShop;

    private bool inEscapeChoice;
    public bool InEscapeChoice => inEscapeChoice;

    

    private float optionMoveCooldown = 0.2f;
    private float optionMoveTimer = 0f;

    private float shopMoveCooldown = 0.2f;
    private float shopMoveTimer = 0f;

    private float escapeMoveCooldown = 0.2f;
    private float escapeMoveTimer = 0f;

    private float interactCooldown = 0f;
    public bool InteractCooldownActive => interactCooldown > 0f;

    private bool lastBlockedState = false;

    public Inventory PlayerInventory => inventory;
    public Player Player => player;

    private bool hasWon;
    private bool hasLost;

public bool HasWon  => hasWon;
public bool HasLost => hasLost;


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        Debug.Log(Screen.width + "x" + Screen.height);

        playerInput = new PlayerInput();
        onFoot = playerInput.OnFoot;

        motor = GetComponent<PlayerMotor>();
        look = GetComponent<PlayerLook>();
        player = FindAnyObjectByType<Player>();
        inventory = player.GetComponent<Inventory>();
        mn = FindAnyObjectByType<Manananggal>();

        onFoot.Jump.performed   += ctx => { if (!IsBlocked()) motor.Jump(); };
        onFoot.Crouch.started   += ctx => { if (!IsBlocked()) motor.Crouch(true); };
        onFoot.Crouch.canceled  += ctx => { if (!IsBlocked()) motor.Crouch(false); };
        onFoot.Sprint.started   += ctx => { if (!IsBlocked()) motor.Sprint(true); };
        onFoot.Sprint.canceled  += ctx => { if (!IsBlocked()) motor.Sprint(false); };
        onFoot.Use.performed    += ctx => { if (!IsBlocked()) inventory.UseSelected(player); };
        onFoot.DEBUG.performed  += ctx => mn?.animDebug();

        onFoot.Drop.performed   += ctx =>
        {
            if (!IsBlocked())
                inventory.DropSelected(player);
        };

        onFoot.Close.performed  += ctx =>
        {
            if (inEscapeChoice)
            {
                // ignore Close in escape choice; player must choose
                return;
            }

            if (inShop && ShopUI.Instance != null)
            {
                ShopUI.Instance.HandleCloseInput();
            }
            else if (inOptionSelect || inDialogue)
            {
                if (DialogueUI.Instance != null)
                {
                    DialogueUI.Instance.HandleCloseFromInput();
                }
            }
        };
    }

    private bool IsBlocked()
    {
        return inDialogue || inOptionSelect || inShop || inEscapeChoice || InteractCooldownActive || hasLost || hasWon;
    }

    private void Update()
    {
        if (interactCooldown > 0f)
            interactCooldown -= Time.deltaTime;

        bool blocked = IsBlocked();
        if (blocked != lastBlockedState)
        {
            if (crossHair != null)
                crossHair.SetActive(!blocked);
            lastBlockedState = blocked;
        }

        // 1) Escape choice mode has highest priority
        if (inEscapeChoice && EscapeChoiceUI.Instance != null)
        {
            HandleEscapeChoiceInput();
            return;
        }

        // 2) Dialogue option-select mode
        if (inOptionSelect)
        {
            HandleOptionSelectInput();
        }

        // 3) Shop UI navigation
        if (inShop && ShopUI.Instance != null)
        {
            HandleShopInput();
        }
    }

    private void FixedUpdate()
    {
        if (IsBlocked())
            motor.ProcessMove(Vector2.zero);
        else
            motor.ProcessMove(onFoot.Movement.ReadValue<Vector2>());
    }

    private void LateUpdate()
    {
        if (IsBlocked()) return;

        look.ProcessLook(onFoot.Look.ReadValue<Vector2>());

        float scroll = onFoot.Scroll.ReadValue<float>();
        if (Mathf.Abs(scroll) > 0.1f)
            motor.Scroll(scroll);
    }

    private void OnEnable()  => onFoot.Enable();
    private void OnDisable() => onFoot.Disable();

    public void SetDialogueMode(bool active) => inDialogue = active;

    public void SetOptionSelectMode(bool active)
    {
        inOptionSelect = active;
        optionMoveTimer = 0f;
    }

    public void SetShopMode(bool active)
    {
        inShop = active;

    }

    public void SetEscapeChoiceMode(bool active)
    {
        inEscapeChoice = active;
        escapeMoveTimer = 0f;

    }

    public void TriggerInteractCooldown(float t = 0.2f)
    {
        interactCooldown = t;
    }

    private void HandleOptionSelectInput()
    {
        optionMoveTimer -= Time.deltaTime;

        Vector2 move = onFoot.Movement.ReadValue<Vector2>();

        if (optionMoveTimer <= 0f)
        {
            if (move.y > 0.5f)
            {
                DialogueUI.Instance.OptionSelect_Move(-1);
                optionMoveTimer = optionMoveCooldown;
            }
            else if (move.y < -0.5f)
            {
                DialogueUI.Instance.OptionSelect_Move(1);
                optionMoveTimer = optionMoveCooldown;
            }
        }

        if (onFoot.Interact.WasPerformedThisFrame())
        {
            DialogueUI.Instance.OptionSelect_Confirm();
        }
    }

    private void HandleShopInput()
    {
        shopMoveTimer -= Time.deltaTime;
        Vector2 move = onFoot.Movement.ReadValue<Vector2>();

        if (shopMoveTimer <= 0f)
        {
            if (move.x > 0.5f)
            {
                ShopUI.Instance.MoveSelection(Vector2.right);
                shopMoveTimer = shopMoveCooldown;
            }
            else if (move.x < -0.5f)
            {
                ShopUI.Instance.MoveSelection(Vector2.left);
                shopMoveTimer = shopMoveCooldown;
            }
            else if (move.y > 0.5f)
            {
                ShopUI.Instance.MoveSelection(Vector2.up);
                shopMoveTimer = shopMoveCooldown;
            }
            else if (move.y < -0.5f)
            {
                ShopUI.Instance.MoveSelection(Vector2.down);
                shopMoveTimer = shopMoveCooldown;
            }
        }

        if (!InteractCooldownActive && onFoot.Interact.WasPerformedThisFrame())
        {
            ShopUI.Instance.OnConfirmInput();
        }
    }

    private void HandleEscapeChoiceInput()
    {
        escapeMoveTimer -= Time.deltaTime;
        Vector2 move = onFoot.Movement.ReadValue<Vector2>();

        if (escapeMoveTimer <= 0f)
        {
            if (move.x > 0.5f)
            {
                EscapeChoiceUI.Instance.MoveSelection(-1);  // up
                escapeMoveTimer = escapeMoveCooldown;
            }
            else if (move.x < -0.5f)
            {
                EscapeChoiceUI.Instance.MoveSelection(1);   // down
                escapeMoveTimer = escapeMoveCooldown;
            }
        }

        if (onFoot.Interact.WasPerformedThisFrame())
        {
            EscapeChoiceUI.Instance.ConfirmSelection();
        }
    }

    public void SetWinState(bool value)
    {
        hasWon = value;
    }

    public void SetLoseState(bool value)
    {
        hasLost = value;
    }

}
