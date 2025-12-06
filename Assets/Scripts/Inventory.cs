using UnityEngine;
using System;
using System.Collections.Generic;

public class Inventory : MonoBehaviour
{
    [SerializeField] private int capacity = 4;

    [SerializeField] private readonly List<Item> _items = new List<Item>();
   
    public int SelectedIndex { get; private set; } = -1;

    public event Action OnInventoryChanged;
    public event Action OnSelectionChanged;

    public int Capacity => capacity;
    public int Count => _items.Count;

    public IReadOnlyList<Item> Items => _items;

    public virtual Item GetSelected()
    {
        if (_items.Count == 0 || SelectedIndex < 0)
        {
            return null;
        }
        return _items[SelectedIndex];
    }

    public virtual void ClearSelectionIfInvalid()
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
    public virtual Item AddOrReplaceAtSelection(Item newItem)
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
        // if (SelectedIndex < 0)
        // {
        //   SelectedIndex = 0;  
        // } 
        Item replaced = _items[SelectedIndex];
        _items[SelectedIndex] = newItem;
        OnInventoryChanged?.Invoke();
        OnSelectionChanged?.Invoke();
        return replaced;
    }

    public virtual void RemoveAt(int index)
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
    public virtual void MoveSelection(int delta)
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
                SoundManager.Instance?.PlaySFX("food_consumed");
                player.changeHunger(amount);
                Debug.Log($"Snack restored {amount} hunger.");
                break;
            case Item.ItemType.drug:
                SoundManager.Instance?.PlaySFX("drug_consumed");
                player.changeHigh(amount);
                Debug.Log($"Drug restored {amount} high.");
                // Blue flash when using drugs
                FlashManager.Instance?.FlashDrug();
                // If there is an active Manananggal, despawn it when the player uses a drug.
                var monster = UnityEngine.Object.FindAnyObjectByType<Manananggal>();
                if (monster != null)
                {
                    monster.DespawnFromDrug();
                }
                break;
            default:
                return;
            
        }

        // remove from inventory & destroy (consume)
        int idx = SelectedIndex;
        RemoveAt(idx);
        Destroy(item.gameObject);
    }

       /// <summary>
    /// Drop the currently selected item into the world in front of the player.
    /// </summary>
    public void DropSelected(Player player)
    {
        Item item = GetSelected();
        if (item == null)
        {
            Debug.Log("No item selected to drop.");
            return;
        }

        // Remove from inventory list
        int idx = SelectedIndex;
        _items.RemoveAt(idx);
        ClearSelectionIfInvalid();
        OnInventoryChanged?.Invoke();
        OnSelectionChanged?.Invoke();

        // Place in world in front of player
        Vector3 dropPos =
            player.transform.position +
            player.transform.forward * 1.2f +
            Vector3.up * 0.5f;

        item.transform.SetParent(null);
        item.transform.position = dropPos;
        item.gameObject.SetActive(true);


        Debug.Log($"Dropped item: {item.name}");
    }

}
