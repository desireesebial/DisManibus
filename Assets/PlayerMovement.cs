using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Speed")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 10f;
    public float crouchSpeed = 2f;

    [Header("Crouch Settings")]
    public float crouchHeight = 1f;
    public float standingHeight = 2f;

    [Header("Sprint Stamina")]
    public float maxSprintTime = 3f;
    public float sprintRechargeTime = 3f;
    public Slider staminaBar;

    private CharacterController controller;
    private float currentSpeed;
    private bool isCrouching = false;
    private float currentSprintTime;
    private float currentRechargeTime;
    private bool isSprinting = false;
    private bool canSprint = true;

    void Start() 
    {
        controller = GetComponent<CharacterController>();
        currentSpeed = walkSpeed;
        currentSprintTime = maxSprintTime;

        if(staminaBar != null)
        {
            staminaBar.maxValue = maxSprintTime;
            staminaBar.value = currentSprintTime;
        }
    }

    void Update()
    {
        MovePlayer();
        HandleCrouch();
        HandleSprintStamina();
    }

    void MovePlayer() 
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        Vector3 move = transform.right * moveX + transform.forward * moveZ;

        // Sprinting part
        if (Input.GetKey(KeyCode.LeftShift) && !isCrouching && canSprint && currentSprintTime > 0)
        {
            currentSpeed = sprintSpeed;
            isSprinting = true;
        }
        // Crouching
        else if (isCrouching)
        {
            currentSpeed = crouchSpeed;
            isSprinting = false;
        }
        // Walking
        else 
        {
            currentSpeed = walkSpeed;
            isSprinting = false;
        }
        controller.Move(move * currentSpeed * Time.deltaTime);
    }

    // Handling Crouch
    void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isCrouching = !isCrouching;
            controller.height = isCrouching ? crouchHeight : standingHeight;
        }
    }

    // Handling Stamina
    void HandleSprintStamina()
    {
        // If sprinting, consume stamina
        if (isSprinting && currentSprintTime > 0)
        {
            currentSprintTime -= Time.deltaTime;
            
            // If stamina runs out, stop sprinting
            if (currentSprintTime <= 0)
            {
                currentSprintTime = 0;
                canSprint = false;
                isSprinting = false;
                currentRechargeTime = 0;
            }
        }
        // If not sprinting and stamina is depleted, recharge
        else if (!canSprint)
        {
            currentRechargeTime += Time.deltaTime;
            
            // If recharge time is complete, allow sprinting again
            if (currentRechargeTime >= sprintRechargeTime)
            {
                canSprint = true;
                currentSprintTime = maxSprintTime;
                currentRechargeTime = 0;
            }
        }
        // If not sprinting and stamina is not full, recharge
        else if (!isSprinting && currentSprintTime < maxSprintTime)
        {
            currentSprintTime += Time.deltaTime;
            currentSprintTime = Mathf.Min(currentSprintTime, maxSprintTime);
        }

        // Update stamina bar
        if (staminaBar != null)
        {
            staminaBar.value = currentSprintTime;
        }
    }

    // Public methods for UI or other scripts to check sprint status
    public bool IsSprinting() => isSprinting;
    public bool CanSprint() => canSprint;
    public float GetSprintTimeRemaining() => currentSprintTime;
    public float GetSprintRechargeProgress() => currentRechargeTime / sprintRechargeTime;
}