using UnityEngine;
using UnityEngine.UIElements;
using System.Collections.Generic;

public class ShopUI : MonoBehaviour
{
    public static ShopUI Instance { get; private set; }

    private UIDocument doc;
    private VisualElement root;

    // Windows / views
    private VisualElement buyWindow;
    private VisualElement sellWindow;

    // Buttons
    private Button[] buyButtons;
    private Button[] sellButtons;

    // Text labels (separate for buy / sell)
    private Label buyMiddleText;
    private Label sellMiddleText;

    private InputManager input;

    // Data/state
    private List<Item> currentItems = new List<Item>();
    private int selectedIndex = -1;
    private bool confirmMode = false;

    private const string SelectedClass = "shop-selected";

    private enum ShopMode
    {
        None,
        Buy,
        Sell
    }

    private ShopMode mode = ShopMode.None;

    private void Awake()
    {
        Instance = this;
        doc = GetComponent<UIDocument>();
        input = FindAnyObjectByType<InputManager>();
    }

    private void Start()
    {
        root = doc.rootVisualElement;

        // Windows
        buyWindow  = root.Q<VisualElement>("MiddleWindow-buy");
        sellWindow = root.Q<VisualElement>("MiddleWindow-sell");

        // Middle text labels (your new names)
        buyMiddleText  = root.Q<Label>("MiddleText-buy");
        sellMiddleText = root.Q<Label>("MiddleText-sell");

        // BUY buttons (3x2 grid)
        buyButtons = new Button[6];
        for (int i = 0; i < 6; i++)
        {
            int idx = i;
            buyButtons[i] = root.Q<Button>($"buyButton{i}");
            if (buyButtons[i] != null)
            {
                buyButtons[i].RegisterCallback<ClickEvent>(_ => OnButtonClicked(idx));
            }
        }

        // SELL buttons (2x2 grid)
        sellButtons = new Button[4];
        for (int i = 0; i < 4; i++)
        {
            int idx = i;
            sellButtons[i] = root.Q<Button>($"sellButton{i}");
            if (sellButtons[i] != null)
            {
                sellButtons[i].RegisterCallback<ClickEvent>(_ => OnButtonClicked(idx));
            }
        }

        HideAll();
    }

    private void HideAll()
    {
        if (buyWindow  != null) buyWindow.style.display  = DisplayStyle.None;
        if (sellWindow != null) sellWindow.style.display = DisplayStyle.None;
    }

    // ---------- Helpers to pick active controls ----------

    private Button[] GetActiveButtons()
    {
        return mode == ShopMode.Buy ? buyButtons :
               mode == ShopMode.Sell ? sellButtons :
               null;
    }

    private Label GetActiveMiddleText()
    {
        return mode == ShopMode.Buy ? buyMiddleText :
               mode == ShopMode.Sell ? sellMiddleText :
               null;
    }

    // ---------- PUBLIC ENTRY POINTS ----------

    public void ShowBuy(List<Item> items)
    {
        mode = ShopMode.Buy;
        HideAll();

        if (buyWindow != null)
            buyWindow.style.display = DisplayStyle.Flex;

        LoadItems(items);
    }

    public void ShowSell(List<Item> items)
    {
        mode = ShopMode.Sell;
        HideAll();

        if (sellWindow != null)
            sellWindow.style.display = DisplayStyle.Flex;

        LoadItems(items);
    }

    private void LoadItems(List<Item> items)
    {
        currentItems = items ?? new List<Item>();
        selectedIndex = currentItems.Count > 0 ? 0 : -1;
        confirmMode = false;

        RefreshButtons();
        UpdateSelectionVisual();
        UpdateDescription();
    }

    // ---------- INPUT HOOKS FROM InputManager ----------

    public void MoveSelection(Vector2 dir)
    {
        if (currentItems == null || currentItems.Count == 0)
            return;

        if (selectedIndex < 0)
            selectedIndex = 0;

        int cols, rows;

        if (mode == ShopMode.Buy)
        {
            cols = 3; rows = 2;   // 3x2 grid
        }
        else if (mode == ShopMode.Sell)
        {
            cols = 2; rows = 2;   // 2x2 grid
        }
        else
        {
            return;
        }

        int row = selectedIndex / cols;
        int col = selectedIndex % cols;

        if (dir.x > 0.5f)       col = (col + 1) % cols;
        else if (dir.x < -0.5f) col = (col - 1 + cols) % cols;
        else if (dir.y > 0.5f)  row = (row - 1 + rows) % rows;
        else if (dir.y < -0.5f) row = (row + 1) % rows;

        int newIndex = row * cols + col;
        if (newIndex >= currentItems.Count)
            newIndex = currentItems.Count - 1;

        selectedIndex = newIndex;
        confirmMode = false;

        UpdateSelectionVisual();
        UpdateDescription();
    }

    public void OnConfirmInput()
    {
        if (currentItems == null || currentItems.Count == 0)
            return;

        if (selectedIndex < 0 || selectedIndex >= currentItems.Count)
            return;

        if (!confirmMode)
        {
            confirmMode = true;
            UpdateDescription();
        }
        else
        {
            if (mode == ShopMode.Buy)
                ConfirmPurchase_Buy();
            else if (mode == ShopMode.Sell)
                ConfirmPurchase_Sell();
        }
    }

    public void HandleCloseInput()
    {
        if (confirmMode)
        {
            confirmMode = false;
            UpdateDescription();
        }
        else
        {
            CloseShop();
        }
    }

    // ---------- INTERNAL HELPERS ----------

