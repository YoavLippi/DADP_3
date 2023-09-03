using System;
using System.Collections;
using System.Collections.Generic;
using Object_Behaviours;
using Unity.VisualScripting;
using UnityEngine;

public class InventoryController : MonoBehaviour
{
    [SerializeField] private List<MovableObject> StartingItems;
    
    [SerializeField] private MovableObject[] heldItems;

    [SerializeField] private Transform cursorPos;

    [SerializeField] private HotbarController hotbar;

    public MovableObject[] HeldItems
    {
        get => heldItems;
        set => heldItems = value;
    }

    private void Start()
    {
        for (int i = 0; i < 10; i++)
        {
            if (i < 5)
            {
                AddToInventory(StartingItems[0]);
            }
            else
            {
                AddToInventory(StartingItems[1]);
            }
        }
    }

    private void Awake()
    {
        StartingItems = new List<MovableObject>();
        foreach (var prefab in Resources.LoadAll<GameObject>("Prefabs"))
        {
            if (prefab.GetComponent<MovableObject>())
            {
                StartingItems.Add(prefab.GetComponent<MovableObject>());
            }
        }

        HeldItems = new MovableObject[10];
    }

    public void AddToInventory(MovableObject newObj)
    {
        foreach (var m in StartingItems)
        {
            if (newObj.itemID == m.itemID)
            {
                for (int i = 0; i < 10; i++)
                {
                    if (heldItems[i] == null)
                    {
                        heldItems[i] = m;
                        break;
                    }
                }

                break;
            }
        }
        hotbar.UpdateHotbar(heldItems);
    }

    public void AddToInventory(MovableObject newObj, int slot)
    {
        foreach (var m in StartingItems)
        {
            if (newObj.itemID == m.itemID)
            {
                if (heldItems[slot] == null) {heldItems[slot] = m;}
                else {AddToInventory(newObj);}
            }
        }
        hotbar.UpdateHotbar(heldItems);
    }

    public void RemoveItem(int hotbarPos)
    {
        heldItems[hotbarPos] = null;
        hotbar.UpdateHotbar(heldItems);
    }

    public void PlaceItem(MovableObject obj,Vector3 position, int hotbarPos)
    {
        bool foundFlag = false;
        int index = 0;
        foreach (var item in heldItems)
        {
            if (item == obj && index == hotbarPos)
            {
                foundFlag = true;
                RemoveItem(hotbarPos);
                Instantiate(obj.ThisObject, position, Quaternion.identity);
                break;
            }

            index++;
        }

        if (!foundFlag)
        {
            throw new Exception("That item wasn't in your inventory... How'd you do that?");
        }
    }
}
