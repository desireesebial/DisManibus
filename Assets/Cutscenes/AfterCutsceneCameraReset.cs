using UnityEngine;
using System.Collections;

public class AfterCutsceneCameraReset : MonoBehaviour
{
    public GameObject player;                    // FirstPersonController (the capsule)
    public Transform cutsceneCam;                // VCutscene_Main (Transform)
    public Camera gameplayCamera;                // PlayerCamera (regular Camera)
    public Transform playerCamera;               // PlayerCamera (child of player)
    public Behaviour[] behavioursToEnableAfterCutscene;

    private Rigidbody playerRb;                  // Player's Rigidbody component

    [Header("Smooth Transition")]
    public float transitionDuration = 0.5f;      // how long to blend (in seconds)
    public AnimationCurve easeCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Ground Detection")]
    [Tooltip("Maximum distance to raycast downward when detecting ground")]
    public float groundDetectionDistance = 50f;

    [Tooltip("Height offset above detected ground to place the player (capsule base height)")]
    public float playerHeightAboveGround = 1f;

    [Tooltip("Layer mask for ground detection (leave default for all layers)")]
    public LayerMask groundLayerMask = ~0; // ~0 means all layers

    [Tooltip("Enable to see raycast visualization in Scene view")]
    public bool showGroundDetectionDebug = true;

    public void OnCutsceneEnd()
    {
        StartCoroutine(SmoothTeleport());
    }

    private IEnumerator SmoothTeleport()
    {
        if (!player || !cutsceneCam || !playerCamera) yield break;

        Debug.Log("[AfterCutsceneCameraReset] SmoothTeleport started");

        // Get player Rigidbody if not cached
        if (playerRb == null)
        {
            playerRb = player.GetComponent<Rigidbody>();
            Debug.Log($"[AfterCutsceneCameraReset] Cached player Rigidbody: {playerRb != null}");
        }

        // Disable any Animator on the player (Timeline may have used it)
        Animator playerAnimator = player.GetComponent<Animator>();
        if (playerAnimator != null)
        {
            Debug.Log($"[AfterCutsceneCameraReset] Disabling player Animator (was enabled: {playerAnimator.enabled})");
            playerAnimator.enabled = false;
        }

        // Disable player controls during the blend
        if (behavioursToEnableAfterCutscene != null)
            foreach (var b in behavioursToEnableAfterCutscene)
                if (b) b.enabled = false;

        // Store original Rigidbody state and prepare for transition
        bool wasKinematic = false;
        if (playerRb != null)
        {
            wasKinematic = playerRb.isKinematic;
            Debug.Log($"[AfterCutsceneCameraReset] Rigidbody state - isKinematic: {playerRb.isKinematic}, constraints: {playerRb.constraints}");
            Debug.Log($"[AfterCutsceneCameraReset] Clearing velocities - linear: {playerRb.linearVelocity}, angular: {playerRb.angularVelocity}");

            playerRb.linearVelocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;
            playerRb.isKinematic = true;

            Debug.Log($"[AfterCutsceneCameraReset] Set to kinematic for smooth transition");
        }

        // calculate target position/rotation with ground detection
        float cameraHeight = playerCamera.localPosition.y;
        Vector3 cutsceneCamPos = cutsceneCam.position;
        Vector3 targetPos;

        Debug.Log($"[AfterCutsceneCameraReset] Cutscene camera final position: {cutsceneCamPos}");
        Debug.Log($"[AfterCutsceneCameraReset] Performing ground detection raycast...");

        // Perform raycast to detect ground below the cutscene camera
        RaycastHit hit;
        Vector3 rayStart = cutsceneCamPos + Vector3.up * 2f; // Start slightly above to account for camera being inside/near ground
        Vector3 rayDirection = Vector3.down;
        float rayDistance = groundDetectionDistance + 2f; // Add the extra 2m we started above

        bool groundDetected = Physics.Raycast(rayStart, rayDirection, out hit, rayDistance, groundLayerMask);

        if (groundDetected)
        {
            // Ground found! Place player on it with the specified offset
            targetPos = hit.point + Vector3.up * playerHeightAboveGround;
            Debug.Log($"[AfterCutsceneCameraReset] Ground detected at {hit.point}");
            Debug.Log($"[AfterCutsceneCameraReset] Hit object: {hit.collider.gameObject.name}");
            Debug.Log($"[AfterCutsceneCameraReset] Player will be placed at: {targetPos}");

            // Debug visualization
            if (showGroundDetectionDebug)
            {
                Debug.DrawRay(rayStart, rayDirection * hit.distance, Color.green, 10f);
                Debug.DrawLine(hit.point, targetPos, Color.cyan, 10f);
            }
        }
        else
        {
            // No ground detected - fallback to original calculation
            targetPos = cutsceneCamPos - new Vector3(0, cameraHeight, 0);
            Debug.LogWarning($"[AfterCutsceneCameraReset] NO GROUND DETECTED within {rayDistance}m!");
            Debug.LogWarning($"[AfterCutsceneCameraReset] Falling back to original position calculation: {targetPos}");
            Debug.LogWarning($"[AfterCutsceneCameraReset] Player may fall! Check cutscene camera final position or increase groundDetectionDistance.");

            // Debug visualization for failed raycast
            if (showGroundDetectionDebug)
            {
                Debug.DrawRay(rayStart, rayDirection * rayDistance, Color.red, 10f);
            }
        }

        Vector3 startPos = player.transform.position;

        Vector3 forward = cutsceneCam.forward;
        forward.y = 0f;
        Quaternion startRot = player.transform.rotation;
        Quaternion targetRot = Quaternion.LookRotation(forward);

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / transitionDuration;
            float eased = easeCurve.Evaluate(t);

            Vector3 newPos = Vector3.Lerp(startPos, targetPos, eased);
            Quaternion newRot = Quaternion.Slerp(startRot, targetRot, eased);

            // Use Rigidbody.MovePosition if available, otherwise use transform
            if (playerRb != null)
            {
                playerRb.MovePosition(newPos);
                playerRb.MoveRotation(newRot);
            }
            else
            {
                player.transform.position = newPos;
                player.transform.rotation = newRot;
            }
            yield return null;
        }

