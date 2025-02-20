using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    #region <------- VARIABLE DEFINITIONS -------> //

    [Header("Player Movement")]
    CharacterController characterController;
    public Transform pickupPoint;
    private float jumpHeight = 1f;
    private float gravityValue = 9.81f;
    public float interactionDistance = 1.2f;

    [Header("Throwing Ability")]
    private float throwForce;
    private float baseThrowForce = 10f;
    private float maxThrowForce = 500f;
    [SerializeField] private bool isObjectPickedUp;
    public bool isReadyToThrow;
    private Rigidbody rb;

    [Header("Scare Visibiity Ability")]
    public bool isGhostVisible;
    public bool isHauntAbilityOnCooldown;
    public GameObject placeholderCube;

    private AbilitiesManager abilitiesManager;
    private float verticalVelocity;
    private float groundedTimer;

    [SerializeField] private Transform selectedInteractableObject;
    private InteractableObject currentInteractableObjectScript;
    private LayerMask NPCLayer;

    private int raycastLayer = 3;

    #endregion

    void Start()
    {
        characterController = gameObject.GetComponent<CharacterController>();
        NPCLayer = LayerMask.GetMask("NPC");
        abilitiesManager = AbilitiesManager.Instance;

        ResetPlayer();
    }

    public void ResetPlayer()
    {
        if (isObjectPickedUp && selectedInteractableObject != null)
        {
            Destroy(selectedInteractableObject.gameObject);
        }
        isObjectPickedUp = false;
        isReadyToThrow = false;
        isGhostVisible = false;
        isHauntAbilityOnCooldown = false;
        placeholderCube.SetActive(true);
    }

    void Update()
    {
        if (GameManager.Instance.isScareLevelRunning)
        {
            PlayerMove();

            // If I haven't picked up something, attempting interaction is allowed.
            if (!isObjectPickedUp)
            {
                // Constant raycasting (casts two rays one at eye level and one slightly below if nothing is found)
                Vector3 fwdDir = transform.TransformDirection(Vector3.forward);
                Vector3 inclinedDir = new Vector3(fwdDir.x, -1, fwdDir.z);
                // If raycast finds an interactable object, highlight it, and E lets you interact with it 
                if (RayCast(fwdDir))
                {
                    SuccessfulRaycast();
                }
                else if (RayCast(inclinedDir))
                {
                    SuccessfulRaycast();
                }

                void SuccessfulRaycast()
                {
                    // Highlight interactable object
                    Outline outline = selectedInteractableObject.GetComponent<Outline>();
                    if (outline != null)
                    {
                        outline.enabled = true;
                    }

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

            if (abilitiesManager.isVisibilityAbilityUnlocked)
            {
                if (Input.GetKeyDown(KeyCode.R) && !isHauntAbilityOnCooldown)
                {
                    StartCoroutine("BecomeVisibleToNPCs");
                }
            }
        }

    }

 

    bool RayCast(Vector3 dir)
    {
        Debug.DrawRay(transform.position, dir.normalized * interactionDistance, Color.green, 3f);

        RaycastHit hit;
        if (Physics.Raycast(transform.position, dir, out hit, interactionDistance, 1 << raycastLayer))
        {

            //print("Something interactable in front of the player!");
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
            //print("Nothing interactable there.");
            return false;
        }
    }

    void InteractWithObject()
    {
        currentInteractableObjectScript = selectedInteractableObject.GetComponent<InteractableObject>();

        // Check that the object is not on cooldown before interacting
        if (!currentInteractableObjectScript.isOnCooldown)
        {
            if (currentInteractableObjectScript.objectType == InteractableObject.ObjectType.Throwable)
            {
                if (abilitiesManager.maxWeightThrowable >= currentInteractableObjectScript.objectWeight)
                {
                    PickUpObject();
                }
                else
                {
                    print("Object is too heavy for your current abilities to throw.");
                }
            }
            if (currentInteractableObjectScript.objectType == InteractableObject.ObjectType.Togglable)
            {
                ToggleObject();
            }
        }
    }

    // <---------------------------------- SCARE VISIBILITY ABILITY ---------------------------------- > //

    IEnumerator BecomeVisibleToNPCs()
    {
        isGhostVisible = true;
        placeholderCube.SetActive(false);
        isHauntAbilityOnCooldown = true;
        yield return new WaitForSeconds(abilitiesManager.ghostScareVisibilityDuration);
        isGhostVisible = false;
        yield return new WaitForSeconds(abilitiesManager.ghostScareCooldown);
        isHauntAbilityOnCooldown = false;
        placeholderCube.SetActive(true);
    }


    // <---------------------------------- TOGGLE ABILITY ---------------------------------- > //

    void ToggleObject()
    {
        currentInteractableObjectScript.ToggleObject();
    }

    // <---------------------------------- THROWING ABILITY ---------------------------------- > //

    void PickUpObject()
    {
        rb = selectedInteractableObject.GetComponent<Rigidbody>();
        
        //Pick up object by making it a child of the player's pickup point, checking that nothing has been picked up already
        if (pickupPoint.childCount < 1)
        {
            selectedInteractableObject.parent = pickupPoint.transform;
            isObjectPickedUp = true;
            //print("Object successfully picked up.");

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
                throwForce += abilitiesManager.throwForceMult * 100 * Time.deltaTime;
                //print("Charging up throw. Throw force: " + throwForce);
            }
        }

        // When E is released, hurl/throw object
        if (Input.GetKeyUp(KeyCode.E))
        {
            // Unparent throwable object from pickup point and parent it back to the level
            selectedInteractableObject.parent = GameObject.FindWithTag("Level").transform;
            // Throw object with calculated force
            rb.AddForce(throwForce * gameObject.transform.forward);
            // Reenable gravity
            rb.useGravity = true;
            // When object is thrown/released by player, set the can scare flag to true (done in player script);
            currentInteractableObjectScript.isCanScareNPC = true;
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
        move *= abilitiesManager.movementSpeed;

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
