using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HotbarController : MonoBehaviour
{
    [SerializeField] private List<GameObject> hotbarSlots;
    [SerializeField] private List<Image> slotImages;
    [SerializeField] private GameObject selectionBox;
    [SerializeField] private int currentSelection;

    public int CurrentSelection
    {
        get => currentSelection;
        set => currentSelection = value;
    }

    private static KeyCode[] NumKeys = new[]
    {
        KeyCode.Alpha0, 
        KeyCode.Alpha1, 
        KeyCode.Alpha2, 
        KeyCode.Alpha3, 
        KeyCode.Alpha4, 
        KeyCode.Alpha5,
        KeyCode.Alpha6,
        KeyCode.Alpha7,
        KeyCode.Alpha8,
        KeyCode.Alpha9
    };

    private void Awake()
    {
        hotbarSlots = new List<GameObject>();
        slotImages = new List<Image>();
        foreach (var slot in GameObject.FindGameObjectsWithTag("Hotbar"))
        {
            hotbarSlots.Add(slot);
        }

        foreach (var slotImage in GameObject.FindGameObjectsWithTag("ItemDisplay"))
        {
            slotImages.Add(slotImage.GetComponent<Image>());
        }
        SortSlots();
        SetSelection(1);
    }

    private void SortSlots()
    {
        for (int i = 0; i < hotbarSlots.Count; i++)
        {
            for (int j = i; j < hotbarSlots.Count; j++)
            {
                if (GetSlotVal(hotbarSlots[i].name) > GetSlotVal(hotbarSlots[j].name))
                {
                    (hotbarSlots[i], hotbarSlots[j]) = (hotbarSlots[j], hotbarSlots[i]);
                    //(slotImages[i], slotImages[j]) = (slotImages[j], slotImages[i]);
                }
                if (GetSlotVal(slotImages[i].name) > GetSlotVal(slotImages[j].name))
                {
                    (slotImages[i], slotImages[j]) = (slotImages[j], slotImages[i]);
                }
            }
        }
    }

    public void UpdateHotbar(MovableObject[] currentInventory)
    {
        //Debug.Log("Called");
        for (int i = 0; i < 10; i++)
        {
            //if (slotImages[i].sprite) Debug.Log(i);
            if (currentInventory[i] != null)
            {
                slotImages[i].sprite = currentInventory[i].HotbarImage;
                slotImages[i].color = Color.white;
            }
            else
            {
                slotImages[i].sprite = null;
                slotImages[i].color = Color.clear;
            }
        }
    }

    private int GetSlotVal(string input)
    {
        char[] output = input.Substring(input.IndexOf(' ') + 1).ToCharArray();
        return output[0];
    }

    public void Scroll(float scrollData)
    {
        int dir = scrollData > 0 ? -1 : 1;
        Loop(dir);
    }

    //numTimes can only be +-1
    private void Loop(int direction)
    {
        currentSelection += direction;
        if (currentSelection > 9) currentSelection = 0;
        if (currentSelection < 0) currentSelection = 9;
        SetSelection(currentSelection);
    }

    private void Update()
    {
        for (int i = 0; i < 10; i++)
        {
            if (Input.GetKeyDown(NumKeys[i]))
            {
                SetSelection(i);
            }
        }
    }

    public void SetSelection(int slotNum)
    {
        currentSelection = slotNum;
        selectionBox.transform.position = hotbarSlots[slotNum].transform.position;
    }
}