        // snap to exact final position/rotation
        if (playerRb != null)
        {
            playerRb.MovePosition(targetPos);
            playerRb.MoveRotation(targetRot);
        }
        else
        {
            player.transform.position = targetPos;
            player.transform.rotation = targetRot;
        }

        // CRITICAL: Keep player kinematic and wait for physics to fully stabilize
        Debug.Log("[AfterCutsceneCameraReset] Player positioned - now stabilizing physics before enabling movement");

        // Wait multiple physics frames to ensure ground collision is established
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();
        yield return new WaitForFixedUpdate();

        Debug.Log("[AfterCutsceneCameraReset] Physics frames complete - verifying ground contact");

        // Verify the player is still above ground
        if (playerRb != null)
        {
            RaycastHit verifyHit;
            Vector3 verifyRayStart = player.transform.position;
            float verifyDistance = 2f;

            if (Physics.Raycast(verifyRayStart, Vector3.down, out verifyHit, verifyDistance, groundLayerMask))
            {
                Debug.Log($"[AfterCutsceneCameraReset] Verification: Ground is {verifyHit.distance}m below player");

                // If player is somehow sinking or floating, snap them to correct height
                if (verifyHit.distance < 0.5f || verifyHit.distance > 1.5f)
                {
                    Debug.LogWarning($"[AfterCutsceneCameraReset] Player height anomaly detected! Adjusting position...");
                    Vector3 correctedPos = verifyHit.point + Vector3.up * playerHeightAboveGround;
                    playerRb.MovePosition(correctedPos);
                    yield return new WaitForFixedUpdate();
                }
            }
            else
            {
                Debug.LogError($"[AfterCutsceneCameraReset] CRITICAL: No ground detected below player during verification! Player may fall!");
            }
        }

