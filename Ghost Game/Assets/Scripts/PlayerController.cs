using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Player Movement")]
    CharacterController characterController;
    public Transform pickupPoint;
    private float movementSpeed = 10f;
    private float jumpHeight = 1f;
    private float gravityValue = 9.81f;
    public float playerWeightLimit = 1f;
    public float interactionDistance = 1.2f;

    [Header("Throwing Ability")]
    private float throwForce;
    public float baseThrowForce = 10f;
    public float maxThrowForce = 500f;
    public float throwForceMult = 1f;
    public bool isObjectPickedUp;
    public bool isReadyToThrow;
    private Rigidbody rb;


    private float verticalVelocity;
    private float groundedTimer;

    [SerializeField] private Transform selectedInteractableObject;
    private InteractableObject currentInteractableObjectScript;

    private int raycastLayer = 3;

    void Start()
    {
        characterController = gameObject.GetComponent<CharacterController>();
        isObjectPickedUp = false;
        isReadyToThrow = false;
    }


    void Update()
    {
        PlayerMove();

        // If I haven't picked up something, attempting interaction is allowed.
        if (!isObjectPickedUp)
        {
            // Constant raycasting
            // If raycast finds an interactable object, highlight it, and E lets you interact with it 
            if (RayCast())
            {
                // Highlight interactable object
                Outline outline = selectedInteractableObject.GetComponent<Outline>();
                outline.enabled = true;

                if (Input.GetKeyDown(KeyCode.E))
                {
                    InteractWithObject();
                }
            }

        }
        else // If an object has been picked up, throw object
        {
            ThrowObject();
        }

    }

    bool RayCast()
    {
        Vector3 fwdDir = transform.TransformDirection(Vector3.forward);
        Debug.DrawRay(transform.position, fwdDir.normalized * interactionDistance, Color.green, 3f);

        RaycastHit hit;
        if (Physics.Raycast(transform.position, fwdDir, out hit, interactionDistance, 1 << raycastLayer))
        {

            print("Something interactable in front of the player!");
            // 'Selects' the object hit by the raycast
            selectedInteractableObject = hit.collider.gameObject.transform;
            // Make sure the selected object exists in the scene before returning true 
            if (hit.collider != null)
                return true;
            else
            {
                return false;
            }
        }

        else
        {
            // Disable highlighted outline on object
            if (selectedInteractableObject != null)
            {
                Outline outline = selectedInteractableObject.GetComponent<Outline>();
                outline.enabled = false;
            }
            print("Nothing interactable there.");
            return false;
        }
    }

    void InteractWithObject()
    {
        currentInteractableObjectScript = selectedInteractableObject.GetComponent<InteractableObject>();
        if (currentInteractableObjectScript.objectType == InteractableObject.ObjectType.Throwable)
        {
            PickUpObject();
        }
        if (currentInteractableObjectScript.objectType == InteractableObject.ObjectType.Togglable)
        {
            ToggleObject();
        }
    }

    // <---------------------------------- TOGGLE ABILITY ---------------------------------- > //

    void ToggleObject()
    {
        currentInteractableObjectScript.isToggledOn = !currentInteractableObjectScript.isToggledOn;
    }

    // <---------------------------------- THROWING ABILITY ---------------------------------- > //

    void PickUpObject()
    {
        rb = selectedInteractableObject.GetComponent<Rigidbody>();
        //Pick up object by making it a child of the player's pickup point, checking that nothing has been picked up already
        if (pickupPoint.childCount < 1)
        {
            selectedInteractableObject.parent = pickupPoint.transform;
            print("Object successfully picked up.");
            isObjectPickedUp = true;

            rb.useGravity = false;
            // Reset throw force;
            throwForce = baseThrowForce;
        }
    }
    void ThrowObject()
    {
        // As E is held down while object is picked up
        if (Input.GetKey(KeyCode.E))
        {
            // Zero out the object's existing velocity
            rb.velocity = Vector3.zero;

            // Generate/calculate throw force
            if (throwForce < maxThrowForce)
            {
                throwForce += throwForceMult * 100 * Time.deltaTime;
                print("Charging up throw. Throw force: " + throwForce);
            }
        }

        // When E is released, hurl/throw object
        if (Input.GetKeyUp(KeyCode.E))
        {
            // Unparent throwable object from pickup point
            selectedInteractableObject.parent = null;
            // Throw object with calculated force
            rb.AddForce(throwForce * gameObject.transform.forward);
            //rb.AddForce(10000F * gameObject.transform.forward);
            // Reenable gravity
            rb.useGravity = true;
            isObjectPickedUp = false;
            //selectedInteractableObject = null;
            print("Object thrown.");
        }
    }

    

    // <---------------------------------- PLAYER MOVEMENT ---------------------------------- > //

    void PlayerMove()
    {
        bool groundedPlayer = characterController.isGrounded;
        if (groundedPlayer)
        {
            // Cooldown interval to allow reliable jumping even whem coming down ramps
            groundedTimer = 0.2f;
        }
        if (groundedTimer > 0)
        {
            groundedTimer -= Time.deltaTime;
        }

        // Slam into the ground
        if (groundedPlayer && verticalVelocity < 0)
        {
            // hit ground
            verticalVelocity = 0f;
        }
        // Apply gravity always, to let us track down ramps properly
        verticalVelocity -= gravityValue * Time.deltaTime;

        // Gather lateral movement input
        Vector3 move = new Vector3(Input.GetAxis("Horizontal"), 0f, Input.GetAxis("Vertical"));
        move *= movementSpeed;

        // Aligns the player character to the appropriate direction if minimum speed is met
        if (move.magnitude > 0.05f)
        {
            gameObject.transform.forward = move;
        }

        // Allow jump as long as the player is on the ground
        if (Input.GetButtonDown("Jump"))
        {
            // Must have been grounded recently to allow jump
            if (groundedTimer > 0)
            {
                // No more until we recontact ground
                groundedTimer = 0;

                // Physics dynamics formula for calculating jump up velocity based on height and gravity
                verticalVelocity += Mathf.Sqrt(jumpHeight * 2 * gravityValue);
            }
        }

        move.y = verticalVelocity;
        characterController.Move(move * Time.deltaTime);
    }




}
