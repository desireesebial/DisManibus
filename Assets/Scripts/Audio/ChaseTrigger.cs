using UnityEngine;

public class ChaseTrigger : MonoBehaviour
{
    private AudioManager audioManager;

    void Start()
    {
        audioManager = AudioManager.Instance; // Grab the singleton
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
            audioManager?.StartChaseMode();
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
            audioManager?.EndChaseMode();
    }
}
