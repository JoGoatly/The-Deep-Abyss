using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [System.Serializable]
    public class InventoryItem
    {
        public string itemName;
        public int quantity;
        public Sprite icon;
    }

    [Header("Inventory Settings")]
    [Tooltip("Maximum number of different items the player can carry")]
    public int maxItems = 10;

    [Tooltip("List of items currently in the inventory")]
    public List<InventoryItem> items = new List<InventoryItem>();

    [Header("Events")]
    [Tooltip("Event that triggers when an item is added to the inventory")]
    public UnityEngine.Events.UnityEvent<string> onItemAdded;

    [Tooltip("Event that triggers when an item is removed from the inventory")]
    public UnityEngine.Events.UnityEvent<string> onItemRemoved;

    // Check if the player has a specific item
    public bool HasItem(string itemName)
    {
        foreach (InventoryItem item in items)
        {
            if (item.itemName == itemName && item.quantity > 0)
            {
                return true;
            }
        }
        return false;
    }

    // Get the quantity of a specific item
    public int GetItemQuantity(string itemName)
    {
        foreach (InventoryItem item in items)
        {
            if (item.itemName == itemName)
            {
                return item.quantity;
            }
        }
        return 0;
    }

    // Add an item to the inventory
    public bool AddItem(string itemName, int quantity = 1, Sprite icon = null)
    {
        // Check if the item already exists in the inventory
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].itemName == itemName)
            {
                // Item exists, increase quantity
                items[i].quantity += quantity;

                // Trigger event
                onItemAdded?.Invoke(itemName);

                return true;
            }
        }

        // Item doesn't exist, create a new entry if there's space
        if (items.Count < maxItems)
        {
            InventoryItem newItem = new InventoryItem
            {
                itemName = itemName,
                quantity = quantity,
                icon = icon
            };

            items.Add(newItem);

            // Trigger event
            onItemAdded?.Invoke(itemName);

            return true;
        }

        // Inventory is full
        Debug.Log("Inventory is full, can't add " + itemName);
        return false;
    }

    // Remove an item from the inventory
    public bool RemoveItem(string itemName, int quantity = 1)
    {
        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].itemName == itemName)
            {
                // Found the item, decrease quantity
                items[i].quantity -= quantity;

                // Remove the item if quantity is 0 or less
                if (items[i].quantity <= 0)
                {
                    items.RemoveAt(i);
                }

                // Trigger event
                onItemRemoved?.Invoke(itemName);

                return true;
            }
        }

        // Item not found
        return false;
    }

    // Clear the entire inventory
    public void ClearInventory()
    {
        items.Clear();
    }
}