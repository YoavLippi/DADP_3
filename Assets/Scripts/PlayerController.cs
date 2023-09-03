using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.Serialization;

public class PlayerController : MonoBehaviour
{
    [Header("Player setup")]
    
    [SerializeField] private float moveSpeed;

    [SerializeField] private float jumpHeight;

    [SerializeField] private float lookSensitivity;

    [SerializeField] private bool invertX, invertY;

    [SerializeField] private KeyCode Forward, Left, Back, Right, Jump, Interact, PickUp, Place;
    [FormerlySerializedAs("ChangeRotationAxis")] [SerializeField] private KeyCode HotbarScroll;
    [SerializeField] private KeyCode Inventory, Crouch;

    [SerializeField] private float lookOffset;

    [Header("Connections")] 
    
    [SerializeField] private UnityEvent Pause;

    [SerializeField] private LookController lookController;

    [SerializeField] private InventoryController inventory;

    [SerializeField] private HotbarController hotbar;

    private Rigidbody rb;
    private CapsuleCollider thisCollider;
    private Transform thisTF;
    private Transform lookTF;
    private Transform cursorTF;
    private bool jumpCD;
    private Vector3 finalMovement;
    private float originalOffset;
    private bool crouched;
    private void Start()
    {
        //passing the values for handling, it's more convenient to keep it all in one script to start
        lookController.LookSens = lookSensitivity;
        lookController.InvertX = invertX;
        lookController.InvertY = invertY;
        lookController.BodyTf = transform;
        rb = GetComponent<Rigidbody>();
        thisTF = transform;
        lookTF = lookController.transform;
        cursorTF = lookController.CursorTf;
        originalOffset = lookOffset;
        thisCollider = GetComponent<CapsuleCollider>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Time.timeScale = 0;
            Cursor.lockState = CursorLockMode.None;
            Pause.Invoke();
        }
        finalMovement = new Vector3(0,0,0);
        Debug.DrawLine(transform.position,transform.position + Vector3.down*1.1f, Color.blue);
        if (Input.GetKey(Forward))
        {
            finalMovement += thisTF.forward;
        }
        
        if (Input.GetKey(Left))
        {
            finalMovement -= thisTF.right;
        }
        
        if (Input.GetKey(Back))
        {
            finalMovement -= thisTF.forward;
        }
        
        if (Input.GetKey(Right))
        {
            finalMovement += thisTF.right;
        }
        
        if (Input.GetKeyDown(Jump))
        {
            PlayerJump();
        }

        finalMovement.y = 0;
        rb.AddForce(finalMovement.normalized * (moveSpeed * Time.deltaTime), ForceMode.Impulse);

        DoCrouch(Input.GetKey(Crouch));
        
        if (Input.GetKeyDown(Interact))
        {
            lookController.DoInteract(false);
        }

        if (Input.GetKeyDown(PickUp))
        {
            lookController.DoInteract(true);
        }
        
        if (Input.GetKeyDown(Inventory))
        {
            
        }

        if (Input.mouseScrollDelta.y != 0)
        {
            hotbar.Scroll(Input.mouseScrollDelta.y);
        }
        
        //DEBUGGG
        if (Input.GetKeyDown(Place) && Time.timeScale != 0)
        {
            if (inventory.HeldItems[hotbar.CurrentSelection])
            {
                inventory.PlaceItem(inventory.HeldItems[hotbar.CurrentSelection], cursorTF.position, hotbar.CurrentSelection);
            }
        }
    }

    public void Unpause()
    {
        Time.timeScale = 1;
        Cursor.lockState = CursorLockMode.Locked;
    }

    private void DoCrouch(bool crouching)
    {
        bool canUncrouch;
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, thisCollider.radius, Vector3.up, out hit, 0.5f) && crouched)
        {
            //Debug.Log($"Uncrouch raycast hit {hit.collider.name}");
            canUncrouch = false;
        }
        else
        {
            canUncrouch = true;
        }
        
        if (crouching)
        {
            lookOffset = originalOffset - 0.5f;
            crouched = true;
            thisCollider.height = 1.5f;
            thisCollider.center = new Vector3(0,-0.25f,0);
        }
        else
        {
            if (canUncrouch)
            {
                lookOffset = originalOffset;
                thisCollider.height = 2f;
                thisCollider.center = Vector3.zero;
            }
        }
    }

    private void FixedUpdate()
    {
        //rb.AddForce(Vector3.down*9.81f, ForceMode.Acceleration);
        rb.velocity = ClampVector(rb.velocity, -10f, 4f, true);
        //rb.MovePosition(thisTF.position + finalMovement * (Time.deltaTime * moveSpeed));
        lookTF.position = thisTF.position + new Vector3(0, lookOffset, 0);
    }

    //Method to clamp the magnitude of the vector
    private Vector3 ClampVector(Vector3 input, float min, float max, bool ignoreY)
    {
        // Normalize the input vector to preserve its direction
        Vector3 output = input.normalized;

        float mag;

        if (ignoreY)
        {
            // Calculate the magnitude using a 2D vector (ignoring Y)
            mag = Mathf.Clamp(new Vector2(input.x, input.z).magnitude, min, max);

            // Apply the clamped magnitude to the X and Z components
            output.x *= mag;
            output.z *= mag;

            // Preserve the Y component from the original input
            output.y = input.y;

            return output;
        }
    
        // Calculate the magnitude of the full 3D vector
        mag = Mathf.Clamp(input.magnitude, min, max);

        // Apply the clamped magnitude to all components
        output *= mag;

        return output;
    }

    private void PlayerJump()
    {
        RaycastHit hit;
        if (Physics.SphereCast(transform.position, 0.5f, Vector3.down, out hit, 1.05f) && !jumpCD)
        {
            StartCoroutine(JumpCooldown());
            //Debug.Log($"Hit {hit.collider}");
            rb.AddForce(Vector3.up * (jumpHeight), ForceMode.Impulse);
        }
    }

    private IEnumerator JumpCooldown()
    {
        jumpCD = true;
        yield return new WaitForSeconds(0.1f);
        jumpCD = false;
    }
}
