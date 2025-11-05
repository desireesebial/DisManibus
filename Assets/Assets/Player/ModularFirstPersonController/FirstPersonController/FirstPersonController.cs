// CHANGE LOG
// 
// CHANGES || version VERSION
//
// "Enable/Disable Headbob, Changed look rotations - should result in reduced camera jitters" || version 1.0.1

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

#if UNITY_EDITOR
    using UnityEditor;
    using System.Net;
#endif

public class FirstPersonController : MonoBehaviour
{
    // Global reference to the active FirstPersonController
    public static FirstPersonController Instance { get; private set; }

    // Public accessors for commonly used components
    public Camera PlayerCamera => playerCamera;
    public Image CrosshairImage => crosshairObject;

    private Rigidbody rb;

    #region Camera Movement Variables

    public Camera playerCamera;

    public float fov = 60f;
    public bool invertCamera = false;
    public bool cameraCanMove = true;
    public float mouseSensitivity = 2f;
    public float maxLookAngle = 50f;

    // Crosshair
    public bool lockCursor = true;
    public bool crosshair = true;
    public Sprite crosshairImage;
    public Color crosshairColor = Color.white;

    // Internal Variables
    private float yaw = 0.0f;
    private float pitch = 0.0f;
    private Image crosshairObject;

    #region Camera Zoom Variables

    public bool enableZoom = true;
    public bool holdToZoom = false;
    public KeyCode zoomKey = KeyCode.Mouse1;
    public float zoomFOV = 30f;
    public float zoomStepTime = 5f;

    // Internal Variables
    private bool isZoomed = false;

    #endregion
    #endregion

    #region Movement Variables

    public bool playerCanMove = true;
    public float walkSpeed = 5f;
    public float maxVelocityChange = 10f;

    // Internal Variables
    private bool isWalking = false;

    #region Sprint

    public bool enableSprint = true;
    public bool unlimitedSprint = false;
    public KeyCode sprintKey = KeyCode.LeftShift;
    public float sprintSpeed = 7f;
    public float sprintDuration = 5f;
    public float sprintCooldown = .5f;
    public float sprintFOV = 80f;
    public float sprintFOVStepTime = 10f;

    // Sprint Bar
    public bool useSprintBar = true;
    public bool hideBarWhenFull = true;
    public Image sprintBarBG;
    public Image sprintBar;
    public float sprintBarWidthPercent = .3f;
    public float sprintBarHeightPercent = .015f;

    // Low Stamina Warning Colors
    [Header("Stamina Warning Colors")]
    [Tooltip("Stamina percentage when bar turns to low stamina color (default: 0.3 = 30%)")]
    [Range(0.1f, 0.5f)]
    public float lowStaminaThreshold = 0.3f;

    [Tooltip("Stamina percentage when bar turns to critical color (default: 0.15 = 15%)")]
    [Range(0.05f, 0.3f)]
    public float criticalStaminaThreshold = 0.15f;

    [Tooltip("Sprint bar color when stamina is normal (above low threshold)")]
    public Color normalStaminaColor = new Color(0.2f, 1f, 0.2f, 1f); // Green

    [Tooltip("Sprint bar color when stamina is low (below 30%)")]
    public Color lowStaminaColor = new Color(1f, 0.8f, 0f, 1f); // Orange/Yellow

    [Tooltip("Sprint bar color when stamina is critical (below 15%)")]
    public Color criticalStaminaColor = new Color(1f, 0.2f, 0.2f, 1f); // Red

    // Internal Variables
    private CanvasGroup sprintBarCG;
    private bool isSprinting = false;
    private float sprintRemaining;
    private float sprintBarWidth;
    private float sprintBarHeight;
    private bool isSprintCooldown = false;
    private float sprintCooldownReset;

    #endregion

    #region Jump

    public bool enableJump = true;
    public KeyCode jumpKey = KeyCode.Space;
    public float jumpPower = 5f;

    // Internal Variables
    private bool isGrounded = false;

    #endregion

    #region Crouch

    public bool enableCrouch = true;
    public bool holdToCrouch = true;
    public KeyCode crouchKey = KeyCode.LeftControl;
    public float crouchHeight = .75f;
    public float speedReduction = .5f;

    // Internal Variables
    private bool isCrouched = false;
    private Vector3 originalScale;
    private float originalWalkSpeed; // Store original walk speed to avoid accumulation errors

    #endregion
    #endregion

    #region Head Bob

    public bool enableHeadBob = true;
    public Transform joint;
    public float bobSpeed = 10f;
    public Vector3 bobAmount = new Vector3(.15f, .05f, 0f);

    // Internal Variables
    private Vector3 jointOriginalPos;
    private float timer = 0;

    #endregion

    #region Audio System

    [Header("Audio Sources")]
    [Tooltip("Audio source for footstep sounds (walk and sprint)")]
    public AudioSource footstepAudioSource;

    [Tooltip("Audio source for breathing sounds (exhausted/fatigued state)")]
    public AudioSource breathingAudioSource;

    [Header("Footstep Audio")]
    [Tooltip("Sound played when walking (drag and drop audio clip)")]
    public AudioClip walkFootstepSound;

    [Tooltip("Sound played when sprinting (drag and drop audio clip)")]
    public AudioClip sprintFootstepSound;

    [Tooltip("Time interval between walk footstep sounds in seconds")]
    public float walkFootstepInterval = 0.5f;

    [Tooltip("Time interval between sprint footstep sounds in seconds")]
    public float sprintFootstepInterval = 0.35f;

    [Header("Stamina Audio")]
    [Tooltip("Looping sound played when stamina is exhausted (drag and drop audio clip)")]
    public AudioClip exhaustedBreathingSound;

    [Tooltip("Stamina percentage where breathing starts to fade out (default: 0.3 = 30%)")]
    [Range(0.1f, 0.5f)]
    public float breathingFadeOutThreshold = 0.3f;

