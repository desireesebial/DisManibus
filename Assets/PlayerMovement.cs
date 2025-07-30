using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(CharacterController))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Movement Speed")]
    public float walkSpeed = 5f;
    public float sprintSpeed = 10f;
    public float crouchSpeed = 2f;

    [Header("Mouse Look")]
    public float mouseSensitivity = 2f;
    public Transform playerCamera;
    public float maxLookUpAngle = 80f;
    public float maxLookDownAngle = -80f;

    [Header("Crouch Settings")]
    public float crouchHeight = 1f;
    public float standingHeight = 2f;
    public float crouchCameraOffset = 0.5f; // How much to lower the camera when crouching
    public float crouchTransitionSpeed = 5f; // How fast to transition between standing and crouching

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
    private float verticalRotation = 0f;
    private Vector3 originalCameraPosition;
    private Vector3 crouchCameraPosition;
    private Vector3 originalControllerCenter;
    private Vector3 crouchControllerCenter;
    private float targetHeight;
    private float currentHeight;

    void Start() 
    {
        controller = GetComponent<CharacterController>();
        currentSpeed = walkSpeed;
        currentSprintTime = maxSprintTime;

        // Lock and hide cursor
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // If no camera is assigned, try to find one
        if (playerCamera == null)
        {
            Camera cam = GetComponentInChildren<Camera>();
            if (cam != null)
            {
                playerCamera = cam.transform;
            }
        }

        // Store original camera position
        if (playerCamera != null)
        {
            originalCameraPosition = playerCamera.localPosition;
            crouchCameraPosition = originalCameraPosition - Vector3.up * crouchCameraOffset;
        }

        // Store original controller settings
        originalControllerCenter = controller.center;
        crouchControllerCenter = new Vector3(originalControllerCenter.x, crouchHeight / 2f, originalControllerCenter.z);
        currentHeight = standingHeight;
        targetHeight = standingHeight;

        if(staminaBar != null)
        {
            staminaBar.maxValue = maxSprintTime;
            staminaBar.value = currentSprintTime;
        }
    }

    void Update()
    {
        MovePlayer();
        HandleMouseLook();
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

    void HandleMouseLook()
    {
        if (playerCamera == null) return;

        // Get mouse input
        float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity;
        float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity;

        // Rotate the player body horizontally
        transform.Rotate(Vector3.up * mouseX);

        // Rotate the camera vertically
        verticalRotation -= mouseY;
        verticalRotation = Mathf.Clamp(verticalRotation, maxLookDownAngle, maxLookUpAngle);
        playerCamera.localRotation = Quaternion.Euler(verticalRotation, 0f, 0f);
    }

    // Handling Crouch
    void HandleCrouch()
    {
        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            isCrouching = !isCrouching;
            targetHeight = isCrouching ? crouchHeight : standingHeight;
        }

        // Smoothly transition height
        if (Mathf.Abs(currentHeight - targetHeight) > 0.01f)
        {
            currentHeight = Mathf.Lerp(currentHeight, targetHeight, Time.deltaTime * crouchTransitionSpeed);
            controller.height = currentHeight;
            
            // Adjust controller center based on height
            float centerY = currentHeight / 2f;
            controller.center = new Vector3(originalControllerCenter.x, centerY, originalControllerCenter.z);
            
            // Smoothly adjust camera position
            if (playerCamera != null)
            {
                float t = (standingHeight - currentHeight) / (standingHeight - crouchHeight);
                t = Mathf.Clamp01(t);
                playerCamera.localPosition = Vector3.Lerp(originalCameraPosition, crouchCameraPosition, t);
            }
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