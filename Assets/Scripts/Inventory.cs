using UnityEngine;
using System;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    [SerializeField] private int capacity = 4;

    [SerializeField] private readonly List<Item> _items = new List<Item>();
    public IReadOnlyList<Item> Items => _items;
    public int SelectedIndex { get; private set; } = -1;

    public event Action OnInventoryChanged;
    public event Action OnSelectionChanged;

    public int Capacity => capacity;
    public int Count => _items.Count;

    public Item GetSelected()
    {
        if (_items.Count == 0 || SelectedIndex < 0)
        {
            return null;
        }
        return _items[SelectedIndex];
    }

    public void ClearSelectionIfInvalid()
    {
        if (_items.Count == 0)
        {
            SelectedIndex = -1;
        }
        else
        {
            SelectedIndex = Mathf.Clamp(SelectedIndex, 0, _items.Count - 1);
        }
    }

    /// <summary>
    /// Add an item. If full, replace the currently selected item and return it (so caller can drop it).
    /// Returns null if not replacing.
    /// </summary>
    public Item AddOrReplaceAtSelection(Item newItem)
    {
        if (_items.Count < capacity)
        {
            _items.Add(newItem);
            if (SelectedIndex == -1)
            {
               SelectedIndex = 0; // first pickup selects index 0 
            } 
            OnInventoryChanged?.Invoke();
            OnSelectionChanged?.Invoke();
            return null;
        }

        // Full: replace the currently selected item
        if (SelectedIndex < 0)
        {
          SelectedIndex = 0;  
        } 
        Item replaced = _items[SelectedIndex];
        _items[SelectedIndex] = newItem;
        OnInventoryChanged?.Invoke();
        OnSelectionChanged?.Invoke();
        return replaced;
    }

    public void RemoveAt(int index)
    {
        if (index < 0 || index >= _items.Count)
        {
            return;
        } 
        _items.RemoveAt(index);
        ClearSelectionIfInvalid();
        OnInventoryChanged?.Invoke();
        OnSelectionChanged?.Invoke();
    }

    /// <summary>
    /// Move selection by delta (+1/-1), wrapping around.
    /// </summary>
    public void MoveSelection(int delta)
    {
        if (_items.Count == 0)
        {
            return;
        }
        SelectedIndex = (SelectedIndex + delta) % _items.Count;
        if (SelectedIndex < 0)
        {
            SelectedIndex += _items.Count;
        }
        OnSelectionChanged?.Invoke();
    }

    public void UseSelected(Player player)
    {
        Item item = GetSelected();
        if (item == null)
        {
            Debug.Log("No item selected.");
            return;
        }

        float amount = item.value;
        switch (item.itemType)
        {
            case Item.ItemType.snack:
                player.changeHunger(amount);
                Debug.Log($"Snack restored {amount} hunger.");
                break;
            case Item.ItemType.drug:
                player.changeHigh(amount);
                Debug.Log($"Drug restored {amount} high.");
                break;
            default:
                Debug.Log($"Item '{item.name}' has no use effect.");
                break;
        }

        // remove from inventory & destroy (consume)
        int idx = SelectedIndex;
        RemoveAt(idx);
        Destroy(item.gameObject);
    }

}
