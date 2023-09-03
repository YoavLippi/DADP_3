using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class MovableObject : MonoBehaviour
{
    [FormerlySerializedAs("weight")] [SerializeField] protected float mass;

    [SerializeField] protected float value;

    [SerializeField] protected Color itemColor;

    [SerializeField] protected MeshRenderer mRenderer;

    [SerializeField] protected Rigidbody rb;

    [SerializeField] protected bool placeable;

    [SerializeField] protected bool shopItem;

    [SerializeField] protected GameObject thisObject;

    [SerializeField] protected Sprite hotbarImage;

    public int itemID;

    protected bool inPlacementMode;

    public Sprite HotbarImage => hotbarImage;

    public GameObject ThisObject
    {
        get => thisObject;
        set => thisObject = value;
    }

    public bool ShopItem
    {
        get => shopItem;
        set => shopItem = value;
    }

    public bool Placeable
    {
        get => placeable;
        set => placeable = value;
    }

    public float Mass
    {
        get => mass;
        set => mass = value;
    }

    public float Value
    {
        get => value;
        set => this.value = value;
    }

    public Color ItemColor
    {
        get => itemColor;
        set => itemColor = value;
    }

    public virtual void Start()
    {
        mRenderer.material.color = itemColor;
        rb.mass = mass;
        placeable = true;
    }

    public virtual void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && inPlacementMode)
        {
            mRenderer.material.color = new Color(255f, 0f, 0f, 0.7f);
            placeable = false;
        }
    }

    public virtual void OnTriggerExit(Collider other)
    {
        if (other.gameObject.CompareTag("Player") && inPlacementMode)
        {
            mRenderer.material.color = new Color(itemColor.r, itemColor.g, itemColor.b, 0.7f);
            placeable = true;
        }
    }

    public virtual void EnterPlacementMode()
    {
        Debug.Log("Entered Placement");
        inPlacementMode = true;
        GetComponent<Collider>().enabled = false;
        mRenderer.material.color = new Color(itemColor.r, itemColor.g, itemColor.b, 0.7f);
    }

    public virtual void ExitPlacementMode()
    {
        inPlacementMode = false;
        GetComponent<Collider>().enabled = true;
        mRenderer.material.color = itemColor;
    }
}
