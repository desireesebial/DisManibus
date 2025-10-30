using UnityEngine;
using UnityEngine.Events;

namespace World.SceneTransition
{
    [RequireComponent(typeof(Collider))]
    public class CheckpointTrigger : MonoBehaviour
    {
        [Header("References")]
        [SerializeField] private SaveLoad.SaveManager saveManager;
        [SerializeField] private string playerTag = "Player";

        [Header("Events")]
        [SerializeField] private UnityEvent onCheckpointReached;

        private bool hasSaved;

        private void Reset()
        {
            var collider = GetComponent<Collider>();
            collider.isTrigger = true;
        }

        private void Awake()
        {
            if (saveManager == null)
            {
                saveManager = FindAnyObjectByType<SaveLoad.SaveManager>();
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (!other.CompareTag(playerTag))
            {
                return;
            }

            if (hasSaved)
            {
                return;
            }

            if (saveManager == null)
            {
                Debug.LogWarning("CheckpointTrigger could not find SaveManager; no save performed.");
                return;
            }

            saveManager.SaveCheckpoint();
            hasSaved = true;
            onCheckpointReached?.Invoke();

#if UNITY_EDITOR
            Debug.Log($"Checkpoint saved at '{name}'.");
#endif
        }

        public void ResetCheckpoint()
        {
            hasSaved = false;
        }
    }
}