        // Restore Rigidbody state and zero velocity
        if (playerRb != null)
        {
            Debug.Log($"[AfterCutsceneCameraReset] Restoring Rigidbody - setting isKinematic to: {wasKinematic}");

            // Ensure constraints are correct (should only freeze rotation, not position)
            playerRb.constraints = RigidbodyConstraints.FreezeRotation;

            // Clear velocities one more time before unfreezing
            playerRb.linearVelocity = Vector3.zero;
            playerRb.angularVelocity = Vector3.zero;

            // Ensure drag is correct
            if (playerRb.linearDamping > 0.1f)
            {
                Debug.LogWarning($"[AfterCutsceneCameraReset] Rigidbody drag was abnormal: {playerRb.linearDamping}, resetting to 0");
                playerRb.linearDamping = 0f;
            }

            // Force physics engine to sync
            Physics.SyncTransforms();

            // NOW restore kinematic state
            playerRb.isKinematic = wasKinematic;

            // Wake up the Rigidbody
            playerRb.WakeUp();

            Debug.Log($"[AfterCutsceneCameraReset] Rigidbody restored - isKinematic: {playerRb.isKinematic}, constraints: {playerRb.constraints}, drag: {playerRb.linearDamping}");
        }

        Debug.Log("[AfterCutsceneCameraReset] SmoothTeleport complete - waiting one more physics frame before re-enabling controls");

        // Wait one more physics frame to let everything settle
        yield return new WaitForFixedUpdate();

        Debug.Log("[AfterCutsceneCameraReset] Physics frame complete - re-enabling controls now");

        // Re-enable controls
        if (behavioursToEnableAfterCutscene != null)
            foreach (var b in behavioursToEnableAfterCutscene)
                if (b) b.enabled = true;

        // Explicitly ensure FirstPersonController is ready to move
        FirstPersonController fpc = player.GetComponent<FirstPersonController>();
        if (fpc != null)
        {
            Debug.Log($"[AfterCutsceneCameraReset] FirstPersonController found - playerCanMove was: {fpc.playerCanMove}, cameraCanMove was: {fpc.cameraCanMove}");
            fpc.playerCanMove = true;
            fpc.cameraCanMove = true;
            Debug.Log($"[AfterCutsceneCameraReset] Set playerCanMove=true, cameraCanMove=true");

            // Check and fix movement parameters
            Debug.Log($"[AfterCutsceneCameraReset] Movement parameters - walkSpeed: {fpc.walkSpeed}, maxVelocityChange: {fpc.maxVelocityChange}");

            if (fpc.maxVelocityChange < 1f)
            {
                Debug.LogWarning($"[AfterCutsceneCameraReset] maxVelocityChange was corrupted ({fpc.maxVelocityChange}), resetting to 10");
                fpc.maxVelocityChange = 10f;
            }

            if (fpc.walkSpeed < 1f)
            {
                Debug.LogWarning($"[AfterCutsceneCameraReset] walkSpeed was corrupted ({fpc.walkSpeed}), resetting to 5");
                fpc.walkSpeed = 5f;
            }

            Debug.Log($"[AfterCutsceneCameraReset] Final movement parameters - walkSpeed: {fpc.walkSpeed}, maxVelocityChange: {fpc.maxVelocityChange}");
        }
        else
        {
            Debug.LogWarning("[AfterCutsceneCameraReset] FirstPersonController not found on player!");
        }

        // Switch cameras
        if (cutsceneCam) cutsceneCam.gameObject.SetActive(false);
        if (gameplayCamera) gameplayCamera.enabled = true;

        Debug.Log("[AfterCutsceneCameraReset] Complete - player should be able to move now");
    }
}
