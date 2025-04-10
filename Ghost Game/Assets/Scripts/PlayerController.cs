using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class PlayerController : MonoBehaviour
{
    #region <------- VARIABLE DEFINITIONS -------> //


    [Header("Player Keyboard Controls")]
    public KeyCode interactionKeyCode = KeyCode.E;
    public KeyCode hauntKeyCode = KeyCode.R;
    public bool isPlayerMovementEnabled;


    [Header("Player Movement")]
    CharacterController characterController;
    public Transform pickupPoint;
    private float jumpHeight = 1f;
    private float gravityValue = 9.81f;
    public float interactionDistance = 1.2f;
    public Vector3 NPCforceVector;


    [Header("Player Animator")]
    public Animator playerAnimator;

    [Header("Throwing Ability")]
    private float throwForce;
    private float baseThrowForce = 10f;
    private float maxThrowForce = 500f;
    public bool isObjectPickedUp;
    public bool isReadyToThrow;
    private Rigidbody rb;

    [Header("Scare Visibiity Ability")]
    public bool isGhostVisible;
    public bool isHauntAbilityOnCooldown;

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
        TeleportPlayer();
        playerAnimator.SetBool("isGhostVictory", false);
        playerAnimator.Play("Idle Move");
        if (isObjectPickedUp && selectedInteractableObject != null)
        {
            Destroy(selectedInteractableObject.gameObject);
        }
        isObjectPickedUp = false;
        isReadyToThrow = false;
        isGhostVisible = false;
        isHauntAbilityOnCooldown = false;
        UIManager.Instance.UpdateHauntAbilityIndicator(1);
        NPCforceVector = Vector3.zero;
        isPlayerMovementEnabled = true;
    }

    private void TeleportPlayer()
    {
        transform.position = GameManager.Instance.playerStartPosition.position;
        Physics.SyncTransforms();
        VFXManager.Instance.InstantiateSpawningPS(GameManager.Instance.playerStartPosition).transform.parent = gameObject.transform;
    }


    void Update()
    {
        GatherMovementInput();

        if (GameManager.Instance.isScareLevelRunning || GameManager.Instance.isTutorialRunning || GameManager.Instance.isHauntTutorialRunning)
        {
            if (isPlayerMovementEnabled)
            {
                PlayerMove();
            }

            // If I haven't picked up something, attempting interaction is allowed.
            if (!isObjectPickedUp)
            {
                // Constant raycasting (casts two rays one at eye level and one slightly below if nothing is found)
                Vector3 fwdDir = transform.TransformDirection(Vector3.forward);
                Vector3 inclinedDir = new Vector3(fwdDir.x, -1, fwdDir.z);
                Vector3 topDir = new Vector3(fwdDir.x, -0.1f, fwdDir.z);


                float xOffset = Mathf.Sin(Mathf.Deg2Rad * rayAngleOffset);
                float zOffset = Mathf.Cos(Mathf.Deg2Rad * rayAngleOffset);
                Vector3 fwdDirL = transform.TransformDirection(new Vector3(- xOffset, -0.3f, zOffset));
                Vector3 fwdDirR = transform.TransformDirection(new Vector3(xOffset, -0.3f, zOffset));
                //Vector3[] fwdRaysDir = {inclinedDir, topDir, fwdDirL, fwdDirR};

                //Vector3[,] fwdRaysDir = { { transform.position + new Vector3(0f, 2f, 0f), topDir }};
                Vector3[,] fwdRaysDir = { { transform.position, inclinedDir}, { transform.position + new Vector3(0f, 1.7f, 0f), topDir }, {transform.position, fwdDirL}, { transform.position, fwdDirR} };

                closestDistance = 10000f;

                bool RaycastingArray()
                {
                    bool isObjDetected = false;

                    for (int i=0; i < fwdRaysDir.GetLength(0); i++)
                    {
                        if (RayCast(fwdRaysDir[i, 0], fwdRaysDir[i, 1]))
                        {
                            isObjDetected = true;
                        }
                    }

                    /*
                    foreach (Vector3 ray in fwdRaysDir)
                    {
                        if (RayCast(transform.position, ray))
                        {
                            isObjDetected = true;
                        }
                    }

                    */
                    return isObjDetected;
                }

                if (RaycastingArray())
                {

                    // <<--------- TUTORIAL STUFF --------- >>
                    // If the toggling tutorial has not been completed
                    if (GameManager.Instance.isTutorialRunning && !GameManager.Instance.isToggleTutorialCompleted)
                    {
                        // Only allow interaction with the television
                        if (selectedInteractableObject.name == "Television")
                        {
                            SuccessfulRaycast();
                            // Completed toggling tutorial, starting to teach premise
                            if (Input.GetKeyDown(interactionKeyCode))
                            {
                                GameManager.Instance.isToggleTutorialCompleted = true;
                                GameManager.Instance.StartTeachPremise();

                            }
                        }
                    }
                    // <<---------------------------------- >>

                    else
                    {
                        // Allow interaction with all objects
                        SuccessfulRaycast();
                    }
                    
                    void SuccessfulRaycast()
                    {
                        // Highlight interactable object
                        selectedInteractableObject.GetComponent<InteractableObject>().ToggleOutline(true);

                        if (Input.GetKeyDown(interactionKeyCode))
                        {
                            InteractWithObject();
                        }
                    }
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
                if (Input.GetKeyDown(hauntKeyCode))
                {
                    if (!isHauntAbilityOnCooldown)
                    {
                        playerAnimator.Play("Haunt");
                        AudioManager.instance.Play("Haunt Ability");
                        VFXManager.Instance.PlayHauntPS();

                        GameManager.Instance.CameraShake();
                        UIManager.Instance.UseHauntAbilityIndicator();
                        StartCoroutine("BecomeVisibleToNPCs");
                    }
                    else
                    {
                        UIManager.Instance.ShakeHauntAbilityIndicator();
                    }
                    
                }
            }
        }

    }


    bool RayCast(Vector3 startingPoint, Vector3 dir)
    {
        Debug.DrawRay(startingPoint, dir.normalized * interactionDistance, Color.green, 0.1f);

        // If an object has been hit
        RaycastHit hit;
        if (Physics.Raycast(startingPoint, dir, out hit, interactionDistance, 1 << raycastLayer))
        { 
            // Make sure the selected object exists in the scene before returning true 
            if (hit.collider != null)
            {
                // If this hit distance smaller than the current smallest distance,
                // Set the hitobj as the selected obj and override the smallest distance
                if (hit.distance < closestDistance)
                {
                    selectedInteractableObject = hit.collider.gameObject.transform;
                    print("current selectedInteractableObj: " + selectedInteractableObject.name);
                  
                    return selectedInteractableObject;
                }
                // Otherwise, do nothing
                else
                {
                    return false;
                }
                
            }
            else
            {
                return false;
            }
        }

        // If no object was hit in the raycast
        else
        {
            // Disable highlighted outline on object
            if (selectedInteractableObject != null)
            {
                selectedInteractableObject.GetComponent<InteractableObject>().ToggleOutline(false);
            }
            return false;
        }
    }

    void InteractWithObject()
    {
        currentInteractableObjectScript = selectedInteractableObject.GetComponent<InteractableObject>();

        // Check that the object is not on cooldown before interacting
        if (!currentInteractableObjectScript.isOnCooldown)
        {
            AudioManager.instance.Play("Interact Sound");
            VFXManager.Instance.InstantiateInteractPS(gameObject.transform);

            if (currentInteractableObjectScript.objectType == InteractableObject.ObjectType.Throwable)
            {
                if (abilitiesManager.maxWeightThrowable >= currentInteractableObjectScript.objectWeight)
                {
                    PickUpObject();
                }
                else
                {
                    UIManager.Instance.DisplayTooHeavyNotification();
                    print("Object is too heavy for your current abilities to throw.");
                }
            }
            if (currentInteractableObjectScript.objectType == InteractableObject.ObjectType.Togglable)
            {
                ToggleObject();
            }
        }
    }

    // <-------------------------- HAUNT/SCARE VISIBILITY ABILITY -------------------------- > //

    IEnumerator BecomeVisibleToNPCs()
    {
        UIManager.Instance.UpdateHauntAbilityIndicator(0);
        isGhostVisible = true;
        isHauntAbilityOnCooldown = true;
        yield return new WaitForSeconds(abilitiesManager.ghostScareVisibilityDuration);
        isGhostVisible = false;

        if (!GameManager.Instance.isHauntTutorialRunning)
        {
            UIManager.Instance.TweenHauntAbilityIndicator(1, abilitiesManager.ghostScareCooldown);
            yield return new WaitForSeconds(abilitiesManager.ghostScareCooldown);
            ResetHauntCooldown();
        }
    }

    public void ResetHauntCooldown()
    {
        UIManager.Instance.UpdateHauntAbilityIndicator(1);
        isHauntAbilityOnCooldown = false;
    }


    // <---------------------------------- TOGGLE ABILITY ---------------------------------- > //

    void ToggleObject()
    {
        playerAnimator.Play("Toggle");
        currentInteractableObjectScript.ToggleObject();
    }

    // <---------------------------------- THROWING ABILITY ---------------------------------- > //

    void PickUpObject()
    {
        rb = selectedInteractableObject.GetComponent<Rigidbody>();
        
        //Pick up object by making it a child of the player's pickup point, checking that nothing has been picked up already
        if (pickupPoint.childCount < 1)
        {
            playerAnimator.Play("Hold");
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
        // As keycode is held down while object is picked up
        if (Input.GetKey(interactionKeyCode))
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
        if (Input.GetKeyUp(interactionKeyCode))
        {
            playerAnimator.Play("Throw");
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
            GameManager.Instance.objectsThrownScore++;
            print("Object thrown.");
        }
    }

    // <---------------------------------- PLAYER MOVEMENT ---------------------------------- > //

    public void GatherMovementInput()
    {
        // Gather lateral movement input
        input = new Vector3(Input.GetAxisRaw("Horizontal"), 0, Input.GetAxisRaw("Vertical"));
    }

    void PlayerMove()
    {
        /*
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
        */

        // Apply gravity always, to let us track down ramps properly
        verticalVelocity -= gravityValue * Time.deltaTime;

        /*
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
        */

        Vector3 moveVector;

        // Skews input to match isometric view
        if (isInputSkewed)
        {
            var matrix = Matrix4x4.Rotate(Quaternion.Euler(0, -45, 0));
            var skewedInput = matrix.MultiplyPoint3x4(input);
            moveVector = skewedInput;
        }
        else
        {
            moveVector = input; //uses input as the movement vector
        }
        // we want to add a vector after looking at the keyboard input


        moveVector *= abilitiesManager.movementSpeed;

        // Aligns the player character to the appropriate direction if minimum speed is met
        if (moveVector.magnitude > 0.05f)
        {
            gameObject.transform.forward = moveVector;
        }

        moveVector.y = verticalVelocity;

        Vector3 finalMoveVector = moveVector * Time.deltaTime + NPCforceVector;
        characterController.Move(finalMoveVector);
    }

}
