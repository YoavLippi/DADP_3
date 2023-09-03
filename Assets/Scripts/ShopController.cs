using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopController : MonoBehaviour
{
    [SerializeField] private List<GameObject> shopInventory;

    [SerializeField] private InventoryController playerInventory;

    [SerializeField] private int placedItemValue;

    public void SellItem(MovableObject item)
    {
        if (item.Value <= placedItemValue)
        {
            playerInventory.AddToInventory(item);
        }
    }

    public void DisplayItems()
    {
        
    }
}
