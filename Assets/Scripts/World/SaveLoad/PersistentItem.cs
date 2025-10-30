using UnityEngine;

namespace SaveLoad
{
    /// <summary>
    /// Makes an item persistent across save/load cycles.
    /// When an item is picked up, it's marked in the save system and won't respawn.
    /// Attach this to ItemsPickable objects that should persist.
    /// </summary>
    public class PersistentItem : PersistentEntity
    {
        private void Start()
        {
            // If this item was already picked up in a previous session, destroy it
            CheckIfAlreadyPickedUp();
        }

        /// <summary>
        /// Checks if this item was already picked up and destroys it if so.
        /// </summary>
        private void CheckIfAlreadyPickedUp()
        {
            if (string.IsNullOrEmpty(PersistentId))
            {
                Debug.LogWarning($"[PersistentItem] Item '{name}' has no persistent ID assigned!");
                return;
            }

            if (PersistentWorldState.Instance != null &&
                PersistentWorldState.Instance.TryGetBool(PersistentId, out bool pickedUp) &&
                pickedUp)
            {
                Debug.Log($"[PersistentItem] Item '{name}' (ID: {PersistentId}) was already picked up. Destroying.");
                Destroy(gameObject);
            }
        }

        /// <summary>
        /// Call this when the item is picked up by the player.
        /// Marks the item as picked up in the save system.
        /// </summary>
        public void MarkAsPickedUp()
        {
            if (string.IsNullOrEmpty(PersistentId))
            {
                Debug.LogWarning($"[PersistentItem] Cannot mark item '{name}' as picked up - no persistent ID!");
                return;
            }

            if (PersistentWorldState.Instance != null)
            {
                PersistentWorldState.Instance.SetBool(PersistentId, true);
                Debug.Log($"[PersistentItem] Item '{name}' (ID: {PersistentId}) marked as picked up.");
            }
        }

        /// <summary>
        /// Items don't need to capture state - they're either present or destroyed.
        /// The pickup state is tracked via bool in PersistentWorldState.
        /// </summary>
        public override void CaptureState(PersistentWorldState worldState)
        {
            // Items are handled via bool states in MarkAsPickedUp()
            // No additional state capture needed
        }

        /// <summary>
        /// State is checked and applied in Start() via CheckIfAlreadyPickedUp().
        /// </summary>
        public override void ApplyState(PersistentWorldState worldState)
        {
            // State is applied in Start() method
        }
    }
}
