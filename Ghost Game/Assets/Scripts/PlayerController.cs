using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("Player Movement")]
    CharacterController characterController;
    private float verticalVelocity;
    private float movementSpeed = 10f;
    private float jumpHeight = 1f;
    private float gravityValue = 9.81f;
    private float groundedTimer; 

    void Start()
    {
        characterController = gameObject.GetComponent<CharacterController>();
    }

    
    void Update()
    {
        PlayerMove();
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
