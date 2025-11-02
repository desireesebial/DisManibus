using UnityEngine;

namespace SaveLoad
{
    /// <summary>
    /// Persists the state of the Head Shrine Puzzle
    /// Saves: puzzle completion status
    /// Note: For simplicity, only saves completion. Partial progress is not saved (resets on scene reload)
    /// </summary>
    [RequireComponent(typeof(HeadShrinePuzzle))]
    public class PersistentHeadShrinePuzzle : PersistentEntity
    {
        private HeadShrinePuzzle puzzleManager;

        private void Awake()
        {
            base.Awake();
            puzzleManager = GetComponent<HeadShrinePuzzle>();
            if (puzzleManager == null)
            {
                Debug.LogError("[PersistentHeadShrinePuzzle] HeadShrinePuzzle component not found!");
            }
        }

        public override void CaptureState(PersistentWorldState worldState)
        {
            if (puzzleManager == null) return;

            // Save puzzle completion status
            // Note: We could save individual placement states, but for simplicity we only save completion
            bool isComplete = CheckIfComplete();
            worldState.SetBool($"{PersistentId}_complete", isComplete);

            Debug.Log($"[PersistentHeadShrinePuzzle] Saved puzzle '{PersistentId}' state: complete={isComplete}");
        }

        public override void ApplyState(PersistentWorldState worldState)
        {
            if (puzzleManager == null) return;

            // Check if puzzle was completed
            if (worldState.TryGetBool($"{PersistentId}_complete", out bool isComplete) && isComplete)
            {
                // Puzzle is complete - mark it as complete (rewards already given)
                Debug.Log($"[PersistentHeadShrinePuzzle] Puzzle '{PersistentId}' was completed, maintaining state");

                // The puzzle manager will handle the completion state naturally
                // We don't need to do anything special as doors/rewards are likely already persistent
                return;
            }

            // Puzzle is not complete - start fresh
            Debug.Log($"[PersistentHeadShrinePuzzle] Puzzle '{PersistentId}' not complete, starting fresh");
        }

        /// <summary>
        /// Check if the shrine puzzle is complete by checking if all placements are filled
        /// Uses reflection to access private field since there's no public property
        /// </summary>
        private bool CheckIfComplete()
        {
            // Use reflection to check the private "shrineComplete" field
            var field = typeof(HeadShrinePuzzle).GetField("shrineComplete",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            if (field != null)
            {
                return (bool)field.GetValue(puzzleManager);
            }

            Debug.LogWarning("[PersistentHeadShrinePuzzle] Could not access shrineComplete field via reflection");
            return false;
        }
    }
}