    private void OnButtonClicked(int index)
    {
        if (currentItems == null || index < 0 || index >= currentItems.Count)
            return;

        if (selectedIndex != index)
        {
            selectedIndex = index;
            confirmMode = false;
            UpdateSelectionVisual();
            UpdateDescription();
        }
        else
        {
            OnConfirmInput();
        }
    }

    private void RefreshButtons()
    {
        Button[] btns = GetActiveButtons();
        if (btns == null) return;

        for (int i = 0; i < btns.Length; i++)
        {
            if (btns[i] == null) continue;

            if (i >= currentItems.Count || currentItems[i] == null)
            {
                btns[i].style.backgroundImage = null;
                continue;
            }

            SpriteRenderer sr = currentItems[i].GetComponentInChildren<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
            {
                Texture2D tex = sr.sprite.texture;
                btns[i].style.backgroundImage = new StyleBackground(tex);
            }
            else
            {
                btns[i].style.backgroundImage = null;
            }
        }
    }

    private void UpdateSelectionVisual()
    {
        Button[] btns = GetActiveButtons();
        if (btns == null) return;

        for (int i = 0; i < btns.Length; i++)
        {
            if (btns[i] == null) continue;

            btns[i].RemoveFromClassList(SelectedClass);

            if (i == selectedIndex)
            {
                btns[i].AddToClassList(SelectedClass);
            }
        }
    }

    private void UpdateDescription()
    {
        Label middleText = GetActiveMiddleText();
        if (middleText == null)
            return;

        if (currentItems == null || currentItems.Count == 0 ||
            selectedIndex < 0 || selectedIndex >= currentItems.Count)
        {
            middleText.text = string.Empty;
            return;
        }

        Item item = currentItems[selectedIndex];

        if (!confirmMode)
        {
            string desc = item.description;
            if (string.IsNullOrEmpty(desc))
                desc = item.name;

            if (mode == ShopMode.Buy)
                middleText.text = $"{desc}  |  Buy: ${item.buyPrice}";
            else if (mode == ShopMode.Sell)
                middleText.text = $"{desc}  |  Sell: ${item.sellValue}";
            else
                middleText.text = desc;
        }
        else
        {
            if (mode == ShopMode.Buy)
                middleText.text = "Confirm buy";
            else if (mode == ShopMode.Sell)
                middleText.text = "Confirm sell";
            else
                middleText.text = "Confirm?";
        }
    }

    // ---------- BUY LOGIC ----------

    private void ConfirmPurchase_Buy()
    {
        if (currentItems == null || selectedIndex < 0 || selectedIndex >= currentItems.Count)
            return;

        Item item = currentItems[selectedIndex];
        if (item == null) return;

        var player = input.Player;
        var inv    = input.PlayerInventory;

        if (player == null || inv == null)
            return;

        int price = item.buyPrice;

        // inventory full
        if (inv.Count >= inv.Capacity)
        {
            Label mt = GetActiveMiddleText();
            if (mt != null) mt.text = "no more space in inventory";
            confirmMode = false;
            return;
        }

        // not enough money
        if (player.money < price)
        {
            Label mt = GetActiveMiddleText();
            if (mt != null) mt.text = "not enough money";
            confirmMode = false;
            return;
        }

        // charge player
        player.changeMoney(-price);

        // move item to player inventory
        item.transform.SetParent(player.transform);
        item.gameObject.SetActive(false);
        inv.AddOrReplaceAtSelection(item);

        Debug.Log($"Player bought: {item.name} for ${price}");

        // remove from shop list
        currentItems.RemoveAt(selectedIndex);

        if (currentItems.Count == 0)
        {
            CloseShop();
            return;
        }

        if (selectedIndex >= currentItems.Count)
            selectedIndex = currentItems.Count - 1;

        confirmMode = false;

        RefreshButtons();
        UpdateSelectionVisual();
        UpdateDescription();
    }

    // ---------- SELL LOGIC ----------

    private void ConfirmPurchase_Sell()
    {
        if (currentItems == null || selectedIndex < 0 || selectedIndex >= currentItems.Count)
            return;

        Item item = currentItems[selectedIndex];
        if (item == null) return;

        var player = input.Player;
        var inv    = input.PlayerInventory;

        if (player == null || inv == null)
            return;

        int value = item.sellValue;

        // pay player
        player.changeMoney(value);
        Debug.Log($"Player sold: {item.name} for ${value}");

        // remove from player inventory
        int invIndex = -1;
        var itemsRO = inv.Items;
        for (int i = 0; i < itemsRO.Count; i++)
        {
            if (itemsRO[i] == item)
            {
                invIndex = i;
                break;
            }
        }

        if (invIndex >= 0)
            inv.RemoveAt(invIndex);

        // destroy item object
        Destroy(item.gameObject);

        // remove from current sell list
        currentItems.RemoveAt(selectedIndex);

        if (currentItems.Count == 0)
        {
            CloseShop();
            return;
        }

        if (selectedIndex >= currentItems.Count)
            selectedIndex = currentItems.Count - 1;

        confirmMode = false;

        RefreshButtons();
        UpdateSelectionVisual();
        UpdateDescription();
    }

    // ---------- CLOSE ----------

    private void CloseShop()
    {
        HideAll();
        mode = ShopMode.None;
        currentItems = new List<Item>();
        selectedIndex = -1;
        confirmMode = false;

        UpdateSelectionVisual();

        if (buyMiddleText  != null) buyMiddleText.text  = string.Empty;
        if (sellMiddleText != null) sellMiddleText.text = string.Empty;

        if (input != null)
            input.SetShopMode(false);
    }
}
