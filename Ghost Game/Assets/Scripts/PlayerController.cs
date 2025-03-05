using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

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
    private Vector3 input;
    public bool isInputSkewed;


    [Header("Raycasting Settings")]
    private int raycastLayer = 3;
    float rayAngleOffset = 30;
    float closestDistance = 10000; 

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
        GatherMovementInput();

        if (GameManager.Instance.isScareLevelRunning)
        {
            PlayerMove();

            // If I haven't picked up something, attempting interaction is allowed.
            if (!isObjectPickedUp)
            {
                // Constant raycasting (casts two rays one at eye level and one slightly below if nothing is found)
                Vector3 fwdDir = transform.TransformDirection(Vector3.forward);
                
                float xOffset = Mathf.Sin(Mathf.Deg2Rad * rayAngleOffset);
                float zOffset = Mathf.Cos(Mathf.Deg2Rad * rayAngleOffset);
                Vector3 fwdDirL = transform.TransformDirection(new Vector3(- xOffset, 0, zOffset));
                Vector3 fwdDirR = transform.TransformDirection(new Vector3(xOffset, 0, zOffset));
                Vector3[] fwdRaysDir = { fwdDir, fwdDirL, fwdDirR};

                foreach (Vector3 ray in fwdRaysDir)
                {
                    RayCast(ray);
                }

                /*
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
                */

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


    Transform RayCast(Vector3 dir)
    {
        Debug.DrawRay(transform.position, dir.normalized * interactionDistance, Color.green, 0.1f);

        // If an object has been hit
        RaycastHit hit;
        if (Physics.Raycast(transform.position, dir, out hit, interactionDistance, 1 << raycastLayer))
        {
            // 'Selects' the object hit by the raycast
            
            // Make sure the selected object exists in the scene before returning true 
            if (hit.collider != null)
            {
                // If this hit distance smaller than the current smallest distance,
                // Set the hitobj as the selected obj and override the smallest distance
                if (hit.distance < closestDistance)
                {
                    selectedInteractableObject = hit.collider.gameObject.transform;
                    
                    SuccessfulRaycast();
                    void SuccessfulRaycast()
                    {
                        // Highlight interactable object
                        Outliner outline = selectedInteractableObject.GetComponent<Outliner>();
                        if (outline != null)
                        {
                            outline.enabled = true;
                        }

                        if (Input.GetKeyDown(KeyCode.E))
                        {
                            InteractWithObject();
                        }
                    }
                    return selectedInteractableObject;
                }
                // Otherwise, do nothing
                else
                {
                    return null;
                }
                
            }
            else
            {
                return null;
            }
        }

        // If no object was hit in the raycast
        else
        {
            // Disable highlighted outline on object
            if (selectedInteractableObject != null)
            {
                Outliner outline = selectedInteractableObject.GetComponent<Outliner>();
                outline.enabled = false;
            }
            return null;
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

            //Slowly move object to pickup point
            selectedInteractableObject.GetComponent<Rigidbody>().DOMove(pickupPoint.transform.position, 0.5f);
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
    void GatherMovementInput()
    {
        // Gather lateral movement input
        input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
    }

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


        // Skews input to match isometric view
        Vector3 move;
   
        if (isInputSkewed)
        {
            var matrix = Matrix4x4.Rotate(Quaternion.Euler(0, -45, 0));
            var skewedInput = matrix.MultiplyPoint3x4(input);
            move = skewedInput;
        }
        else
        {
             move = input;
        }
        

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
