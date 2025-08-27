using UnityEngine;

public class PlayerProximity : MonoBehaviour
{
    private AudioManager audioManager;

    void Start()
    {
        audioManager = AudioManager.Instance; // Grab the singleton
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            audioManager?.SetPlayerNearby(true);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            audioManager?.SetPlayerNearby(false);
    }
}
