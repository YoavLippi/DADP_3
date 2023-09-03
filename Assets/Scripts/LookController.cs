using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class LookController : MonoBehaviour
{
    [SerializeField] private Camera lookCamera;
    
    [SerializeField] private Transform cursorTf;

    [SerializeField] private Image crosshair;

    [SerializeField] private Rigidbody testBody;

    [SerializeField] private ShopController shop;

    [SerializeField] private InventoryController playerInventory;

    [SerializeField] private HotbarController hotbar;

    private float lookSens;
    private bool invertX, invertY;
    private Space baseLook;
    private Transform bodyTF;
    private bool lookActive;
    private bool carryingItem;
    private GameObject carriedItem;
    private Color crosshairBaseColor;
    

    public Transform BodyTf
    {
        get => bodyTF;
        set => bodyTF = value;
    }

    public Transform CursorTf
    {
        get => cursorTf;
        set => cursorTf = value;
    }

    public bool InvertX
    {
        get => invertX;
        set => invertX = value;
    }

    public bool InvertY
    {
        get => invertY;
        set => invertY = value;
    }

    public float LookSens
    {
        get => lookSens;
        set => lookSens = value;
    }

    void Start()
    {
        lookCamera.transform.position = transform.position;
        Cursor.lockState = CursorLockMode.Locked;
        lookActive = true;
        carryingItem = false;
        crosshairBaseColor = crosshair.color;
    }
    
    void Update()
    {
        if (lookActive)
        {
            Vector3 thisPos = transform.position;
            Debug.DrawRay(thisPos, cursorTf.position-thisPos, Color.green);
            //Debug.DrawLine(transform.position, cursorTf.position, Color.red);
            lookCamera.transform.position = thisPos;
        
            float rayDist = 4f;
            RaycastHit hit;
            int mask = 1 << LayerMask.NameToLayer("Movables") | 1 << LayerMask.NameToLayer("LevelGeometry") | 1 << LayerMask.NameToLayer("Interactibles");
            float castRadius = 0.1f;
            if (carryingItem)
            {
                castRadius = GetMaxComponent(carriedItem.GetComponentInChildren<MeshRenderer>().bounds.size)/2f;
                //Debug.Log(castRadius);
            }
            
            if (Physics.SphereCast(thisPos, castRadius, cursorTf.position - thisPos, out hit, rayDist, mask))
            {
                //Debug.Log($"Hit {hit.collider.name}");
                float offset = Mathf.Clamp(Vector3.Distance(thisPos, hit.point), 0f, rayDist);
                cursorTf.position = thisPos;
                cursorTf.Translate(0,0,offset);
                /*if (carryingItem)
                {
                    cursorTf.Translate(hit.normal.normalized*castRadius);
                }*/
                if (hit.transform.gameObject.CompareTag("movables"))
                {
                    crosshair.color = new Color(crosshairBaseColor.r+10f, crosshairBaseColor.g+10f, crosshairBaseColor.b+10f);
                }
            }
            else
            {
                if (!carryingItem)
                {
                    crosshair.color = crosshairBaseColor;
                }
                if (Math.Abs((thisPos-cursorTf.position).magnitude - rayDist) > 0.1f)
                {
                    cursorTf.position = thisPos;
                    cursorTf.Translate(0,0,rayDist);
                }
            }
            
            //Code for picking up and placing items
            if (carryingItem)
            {
                if (Input.GetKeyDown(KeyCode.Mouse0) && carriedItem.GetComponent<MovableObject>().Placeable)
                {
                    float throwForce = 300f;
                    carriedItem.GetComponent<Rigidbody>().isKinematic = false;
                    carriedItem.GetComponent<Rigidbody>().AddForce((cursorTf.position - thisPos).normalized*throwForce, ForceMode.Acceleration);
                    DoInteract(false);
                }
            }

            var mouseDelta = new Vector2(Input.GetAxis("Mouse X"), -Input.GetAxis("Mouse Y")) * lookSens;
            //x corresponds to y rotation, y corresponds to x rotation 
            if (mouseDelta != Vector2.zero)
            {
                mouseDelta.x = invertX ? -mouseDelta.x : mouseDelta.x;
                mouseDelta.y = invertY ? -mouseDelta.y : mouseDelta.y;
            
                transform.Rotate(mouseDelta.y, mouseDelta.x, 0f, Space.Self);
            
                var eulerRotation = transform.rotation.eulerAngles;
                eulerRotation.x = ClampAngle(eulerRotation.x, 280f, 80f);
                transform.rotation = Quaternion.Euler(eulerRotation.x, eulerRotation.y, 0f);
                bodyTF.rotation = Quaternion.Euler(0f, eulerRotation.y, 0f);
            }

            lookCamera.transform.rotation = transform.rotation;
        }
    }

    public static float GetMaxComponent(Vector3 input)
    {
        float max = MaxAB(input.x, input.y);
        return max > input.z ? max : input.z;
    }

    private static float MaxAB(float a, float b)
    {
        return a > b ? a : b;
    }

    public void DoInteract(bool pickup)
    {
        LayerMask excludedLayers = 1 << LayerMask.NameToLayer("Interactibles") |
                                   1 << LayerMask.NameToLayer("LevelGeometry") | 1 << LayerMask.NameToLayer("Player");
        if (!carryingItem)
        {
            float rayDist = 4f;
            RaycastHit hit;
            int mask = 1 << LayerMask.NameToLayer("Movables") | 1 << LayerMask.NameToLayer("LevelGeometry") | 1 << LayerMask.NameToLayer("Interactibles");
            if (Physics.SphereCast(transform.position, 0.1f, cursorTf.position - transform.position, out hit, rayDist, mask))
            {
                //making sure you can only interact with movables even when the raycast has something in the way
                if (hit.transform.gameObject.CompareTag("movables"))
                {
                    if (hit.transform.GetComponent<MovableObject>().ShopItem)
                    {
                        shop.SellItem(hit.transform.GetComponent<MovableObject>());
                    }
                    else if (pickup)
                    {
                        playerInventory.AddToInventory(hit.transform.gameObject.GetComponent<MovableObject>(), hotbar.CurrentSelection);
                        Destroy(hit.transform.gameObject);
                    } else
                    {
                        carryingItem = true;
                        carriedItem = hit.transform.gameObject;
                        carriedItem.GetComponent<Collider>().excludeLayers = excludedLayers;
                        carriedItem.GetComponent<MovableObject>().EnterPlacementMode();
                        carriedItem.GetComponent<Rigidbody>().isKinematic = true;
                        carriedItem.transform.position = cursorTf.position;
                        carriedItem.transform.SetParent(cursorTf);
                        carriedItem.layer = LayerMask.NameToLayer("Ignore Raycast");
                    }
                }
            }
        }
        else
        {
            if (carriedItem.GetComponent<MovableObject>().Placeable)
            {
                carryingItem = false;
                carriedItem.layer = LayerMask.NameToLayer("Movables");
                carriedItem.GetComponent<Collider>().excludeLayers = LayerMask.NameToLayer("UI");
                carriedItem.GetComponent<MovableObject>().ExitPlacementMode();
                carriedItem.GetComponent<Rigidbody>().isKinematic = false;
                carriedItem.transform.SetParent(null);
                carriedItem.transform.position = cursorTf.position;
                carriedItem = null;
            }
        }
    }

    public void Pause(bool yesNo)
    {
        lookActive = !yesNo;
    }
    
    // Custom clamping function for angles
    float ClampAngle(float angle, float min, float max)
    {
        if (angle < -360f)
        {
            angle += 360f;
        }
        if (angle > 360f)
        {
            angle -= 360f;
        }
    
        // Adjust the min and max angles to be within the 0-360 range
        min = Mathf.Repeat(min, 360f);
        max = Mathf.Repeat(max, 360f);
    
        if (max < min)
        {
            if (angle > max && angle < min)
            {
                if (min - angle <= 30)
                {
                    angle = min;
                }
                else
                {
                    angle = max;
                }
            }
        }
        else
        {
            if (angle < min)
            {
                angle = min;
            }
            else if (angle > max)
            {
                angle = max;
            }
        }
    
        return angle;
    }
}
