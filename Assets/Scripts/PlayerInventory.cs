using UnityEngine;
using System.Collections.Generic;

public class PlayerInventory : MonoBehaviour
{
    private HashSet<string> items = new HashSet<string>();

    public void AddItem(string itemName)
    {
        items.Add(itemName);
        Debug.Log($"{gameObject.name} - Inventaire: {itemName} ajouté");
    }

    public bool HasItem(string itemName)
    {
        return items.Contains(itemName);
    }

    public void RemoveItem(string itemName)
    {
        items.Remove(itemName);
        Debug.Log($"{gameObject.name} - Inventaire: {itemName} retiré");
    }
}
