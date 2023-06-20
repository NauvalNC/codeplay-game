using System.Collections.Generic;
using UnityEngine;

public class InventoryManager : MonoBehaviour
{
    private static InventoryManager instance;
    public static InventoryManager Instance
    {
        get
        {
            if (!instance) instance = FindObjectOfType<InventoryManager>();
            return instance;
        }
    }

    private Dictionary<string, Item> inventory = new Dictionary<string, Item>();

    private List<Item> levelItems = new List<Item>();

    private void Awake()
    {
        levelItems.AddRange(FindObjectsOfType<Item>());
    }

    public void AddItem(Item item)
    {
        Item tItem = FindItem(item.ItemID);
        if (!tItem)
        {
            inventory.Add(item.ItemID, item);
            return;
        }

        tItem.AddItem(item.Quantity);
    }

    public bool ConsumeItem(string itemID)
    {
        Item tItem = FindItem(itemID);
        if (!tItem)
        {
            Debug.LogWarning("Cannot consume item. Item ID is not valid.");
            return false;
        }

        tItem.ConsumeItem();
        if (tItem.Quantity <= 0) 
        {
            inventory.Remove(itemID);
        }

        return true;
    }

    public Item FindItem(string itemID)
    {
        if (!inventory.ContainsKey(itemID)) return null;
        return inventory[itemID];
    }

    public void ClearItems()
    {
        Debug.Log("Inventory items cleared.");
        inventory.Clear();
    }

    public void ResetLevelItems()
    {
        foreach(Item tItem in levelItems)
        {
            tItem.ResetItem();
        }

        Debug.Log("Level items are reset.");
    }
}