    // Internal Variables
    private float nextFootstepTime = 0f;
    private bool isExhaustedBreathingPlaying = false;

    #endregion

    #region Stamina Recovery Gate

    // Internal Variables
    private bool canSprintAgain = true; // Gate to prevent sprinting until 30% stamina recovered
    private bool isFatigued = false; // Player is fatigued when stamina hits 0

    #endregion

    #region Exhaustion System

    [Header("Exhaustion System")]
    [Tooltip("Speed multiplier when exhausted (0.5 = 50% of walk speed, matching crouch)")]
    [Range(0.3f, 0.9f)]
    public float exhaustedSpeedReduction = 0.5f;

    [Tooltip("Speed of the smooth transition when entering/exiting exhaustion (higher = faster)")]
    public float exhaustionTransitionSpeed = 2f;

    // Internal Variables
    private float currentSpeedMultiplier = 1f; // Current speed multiplier (1.0 = normal, 0.5 = exhausted)

    #endregion

    #region Tutorial

    [Header("Stamina Tutorial")]
    [Tooltip("Optional reference to StaminaTutorial component for first-time sprint notification")]
    public StaminaTutorial staminaTutorial; // UPDATED

    // Internal Variables
    private bool hasTriggeredStaminaTutorial = false;

    #endregion

    public bool IsCrouched => isCrouched;
    public bool IsSprinting => isSprinting;

    public void SetCrouchState(bool shouldCrouch)
    {
        if (enableCrouch)
        {
            if (isCrouched != shouldCrouch)
            {
                ToggleCrouch(shouldCrouch);
            }
        }
    }

    public void SetSprintState(bool shouldSprint)
    {
        if (!enableSprint)
        {
            return;
        }

        if (shouldSprint && !isSprinting)
        {
            if (playerCanMove && sprintRemaining > 0f && !isSprintCooldown)
            {
                isSprinting = true;
            }
        }
        else if (!shouldSprint && isSprinting)
        {
            isSprinting = false;
        }
    }

    /// <summary>
    /// Disables camera look/rotation. Useful for pause menus or cutscenes.
    /// </summary>
    public void DisableCameraLook()
    {
        cameraCanMove = false;
    }

    /// <summary>
    /// Enables camera look/rotation.
    /// </summary>
    public void EnableCameraLook()
    {
        cameraCanMove = true;
    }

    private void ToggleCrouch(bool targetState)
    {
        if (targetState)
        {
            transform.localScale = new Vector3(originalScale.x, crouchHeight, originalScale.z);
            walkSpeed = originalWalkSpeed * speedReduction; // Use stored value to avoid accumulation
            isCrouched = true;
        }
        else
        {
            // Check if there's enough space to stand up
            if (!CanStandUp())
            {
                return; // Don't uncrouch if there's an obstacle above
            }

            transform.localScale = new Vector3(originalScale.x, originalScale.y, originalScale.z);
            walkSpeed = originalWalkSpeed; // Restore original walk speed
            isCrouched = false;
        }
    }

    private void Awake()
    {
        // Set global reference
        Instance = this;

        rb = GetComponent<Rigidbody>();

        crosshairObject = GetComponentInChildren<Image>();

        // Set internal variables
        playerCamera.fieldOfView = fov;
        originalScale = transform.localScale;
        originalWalkSpeed = walkSpeed; // Store original walk speed

        // Handle head bob joint assignment
        if (enableHeadBob)
        {
            if (joint == null)
            {
                // Try to auto-assign to camera transform as a sensible default
                if (playerCamera != null)
                {
                    joint = playerCamera.transform;
                    Debug.LogWarning("[FirstPersonController] Head bob 'joint' was not assigned. Auto-assigned to camera transform.");
                }
                else
                {
                    // No valid joint available, disable head bob
                    enableHeadBob = false;
                    Debug.LogError("[FirstPersonController] Head bob 'joint' is not assigned and camera is null. Head bob has been disabled.");
                }
            }

            // Only set joint position if we have a valid joint
            if (joint != null)
            {
                jointOriginalPos = joint.localPosition;
            }
        }

        if (!unlimitedSprint)
        {
            sprintRemaining = sprintDuration;
            sprintCooldownReset = sprintCooldown;
        }
    }

    void Start()
    {
        if(lockCursor)
        {
            Cursor.lockState = CursorLockMode.Locked;
        }

        if(crosshair)
        {
            crosshairObject.sprite = crosshairImage;
            crosshairObject.color = crosshairColor;
        }
        else
        {
            crosshairObject.gameObject.SetActive(false);
        }

        #region Sprint Bar

        sprintBarCG = GetComponentInChildren<CanvasGroup>();

        if(useSprintBar)
        {
            sprintBarBG.gameObject.SetActive(true);
            sprintBar.gameObject.SetActive(true);

            float screenWidth = Screen.width;
            float screenHeight = Screen.height;

            sprintBarWidth = screenWidth * sprintBarWidthPercent;
            sprintBarHeight = screenHeight * sprintBarHeightPercent;

            sprintBarBG.rectTransform.sizeDelta = new Vector3(sprintBarWidth, sprintBarHeight, 0f);
            sprintBar.rectTransform.sizeDelta = new Vector3(sprintBarWidth - 2, sprintBarHeight - 2, 0f);

            if(hideBarWhenFull)
            {
                sprintBarCG.alpha = 0;
            }
        }
        else
        {
            sprintBarBG.gameObject.SetActive(false);
            sprintBar.gameObject.SetActive(false);
        }

        #endregion

        #region Stamina Tutorial Setup

        // Pass sprint bar reference to tutorial for auto-positioning
        if (staminaTutorial != null && sprintBar != null)
        {
            RectTransform sprintBarRect = sprintBar.GetComponent<RectTransform>();
            if (sprintBarRect != null)
            {
                staminaTutorial.SetStaminaBarReference(sprintBarRect);
                Debug.Log("[FirstPersonController] Sprint bar reference passed to StaminaTutorial");
            }
        }

        #endregion

        #region Audio Validation

        if (enableSprint && !unlimitedSprint)
        {
            if (breathingAudioSource == null)
            {
                Debug.LogError("[FirstPersonController] REQUIRED: Breathing AudioSource must be assigned for exhaustion system!", this);
            }
            else if (!breathingAudioSource.enabled)
            {
                Debug.LogWarning("[FirstPersonController] Breathing AudioSource is disabled - enabling it.", this);
                breathingAudioSource.enabled = true;
            }

            if (exhaustedBreathingSound == null)
            {
                Debug.LogError("[FirstPersonController] REQUIRED: Exhausted breathing sound must be assigned!", this);
            }
        }

        if (footstepAudioSource != null && !footstepAudioSource.enabled)
        {
            Debug.LogWarning("[FirstPersonController] Footstep AudioSource is disabled - enabling it.", this);
            footstepAudioSource.enabled = true;
        }

        #endregion
    }

