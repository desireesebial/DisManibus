using UnityEngine;

public class CutsceneCoordinator : MonoBehaviour
{
    public GameObject player;   // The capsule (with collider + rigidbody)
    public Camera cutsceneCam;  // The camera used in Timeline

    public void OnCutsceneEnd()
    {
        // Move the player to where the cutscene camera ended
        player.transform.position = cutsceneCam.transform.position;

        // Align facing direction
        Vector3 lookDir = cutsceneCam.transform.forward;
        lookDir.y = 0f; // Keep only horizontal rotation
        player.transform.rotation = Quaternion.LookRotation(lookDir);
    }
}
