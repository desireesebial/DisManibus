using UnityEngine;
using System.Collections;

public class AfterCutsceneCameraReset : MonoBehaviour
{
    public GameObject player;                    // FirstPersonController (the capsule)
    public Transform cutsceneCam;                // VCutscene_Main (Transform)
    public Camera gameplayCamera;                // PlayerCamera (regular Camera)
    public Transform playerCamera;               // PlayerCamera (child of player)
    public Behaviour[] behavioursToEnableAfterCutscene;

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

        // Disable player controls during the blend
        if (behavioursToEnableAfterCutscene != null)
            foreach (var b in behavioursToEnableAfterCutscene)
                if (b) b.enabled = false;

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

            player.transform.position = Vector3.Lerp(startPos, targetPos, eased);
            player.transform.rotation = Quaternion.Slerp(startRot, targetRot, eased);
            yield return null;
        }

        // snap to exact final position/rotation
        player.transform.position = targetPos;
        player.transform.rotation = targetRot;

        // Re-enable controls
        if (behavioursToEnableAfterCutscene != null)
            foreach (var b in behavioursToEnableAfterCutscene)
                if (b) b.enabled = true;

        // Switch cameras
        if (cutsceneCam) cutsceneCam.gameObject.SetActive(false);
        if (gameplayCamera) gameplayCamera.enabled = true;
    }
}