    private void OnValidate()
    {
        #region Audio Validation
        if (enableSprint && !unlimitedSprint)
        {
            if (breathingAudioSource == null)
            {
                Debug.LogWarning("[FirstPersonController] Breathing AudioSource not assigned! Exhaustion audio won't play.", this);
            }

            if (exhaustedBreathingSound == null)
            {
                Debug.LogWarning("[FirstPersonController] Exhausted breathing sound not assigned! Exhaustion audio won't play.", this);
            }
        }

        if (footstepAudioSource == null)
        {
            Debug.LogWarning("[FirstPersonController] Footstep AudioSource not assigned! Footstep sounds won't play.", this);
        }
        #endregion
    }

    // Helper to get an active player camera from anywhere
    public static Camera GetActivePlayerCamera()
    {
        if (Instance != null && Instance.playerCamera != null)
        {
            return Instance.playerCamera;
        }
        return Camera.main;
    }

    float camRotation;

    private void Update()
    {
        #region Camera

        // Control camera movement
        if(cameraCanMove)
        {
            yaw = transform.localEulerAngles.y + Input.GetAxis("Mouse X") * mouseSensitivity;

            if (!invertCamera)
            {
                pitch -= mouseSensitivity * Input.GetAxis("Mouse Y");
            }
            else
            {
                // Inverted Y
                pitch += mouseSensitivity * Input.GetAxis("Mouse Y");
            }

            // Clamp pitch between lookAngle
            pitch = Mathf.Clamp(pitch, -maxLookAngle, maxLookAngle);

            transform.localEulerAngles = new Vector3(0, yaw, 0);
            playerCamera.transform.localEulerAngles = new Vector3(pitch, 0, 0);
        }

        #region Camera Zoom

        if (enableZoom)
        {
            // Changes isZoomed when key is pressed
            // Behavior for toogle zoom
            if(Input.GetKeyDown(zoomKey) && !holdToZoom && !isSprinting)
            {
                if (!isZoomed)
                {
                    isZoomed = true;
                }
                else
                {
                    isZoomed = false;
                }
            }

            // Changes isZoomed when key is pressed
            // Behavior for hold to zoom
            if(holdToZoom && !isSprinting)
            {
                if(Input.GetKeyDown(zoomKey))
                {
                    isZoomed = true;
                }
                else if(Input.GetKeyUp(zoomKey))
                {
                    isZoomed = false;
                }
            }

            // Lerps camera.fieldOfView to allow for a smooth transistion
            if(isZoomed)
            {
                playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, zoomFOV, zoomStepTime * Time.deltaTime);
            }
            else if(!isZoomed && !isSprinting)
            {
                playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, fov, zoomStepTime * Time.deltaTime);
            }
        }

        #endregion
        #endregion

        #region Sprint

        if(enableSprint)
        {
            if(isSprinting)
            {
                isZoomed = false;
                playerCamera.fieldOfView = Mathf.Lerp(playerCamera.fieldOfView, sprintFOV, sprintFOVStepTime * Time.deltaTime);

                // Drain sprint remaining while sprinting
                if(!unlimitedSprint)
                {
                    sprintRemaining -= 1 * Time.deltaTime;
                    if (sprintRemaining <= 0)
                    {
                        Debug.Log("[FirstPersonController] Stamina depleted - Player is now fatigued", this);
                        isSprinting = false;
                        isSprintCooldown = true;
                        canSprintAgain = false; // Gate sprint until 30% recovery
                        isFatigued = true; // Player is now fatigued
                    }
                }
            }
            else
            {
                // Regain sprint while not sprinting
                sprintRemaining = Mathf.Clamp(sprintRemaining += 1 * Time.deltaTime, 0, sprintDuration);

                // Check if stamina has recovered to 30% threshold - allow sprinting again
                float staminaPercent = sprintRemaining / sprintDuration;
                if (!canSprintAgain && staminaPercent >= breathingFadeOutThreshold)
                {
                    Debug.Log($"[FirstPersonController] Stamina recovered to {staminaPercent:P0} - Player no longer fatigued", this);
                    canSprintAgain = true;
                    isFatigued = false; // No longer fatigued
                }
            }

            // Handles sprint cooldown 
            // When sprint remaining == 0 stops sprint ability until hitting cooldown
            if(isSprintCooldown)
            {
                sprintCooldown -= 1 * Time.deltaTime;
                if (sprintCooldown <= 0)
                {
                    isSprintCooldown = false;
                }
            }
            else
            {
                sprintCooldown = sprintCooldownReset;
            }

            // Handles sprintBar
            if(useSprintBar && !unlimitedSprint)
            {
                float sprintRemainingPercent = sprintRemaining / sprintDuration;
                sprintBar.transform.localScale = new Vector3(sprintRemainingPercent, 1f, 1f);

                // Update sprint bar color based on stamina level
                if (sprintRemainingPercent <= criticalStaminaThreshold)
                {
                    // Critical stamina - use red
                    sprintBar.color = criticalStaminaColor;
                }
                else if (sprintRemainingPercent <= lowStaminaThreshold)
                {
                    // Low stamina - smoothly transition from yellow to red
                    float t = (sprintRemainingPercent - criticalStaminaThreshold) / (lowStaminaThreshold - criticalStaminaThreshold);
                    sprintBar.color = Color.Lerp(criticalStaminaColor, lowStaminaColor, t);
                }
                else
                {
                    // Normal stamina - smoothly transition from green to yellow
                    float t = (sprintRemainingPercent - lowStaminaThreshold) / (1f - lowStaminaThreshold);
                    sprintBar.color = Color.Lerp(lowStaminaColor, normalStaminaColor, t);
                }
            }
        }

        #endregion

        #region Jump

        // Gets input and calls jump method
        if(enableJump && Input.GetKeyDown(jumpKey) && isGrounded)
        {
            Jump();
        }

        #endregion

        #region Crouch

        if (enableCrouch)
        {
            if(Input.GetKeyDown(crouchKey) && !holdToCrouch)
            {
                Crouch();
            }

            if(Input.GetKeyDown(crouchKey) && holdToCrouch)
            {
                // When pressing crouch key, set to crouch (was inverted before)
                if (!isCrouched)
                {
                    Crouch();
                }
            }
            else if(Input.GetKeyUp(crouchKey) && holdToCrouch)
            {
                // When releasing crouch key, uncrouch (was inverted before)
                if (isCrouched)
                {
                    Crouch();
                }
            }
        }

        #endregion

        CheckGround();

        if(enableHeadBob)
        {
            HeadBob();
        }

        HandleFootsteps();
        HandleExhaustedBreathing();
        HandleExhaustion();

    }

    void FixedUpdate()
    {
        #region Movement

        if (playerCanMove)
        {
            // Calculate how fast we should be moving
            Vector3 targetVelocity = new Vector3(Input.GetAxis("Horizontal"), 0, Input.GetAxis("Vertical"));

            // Normalize diagonal movement to prevent faster speed when pressing two keys
            // (e.g., W+D would be 1.414x faster without normalization)
            if (targetVelocity.magnitude > 1f)
            {
                targetVelocity.Normalize();
            }

            // Checks if player is walking and isGrounded
            // Will allow head bob
            if (targetVelocity.x != 0 || targetVelocity.z != 0 && isGrounded)
            {
                isWalking = true;
            }
            else
            {
                isWalking = false;
            }

            // All movement calculations while sprint is active
            // Added canSprintAgain check to enforce 30% recovery gate
            if (enableSprint && Input.GetKey(sprintKey) && sprintRemaining > 0f && !isSprintCooldown && canSprintAgain)
            {
                // Don't allow sprinting while crouched - prevents collision issues
                if (isCrouched)
                {
                    // Use walk speed instead when trying to sprint while crouched
                    targetVelocity = transform.TransformDirection(targetVelocity) * walkSpeed * currentSpeedMultiplier;

                    Vector3 velocity = rb.linearVelocity;
                    Vector3 velocityChange = (targetVelocity - velocity);
                    velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
                    velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
                    velocityChange.y = 0;

                    rb.AddForce(velocityChange, ForceMode.VelocityChange);
                }
                else
                {
                    // Normal sprint behavior when not crouched
                    // Trigger stamina tutorial on first sprint
                    if (!hasTriggeredStaminaTutorial && staminaTutorial != null)
                    {
                        staminaTutorial.ShowTutorial();
                        hasTriggeredStaminaTutorial = true;
                    }

                    targetVelocity = transform.TransformDirection(targetVelocity) * sprintSpeed;

                    // Apply a force that attempts to reach our target velocity
                    Vector3 velocity = rb.linearVelocity;
                    Vector3 velocityChange = (targetVelocity - velocity);
                    velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
                    velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
                    velocityChange.y = 0;

                    // Player is only moving when valocity change != 0
                    // Makes sure fov change only happens during movement
                    if (velocityChange.x != 0 || velocityChange.z != 0)
                    {
                        isSprinting = true;

                        if (hideBarWhenFull && !unlimitedSprint)
                        {
                            sprintBarCG.alpha += 5 * Time.deltaTime;
                        }
                    }

                    rb.AddForce(velocityChange, ForceMode.VelocityChange);
                }
            }
            // All movement calculations while walking
            else
            {
                isSprinting = false;

                if (hideBarWhenFull && sprintRemaining == sprintDuration)
                {
                    sprintBarCG.alpha -= 3 * Time.deltaTime;
                }

                targetVelocity = transform.TransformDirection(targetVelocity) * walkSpeed * currentSpeedMultiplier;

                // Apply a force that attempts to reach our target velocity
                Vector3 velocity = rb.linearVelocity;
                Vector3 velocityChange = (targetVelocity - velocity);
                velocityChange.x = Mathf.Clamp(velocityChange.x, -maxVelocityChange, maxVelocityChange);
                velocityChange.z = Mathf.Clamp(velocityChange.z, -maxVelocityChange, maxVelocityChange);
                velocityChange.y = 0;

                rb.AddForce(velocityChange, ForceMode.VelocityChange);
            }
        }

        #endregion
    }

    // Sets isGrounded based on a raycast sent straigth down from the player object
    private void CheckGround()
    {
        // Use original scale for consistent ground checking, regardless of crouch state
        Vector3 origin = new Vector3(transform.position.x, transform.position.y - (originalScale.y * .5f), transform.position.z);
        Vector3 direction = transform.TransformDirection(Vector3.down);
        float distance = .75f;

        if (Physics.Raycast(origin, direction, out RaycastHit hit, distance))
        {
            Debug.DrawRay(origin, direction * distance, Color.red);
            isGrounded = true;
        }
        else
        {
            isGrounded = false;
        }
    }

    // Checks if there's enough space above the player to stand up from crouch
    private bool CanStandUp()
    {
        // Calculate the height difference between crouch and stand
        float heightDifference = originalScale.y - crouchHeight;

        // Cast a ray upward from the top of the crouched player to check for obstacles
        Vector3 origin = transform.position + Vector3.up * (crouchHeight * 0.5f);
        float checkDistance = heightDifference * 0.5f + 0.1f; // Add small buffer

        // Check if there's an obstacle above
        if (Physics.Raycast(origin, Vector3.up, checkDistance))
        {
            Debug.DrawRay(origin, Vector3.up * checkDistance, Color.yellow);
            return false; // Can't stand up, obstacle detected
        }

        Debug.DrawRay(origin, Vector3.up * checkDistance, Color.green);
        return true; // Safe to stand up
    }

    private void Jump()
    {
        // Adds force to the player rigidbody to jump
        if (isGrounded)
        {
            rb.AddForce(0f, jumpPower, 0f, ForceMode.Impulse);
            isGrounded = false;
        }

        // When crouched and using toggle system, will uncrouch for a jump
        if(isCrouched && !holdToCrouch)
        {
            Crouch();
        }
    }

    private void Crouch()
    {
        if(isCrouched)
        {
            ToggleCrouch(false);
        }
        else
        {
            ToggleCrouch(true);
        }
    }

    private void HeadBob()
    {
        // Safety check - don't run if joint is null
        if (joint == null)
        {
            return;
        }

        if(isWalking)
        {
            // Calculates HeadBob speed during sprint
            if(isSprinting)
            {
                timer += Time.deltaTime * (bobSpeed + sprintSpeed);
            }
            // Calculates HeadBob speed during crouched movement
            else if (isCrouched)
            {
                timer += Time.deltaTime * (bobSpeed * speedReduction);
            }
            // Calculates HeadBob speed during walking
            else
            {
                timer += Time.deltaTime * bobSpeed;
            }
            // Applies HeadBob movement
            joint.localPosition = new Vector3(jointOriginalPos.x + Mathf.Sin(timer) * bobAmount.x, jointOriginalPos.y + Mathf.Sin(timer) * bobAmount.y, jointOriginalPos.z + Mathf.Sin(timer) * bobAmount.z);
        }
        else
        {
            // Resets when play stops moving
            timer = 0;
            joint.localPosition = new Vector3(Mathf.Lerp(joint.localPosition.x, jointOriginalPos.x, Time.deltaTime * bobSpeed), Mathf.Lerp(joint.localPosition.y, jointOriginalPos.y, Time.deltaTime * bobSpeed), Mathf.Lerp(joint.localPosition.z, jointOriginalPos.z, Time.deltaTime * bobSpeed));
        }
    }

    private void HandleFootsteps()
    {
        // Safety checks - ensure we have audio source and are grounded
        if (footstepAudioSource == null || !isGrounded)
        {
            return;
        }

        // Check if player is pressing movement keys (INPUT-BASED, not velocity-based)
        float horizontalInput = Input.GetAxis("Horizontal");
        float verticalInput = Input.GetAxis("Vertical");
        bool hasMovementInput = (Mathf.Abs(horizontalInput) > 0.1f || Mathf.Abs(verticalInput) > 0.1f);

        if (hasMovementInput && Time.time >= nextFootstepTime)
        {
            // Determine which footstep sound to play based on sprint state
            AudioClip footstepToPlay = null;
            float footstepInterval = walkFootstepInterval;

            // Sprint audio: Only play when BOTH sprinting AND pressing movement keys (Shift + WASD)
            if (isSprinting && sprintFootstepSound != null)
            {
                footstepToPlay = sprintFootstepSound;
                footstepInterval = sprintFootstepInterval;
                Debug.Log("Playing sprint footstep sound (Shift + WASD pressed)");
            }
            // Walk audio: Play when pressing movement keys WITHOUT sprint (WASD only)
            else if (walkFootstepSound != null)
            {
                footstepToPlay = walkFootstepSound;
                footstepInterval = walkFootstepInterval;
                Debug.Log("Playing walk footstep sound (WASD pressed, no Shift)");
            }

            // Play the footstep sound if we have a valid clip
            if (footstepToPlay != null)
            {
                footstepAudioSource.PlayOneShot(footstepToPlay);
                nextFootstepTime = Time.time + footstepInterval;
            }
        }

        // Reset footstep timer when not pressing movement keys
        if (!hasMovementInput)
        {
            nextFootstepTime = 0f;
        }
    }

    private void HandleExhaustedBreathing()
    {
        // Safety checks - ensure we have audio source and clip
        if (breathingAudioSource == null || exhaustedBreathingSound == null)
        {
            if (isFatigued && Time.frameCount % 300 == 0) // Log warning every 5 seconds at 60fps
            {
                Debug.LogWarning("[FirstPersonController] Cannot play exhausted breathing - AudioSource or AudioClip is missing!", this);
            }
            return;
        }

        // If player is fatigued, play and manage breathing audio
        if (isFatigued)
        {
            // Start playing the breathing loop if not already playing
            if (!isExhaustedBreathingPlaying)
            {
                Debug.Log("[FirstPersonController] Starting exhausted breathing - stamina depleted", this);
                breathingAudioSource.clip = exhaustedBreathingSound;
                breathingAudioSource.loop = true;
                breathingAudioSource.Play();
                isExhaustedBreathingPlaying = true;
            }

            // Calculate fade out based on stamina recovery (0% to 30%)
            float staminaPercent = sprintRemaining / sprintDuration;

            // Volume lerps from 1.0 (at 0% stamina) to 0.0 (at 30% stamina)
            // Using inverse lerp to map stamina percentage to volume
            if (staminaPercent <= breathingFadeOutThreshold)
            {
                float volumeFactor = 1f - (staminaPercent / breathingFadeOutThreshold);
                breathingAudioSource.volume = Mathf.Clamp01(volumeFactor);

                // Debug log every second (60 frames at 60fps)
                if (Time.frameCount % 60 == 0)
                {
                    Debug.Log($"[FirstPersonController] Fatigued - Stamina: {staminaPercent:P0}, Volume: {breathingAudioSource.volume:F2}", this);
                }
            }
        }
        else
        {
            // Stop breathing audio when no longer fatigued
            if (isExhaustedBreathingPlaying)
            {
                Debug.Log("[FirstPersonController] Stopping exhausted breathing - stamina recovered", this);
                breathingAudioSource.Stop();
                breathingAudioSource.volume = 1f; // Reset volume for next time
                isExhaustedBreathingPlaying = false;
            }
        }
    }

    private void HandleExhaustion()
    {
        // Determine target speed multiplier based on fatigue state
        float targetMultiplier = isFatigued ? exhaustedSpeedReduction : 1f;

        // Smoothly lerp to target multiplier
        float previousMultiplier = currentSpeedMultiplier;
        currentSpeedMultiplier = Mathf.Lerp(currentSpeedMultiplier, targetMultiplier,
                                           exhaustionTransitionSpeed * Time.deltaTime);

        // Log state changes
        if (previousMultiplier >= 0.99f && currentSpeedMultiplier < 0.99f)
        {
            Debug.Log($"[FirstPersonController] Exhaustion started - Speed reducing to {exhaustedSpeedReduction:P0}", this);
        }
        else if (previousMultiplier < 0.99f && currentSpeedMultiplier >= 0.99f)
        {
            Debug.Log("[FirstPersonController] Exhaustion ended - Speed restored to normal", this);
        }
    }


}



