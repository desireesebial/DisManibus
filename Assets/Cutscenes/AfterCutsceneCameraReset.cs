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

        // calculate target position/rotation
        float cameraHeight = playerCamera.localPosition.y;
        Vector3 targetPos = cutsceneCam.position - new Vector3(0, cameraHeight, 0);
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

        // Restore Rigidbody state and zero velocity
        if (playerRb != null)
        {
            Debug.Log($"[AfterCutsceneCameraReset] Restoring Rigidbody - setting isKinematic to: {wasKinematic}");

            // Ensure constraints are correct (should only freeze rotation, not position)
            playerRb.constraints = RigidbodyConstraints.FreezeRotation;

            playerRb.isKinematic = wasKinematic;
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

            // Wake up the Rigidbody
            playerRb.WakeUp();

            Debug.Log($"[AfterCutsceneCameraReset] Rigidbody restored - isKinematic: {playerRb.isKinematic}, constraints: {playerRb.constraints}, drag: {playerRb.linearDamping}");
        }

        Debug.Log("[AfterCutsceneCameraReset] SmoothTeleport complete - waiting one physics frame before re-enabling controls");

        // Wait one physics frame to let everything settle
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
