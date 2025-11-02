using UnityEngine;

namespace DisManibus.World.SceneTransition
{
    /// <summary>
    /// Manages portal unlock state across all scenes
    /// Tracks whether the 1st floor sequential torch puzzle has been completed
    /// </summary>
    public static class PortalManager
    {
        private const string PORTAL_UNLOCK_KEY = "AllPortalsUnlocked";

        /// <summary>
        /// Returns true if the sequential torch puzzle has been completed
        /// and all portals should be unlocked
        /// </summary>
        public static bool ArePortalsUnlocked()
        {
            return PlayerPrefs.GetInt(PORTAL_UNLOCK_KEY, 0) == 1;
        }

        /// <summary>
        /// Unlocks all portals across all scenes
        /// Called when the sequential torch puzzle is completed
        /// </summary>
        public static void UnlockAllPortals()
        {
            PlayerPrefs.SetInt(PORTAL_UNLOCK_KEY, 1);
            PlayerPrefs.Save();
            Debug.Log("All portals have been unlocked for bidirectional travel!");
        }

        /// <summary>
        /// Locks all portals (for testing or new game)
        /// </summary>
        public static void LockAllPortals()
        {
            PlayerPrefs.SetInt(PORTAL_UNLOCK_KEY, 0);
            PlayerPrefs.Save();
            Debug.Log("All portals have been locked.");
        }

        /// <summary>
        /// Resets portal state (useful for new game)
        /// </summary>
        public static void ResetPortalState()
        {
            LockAllPortals();
        }
    }
}
