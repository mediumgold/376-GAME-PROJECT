using UnityEngine;
using UnityEngine.UIElements;

public class GameUIHandler : MonoBehaviour
{
    public Player player;
    public UIDocument UIDoc;
    private Label hungerLabel;
    private Label highLabel;
    private Label moneyLabel;

    VisualElement hungerFill;
    VisualElement highFill;
    VisualElement inventoryContainer;

    private Inventory inventory;


    private void Start()
    {
        var root = UIDoc.rootVisualElement;

        hungerLabel = root.Q<Label>("hungerLabel");
        highLabel = root.Q<Label>("highLabel");
        moneyLabel = root.Q<Label>("moneyLabel");

        hungerFill = root.Q<VisualElement>("hungerBarFill");
        highFill = root.Q<VisualElement>("highBarFill");
        inventoryContainer = root.Q<VisualElement>("inventoryContainer");

        inventory = player != null ? player.GetComponent<Inventory>() : GetComponent<Inventory>();
        if (inventory != null)
        {
            inventory.OnInventoryChanged += UpdateInventoryUI;
            inventory.OnSelectionChanged += UpdateInventoryUI;
        }

        player.OnHungerChange += UpdateHungerUI;
        player.OnHighChange += UpdateHighUI;
        player.OnMoneyChange += UpdateMoneyUI;


        UpdateHungerUI();
        UpdateHighUI();
        UpdateMoneyUI();



    }

    void OnDestroy()
    {
        if (inventory != null)
        {
            inventory.OnInventoryChanged -= UpdateInventoryUI;
            inventory.OnSelectionChanged -= UpdateInventoryUI;
        }
    }

    void UpdateHungerUI()
    {
        float pct = player.maxHunger > 0 ? player.m_currentHunger / player.maxHunger : 0f;
        if (hungerLabel != null)
        {
            hungerLabel.text = $"{player.CurrentHunger}/{player.maxHunger}";
        }

        if (hungerFill != null)
        {
            hungerFill.style.width = Length.Percent(Mathf.Clamp01(pct) * 100f);
        }

    }

    void UpdateHighUI()
    {
        float pct = player.maxHigh > 0 ? player.m_currentHigh / player.maxHigh : 0f;
        //Debug.Log($"High % = {pct*100f:0.00}%");

        if (highLabel != null)
        {
            highLabel.text = $"{player.CurrentHigh}/{player.maxHigh}";
        }

        if (highFill != null)
        {
            highFill.style.width = Length.Percent(Mathf.Clamp01(pct) * 100f);

        }

    }
    void UpdateMoneyUI()
    {
        if (moneyLabel != null)
        {
            moneyLabel.text = $"${player.money}";
        }
    }

    private void UpdateInventoryUI()
    {
        if (inventoryContainer == null) return;
        if (inventory == null) return;

        var selected = inventory.GetSelected();
        if (selected == null)
        {
            inventoryContainer.style.backgroundImage = null;
            return;
        }

        SpriteRenderer preview = selected.GetComponentInChildren<SpriteRenderer>();
        

        if (preview != null && preview.sprite != null)
        {
            Texture2D tex = preview.sprite.texture;
            inventoryContainer.style.backgroundImage = new StyleBackground(tex);
        }
        else
        {
            // Clear if nothing
            inventoryContainer.style.backgroundImage = null;
        }
    }
}
