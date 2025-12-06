using UnityEngine;
using System.Collections.Generic;

public class NPCInventory : MonoBehaviour
{
    [Header("POOL OF POSSIBLE ITEMS THIS NPC CAN SELL")]
    [SerializeField] private List<Item> itemPool = new List<Item>();

    [Header("Current Active Shop Items (Always 6 Max)")]
    [SerializeField] private List<Item> activeShopItems = new List<Item>(6);

    public List<Item> ActiveShopItems => activeShopItems;

    /// <summary>
    /// Returns true if inventory is empty (all items bought).
    /// </summary>
    public bool IsEmpty => activeShopItems.Count == 0;

    /// <summary>
    /// Only generates new inventory if current one is empty.
    /// Call this when opening the shop.
    /// </summary>
    public void GenerateShopInventoryIfEmpty()
    {
        // Clean up any null references (items that were bought)
        activeShopItems.RemoveAll(item => item == null);

        // Only regenerate if completely empty
        if (activeShopItems.Count > 0)
            return;

        for (int i = 0; i < 6; i++)
        {
            if (itemPool.Count == 0)
                break;

            Item random = itemPool[Random.Range(0, itemPool.Count)];

            // Instantiate as inactive so it doesn't appear in the world
            Item spawned = Instantiate(random);
            spawned.gameObject.SetActive(false);
            activeShopItems.Add(spawned);
        }
    }

    /// <summary>
    /// Force regenerate inventory (destroys existing items).
    /// </summary>
    public void ForceRegenerateInventory()
    {
        foreach (Item oldItem in activeShopItems)
        {
            if (oldItem != null)
                Destroy(oldItem.gameObject);
        }
        activeShopItems.Clear();
        GenerateShopInventoryIfEmpty();
    }
}