// Custom Editor
#if UNITY_EDITOR
    [CustomEditor(typeof(FirstPersonController)), InitializeOnLoadAttribute]
    public class FirstPersonControllerEditor : Editor
    {
    FirstPersonController fpc;
    SerializedObject SerFPC;

    private void OnEnable()
    {
        fpc = (FirstPersonController)target;
        SerFPC = new SerializedObject(fpc);
    }

    public override void OnInspectorGUI()
    {
        SerFPC.Update();

        EditorGUILayout.Space();
        GUILayout.Label("Modular First Person Controller", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 16 });
        GUILayout.Label("By Jess Case", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Normal, fontSize = 12 });
        GUILayout.Label("version 1.0.1", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Normal, fontSize = 12 });
        EditorGUILayout.Space();

        #region Camera Setup

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("Camera Setup", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));
        EditorGUILayout.Space();

        fpc.playerCamera = (Camera)EditorGUILayout.ObjectField(new GUIContent("Camera", "Camera attached to the controller."), fpc.playerCamera, typeof(Camera), true);
        fpc.fov = EditorGUILayout.Slider(new GUIContent("Field of View", "The camera�s view angle. Changes the player camera directly."), fpc.fov, fpc.zoomFOV, 179f);
        fpc.cameraCanMove = EditorGUILayout.ToggleLeft(new GUIContent("Enable Camera Rotation", "Determines if the camera is allowed to move."), fpc.cameraCanMove);

        GUI.enabled = fpc.cameraCanMove;
        fpc.invertCamera = EditorGUILayout.ToggleLeft(new GUIContent("Invert Camera Rotation", "Inverts the up and down movement of the camera."), fpc.invertCamera);
        fpc.mouseSensitivity = EditorGUILayout.Slider(new GUIContent("Look Sensitivity", "Determines how sensitive the mouse movement is."), fpc.mouseSensitivity, .1f, 10f);
        fpc.maxLookAngle = EditorGUILayout.Slider(new GUIContent("Max Look Angle", "Determines the max and min angle the player camera is able to look."), fpc.maxLookAngle, 40, 90);
        GUI.enabled = true;

        fpc.lockCursor = EditorGUILayout.ToggleLeft(new GUIContent("Lock and Hide Cursor", "Turns off the cursor visibility and locks it to the middle of the screen."), fpc.lockCursor);

        fpc.crosshair = EditorGUILayout.ToggleLeft(new GUIContent("Auto Crosshair", "Determines if the basic crosshair will be turned on, and sets is to the center of the screen."), fpc.crosshair);

        // Only displays crosshair options if crosshair is enabled
        if(fpc.crosshair) 
        { 
            EditorGUI.indentLevel++; 
            EditorGUILayout.BeginHorizontal(); 
            EditorGUILayout.PrefixLabel(new GUIContent("Crosshair Image", "Sprite to use as the crosshair.")); 
            fpc.crosshairImage = (Sprite)EditorGUILayout.ObjectField(fpc.crosshairImage, typeof(Sprite), false);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            fpc.crosshairColor = EditorGUILayout.ColorField(new GUIContent("Crosshair Color", "Determines the color of the crosshair."), fpc.crosshairColor);
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--; 
        }

        EditorGUILayout.Space();

        #region Camera Zoom Setup

        GUILayout.Label("Zoom", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));

        fpc.enableZoom = EditorGUILayout.ToggleLeft(new GUIContent("Enable Zoom", "Determines if the player is able to zoom in while playing."), fpc.enableZoom);

        GUI.enabled = fpc.enableZoom;
        fpc.holdToZoom = EditorGUILayout.ToggleLeft(new GUIContent("Hold to Zoom", "Requires the player to hold the zoom key instead if pressing to zoom and unzoom."), fpc.holdToZoom);
        fpc.zoomKey = (KeyCode)EditorGUILayout.EnumPopup(new GUIContent("Zoom Key", "Determines what key is used to zoom."), fpc.zoomKey);
        fpc.zoomFOV = EditorGUILayout.Slider(new GUIContent("Zoom FOV", "Determines the field of view the camera zooms to."), fpc.zoomFOV, .1f, fpc.fov);
        fpc.zoomStepTime = EditorGUILayout.Slider(new GUIContent("Step Time", "Determines how fast the FOV transitions while zooming in."), fpc.zoomStepTime, .1f, 10f);
        GUI.enabled = true;

        #endregion

        #endregion

        #region Movement Setup

        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("Movement Setup", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));
        EditorGUILayout.Space();

        fpc.playerCanMove = EditorGUILayout.ToggleLeft(new GUIContent("Enable Player Movement", "Determines if the player is allowed to move."), fpc.playerCanMove);

        GUI.enabled = fpc.playerCanMove;
        fpc.walkSpeed = EditorGUILayout.Slider(new GUIContent("Walk Speed", "Determines how fast the player will move while walking."), fpc.walkSpeed, .1f, fpc.sprintSpeed);
        GUI.enabled = true;

        EditorGUILayout.Space();

        #region Sprint

        GUILayout.Label("Sprint", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));

        fpc.enableSprint = EditorGUILayout.ToggleLeft(new GUIContent("Enable Sprint", "Determines if the player is allowed to sprint."), fpc.enableSprint);

        GUI.enabled = fpc.enableSprint;
        fpc.unlimitedSprint = EditorGUILayout.ToggleLeft(new GUIContent("Unlimited Sprint", "Determines if 'Sprint Duration' is enabled. Turning this on will allow for unlimited sprint."), fpc.unlimitedSprint);
        fpc.sprintKey = (KeyCode)EditorGUILayout.EnumPopup(new GUIContent("Sprint Key", "Determines what key is used to sprint."), fpc.sprintKey);
        fpc.sprintSpeed = EditorGUILayout.Slider(new GUIContent("Sprint Speed", "Determines how fast the player will move while sprinting."), fpc.sprintSpeed, fpc.walkSpeed, 20f);

        //GUI.enabled = !fpc.unlimitedSprint;
        fpc.sprintDuration = EditorGUILayout.Slider(new GUIContent("Sprint Duration", "Determines how long the player can sprint while unlimited sprint is disabled."), fpc.sprintDuration, 1f, 20f);
        fpc.sprintCooldown = EditorGUILayout.Slider(new GUIContent("Sprint Cooldown", "Determines how long the recovery time is when the player runs out of sprint."), fpc.sprintCooldown, .1f, fpc.sprintDuration);
        //GUI.enabled = true;

        fpc.sprintFOV = EditorGUILayout.Slider(new GUIContent("Sprint FOV", "Determines the field of view the camera changes to while sprinting."), fpc.sprintFOV, fpc.fov, 179f);
        fpc.sprintFOVStepTime = EditorGUILayout.Slider(new GUIContent("Step Time", "Determines how fast the FOV transitions while sprinting."), fpc.sprintFOVStepTime, .1f, 20f);

        fpc.useSprintBar = EditorGUILayout.ToggleLeft(new GUIContent("Use Sprint Bar", "Determines if the default sprint bar will appear on screen."), fpc.useSprintBar);

        // Only displays sprint bar options if sprint bar is enabled
        if(fpc.useSprintBar)
        {
            EditorGUI.indentLevel++;

            EditorGUILayout.BeginHorizontal();
            fpc.hideBarWhenFull = EditorGUILayout.ToggleLeft(new GUIContent("Hide Full Bar", "Hides the sprint bar when sprint duration is full, and fades the bar in when sprinting. Disabling this will leave the bar on screen at all times when the sprint bar is enabled."), fpc.hideBarWhenFull);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(new GUIContent("Bar BG", "Object to be used as sprint bar background."));
            fpc.sprintBarBG = (Image)EditorGUILayout.ObjectField(fpc.sprintBarBG, typeof(Image), true);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.PrefixLabel(new GUIContent("Bar", "Object to be used as sprint bar foreground."));
            fpc.sprintBar = (Image)EditorGUILayout.ObjectField(fpc.sprintBar, typeof(Image), true);
            EditorGUILayout.EndHorizontal();


            EditorGUILayout.BeginHorizontal();
            fpc.sprintBarWidthPercent = EditorGUILayout.Slider(new GUIContent("Bar Width", "Determines the width of the sprint bar."), fpc.sprintBarWidthPercent, .1f, .5f);
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal();
            fpc.sprintBarHeightPercent = EditorGUILayout.Slider(new GUIContent("Bar Height", "Determines the height of the sprint bar."), fpc.sprintBarHeightPercent, .001f, .025f);
            EditorGUILayout.EndHorizontal();
            EditorGUI.indentLevel--;
        }
        GUI.enabled = true;

        EditorGUILayout.Space();

        #endregion

        #region Jump

        GUILayout.Label("Jump", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));

        fpc.enableJump = EditorGUILayout.ToggleLeft(new GUIContent("Enable Jump", "Determines if the player is allowed to jump."), fpc.enableJump);

        GUI.enabled = fpc.enableJump;
        fpc.jumpKey = (KeyCode)EditorGUILayout.EnumPopup(new GUIContent("Jump Key", "Determines what key is used to jump."), fpc.jumpKey);
        fpc.jumpPower = EditorGUILayout.Slider(new GUIContent("Jump Power", "Determines how high the player will jump."), fpc.jumpPower, .1f, 20f);
        GUI.enabled = true;

        EditorGUILayout.Space();

        #endregion

        #region Crouch

        GUILayout.Label("Crouch", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));

        fpc.enableCrouch = EditorGUILayout.ToggleLeft(new GUIContent("Enable Crouch", "Determines if the player is allowed to crouch."), fpc.enableCrouch);

        GUI.enabled = fpc.enableCrouch;
        fpc.holdToCrouch = EditorGUILayout.ToggleLeft(new GUIContent("Hold To Crouch", "Requires the player to hold the crouch key instead if pressing to crouch and uncrouch."), fpc.holdToCrouch);
        fpc.crouchKey = (KeyCode)EditorGUILayout.EnumPopup(new GUIContent("Crouch Key", "Determines what key is used to crouch."), fpc.crouchKey);
        fpc.crouchHeight = EditorGUILayout.Slider(new GUIContent("Crouch Height", "Determines the y scale of the player object when crouched."), fpc.crouchHeight, .1f, 1);
        fpc.speedReduction = EditorGUILayout.Slider(new GUIContent("Speed Reduction", "Determines the percent 'Walk Speed' is reduced by. 1 being no reduction, and .5 being half."), fpc.speedReduction, .1f, 1);
        GUI.enabled = true;

        #endregion

        #endregion

        #region Head Bob

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("Head Bob Setup", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));
        EditorGUILayout.Space();

        fpc.enableHeadBob = EditorGUILayout.ToggleLeft(new GUIContent("Enable Head Bob", "Determines if the camera will bob while the player is walking."), fpc.enableHeadBob);
        

        GUI.enabled = fpc.enableHeadBob;
        fpc.joint = (Transform)EditorGUILayout.ObjectField(new GUIContent("Camera Joint", "Joint object position is moved while head bob is active."), fpc.joint, typeof(Transform), true);
        fpc.bobSpeed = EditorGUILayout.Slider(new GUIContent("Speed", "Determines how often a bob rotation is completed."), fpc.bobSpeed, 1, 20);
        fpc.bobAmount = EditorGUILayout.Vector3Field(new GUIContent("Bob Amount", "Determines the amount the joint moves in both directions on every axes."), fpc.bobAmount);
        GUI.enabled = true;

        #endregion

        #region Audio System

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("Audio System", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));
        EditorGUILayout.Space();

        // Audio Sources
        GUILayout.Label("Audio Sources", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold, fontSize = 12 }, GUILayout.ExpandWidth(true));
        fpc.footstepAudioSource = (AudioSource)EditorGUILayout.ObjectField(new GUIContent("Footstep Audio Source", "Audio source for footstep sounds (walk and sprint)"), fpc.footstepAudioSource, typeof(AudioSource), true);
        fpc.breathingAudioSource = (AudioSource)EditorGUILayout.ObjectField(new GUIContent("Breathing Audio Source", "Audio source for breathing sounds (exhausted/fatigued state)"), fpc.breathingAudioSource, typeof(AudioSource), true);

        EditorGUILayout.Space();

        // Footstep Audio
        GUILayout.Label("Footstep Audio", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold, fontSize = 12 }, GUILayout.ExpandWidth(true));
        fpc.walkFootstepSound = (AudioClip)EditorGUILayout.ObjectField(new GUIContent("Walk Footstep Sound", "Sound played when walking"), fpc.walkFootstepSound, typeof(AudioClip), false);
        fpc.sprintFootstepSound = (AudioClip)EditorGUILayout.ObjectField(new GUIContent("Sprint Footstep Sound", "Sound played when sprinting"), fpc.sprintFootstepSound, typeof(AudioClip), false);
        fpc.walkFootstepInterval = EditorGUILayout.Slider(new GUIContent("Walk Interval", "Time interval between walk footstep sounds in seconds"), fpc.walkFootstepInterval, 0.1f, 2f);
        fpc.sprintFootstepInterval = EditorGUILayout.Slider(new GUIContent("Sprint Interval", "Time interval between sprint footstep sounds in seconds"), fpc.sprintFootstepInterval, 0.1f, 2f);

        EditorGUILayout.Space();

        // Stamina Audio
        GUILayout.Label("Stamina Audio", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold, fontSize = 12 }, GUILayout.ExpandWidth(true));
        fpc.exhaustedBreathingSound = (AudioClip)EditorGUILayout.ObjectField(new GUIContent("Exhausted Breathing", "Looping sound played when stamina is exhausted"), fpc.exhaustedBreathingSound, typeof(AudioClip), false);
        fpc.breathingFadeOutThreshold = EditorGUILayout.Slider(new GUIContent("Breathing Fade Out", "Stamina percentage where breathing starts to fade out"), fpc.breathingFadeOutThreshold, 0.1f, 0.5f);

        #endregion

        #region Exhaustion System

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("", GUI.skin.horizontalSlider);
        GUILayout.Label("Exhaustion System", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleCenter, fontStyle = FontStyle.Bold, fontSize = 13 }, GUILayout.ExpandWidth(true));
        EditorGUILayout.Space();

        GUILayout.Label("Speed Reduction", new GUIStyle(GUI.skin.label) { alignment = TextAnchor.MiddleLeft, fontStyle = FontStyle.Bold, fontSize = 12 }, GUILayout.ExpandWidth(true));
        fpc.exhaustedSpeedReduction = EditorGUILayout.Slider(new GUIContent("Exhausted Speed", "Speed multiplier when exhausted (0.5 = 50% speed, matching crouch)"), fpc.exhaustedSpeedReduction, 0.3f, 0.9f);
        fpc.exhaustionTransitionSpeed = EditorGUILayout.Slider(new GUIContent("Transition Speed", "How quickly speed transitions when entering/exiting exhaustion"), fpc.exhaustionTransitionSpeed, 0.5f, 5f);

        EditorGUILayout.Space();
        EditorGUILayout.HelpBox("When stamina depletes completely, movement speed is reduced until stamina recovers to 30%. The speed reduction matches the breathing fade timing.", MessageType.Info);

        #endregion

        //Sets any changes from the prefab
        if(GUI.changed)
        {
            EditorUtility.SetDirty(fpc);
            Undo.RecordObject(fpc, "FPC Change");
            SerFPC.ApplyModifiedProperties();
        }
    }

}

#endif
