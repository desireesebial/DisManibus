using UnityEngine;

namespace SaveLoad
{
    /// <summary>
    /// Bridges doorscript with the save system, making doors persistent across save/load cycles.
    /// Saves both the open/closed state AND the locked/unlocked state.
    /// Attach this to doorscript objects that should persist their state.
    /// </summary>
    [RequireComponent(typeof(doorscript))]
    public class PersistentDoorBridge : PersistentEntity
    {
        private doorscript door;

        protected override void Awake()
        {
            base.Awake();
            door = GetComponent<doorscript>();

            if (door == null)
            {
                Debug.LogError($"[PersistentDoorBridge] No doorscript found on '{name}'!");
            }
        }

        private void Start()
        {
            // Apply saved state after doorscript initializes
            if (PersistentWorldState.Instance != null)
            {
                ApplyState(PersistentWorldState.Instance);
            }
        }

        /// <summary>
        /// Captures the current state of the door to be saved.
        /// Saves both the open state and the locked state.
        /// </summary>
        public override void CaptureState(PersistentWorldState worldState)
        {
            if (worldState == null || string.IsNullOrEmpty(PersistentId) || door == null)
                return;

            // Save open state using the door state system
            worldState.SetDoorState(PersistentId + "_open", door.isOpen);

            // Save locked state using the bool state system
            worldState.SetBool(PersistentId + "_locked", door.isLocked);

            Debug.Log($"[PersistentDoorBridge] Captured state for door '{name}' (ID: {PersistentId}): Open={door.isOpen}, Locked={door.isLocked}");
        }

        /// <summary>
        /// Applies the saved state to the door when loading.
        /// Restores both the open state and the locked state.
        /// </summary>
        public override void ApplyState(PersistentWorldState worldState)
        {
            if (worldState == null || string.IsNullOrEmpty(PersistentId) || door == null)
                return;

            // Restore locked state first
            if (worldState.TryGetBool(PersistentId + "_locked", out bool wasLocked))
            {
                door.isLocked = wasLocked;
                Debug.Log($"[PersistentDoorBridge] Restored lock state for door '{name}': Locked={wasLocked}");
            }

            // Restore open state
            if (worldState.TryGetDoorState(PersistentId + "_open", out bool wasOpen))
            {
                // Only update if the state is different from current
                if (wasOpen != door.isOpen)
                {
                    if (wasOpen)
                    {
                        door.ForceOpen();
                        Debug.Log($"[PersistentDoorBridge] Forced door '{name}' to open position");
                    }
                    else
                    {
                        door.ForceClose();
                        Debug.Log($"[PersistentDoorBridge] Forced door '{name}' to closed position");
                    }
                }
                else
                {
                    Debug.Log($"[PersistentDoorBridge] Door '{name}' already in correct state: Open={wasOpen}");
                }
            }
        }
    }
}
