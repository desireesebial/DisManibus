using UnityEngine;

namespace SaveLoad
{
    [RequireComponent(typeof(Collider))]
    public class PersistentDoor : PersistentEntity
    {
        [SerializeField] private bool isOpen;
        [SerializeField] private Animator animator;
        [SerializeField] private string animatorOpenParameter = "IsOpen";

        private void Start()
        {
            ApplyVisualState();
        }

        public override void CaptureState(PersistentWorldState worldState)
        {
            if (worldState == null || string.IsNullOrEmpty(PersistentId))
                return;

            worldState.SetDoorState(PersistentId, isOpen);
        }

        public override void ApplyState(PersistentWorldState worldState)
        {
            if (worldState == null || string.IsNullOrEmpty(PersistentId))
                return;

            if (worldState.TryGetDoorState(PersistentId, out bool open))
            {
                isOpen = open;
                ApplyVisualState();
            }
        }

        public void SetOpen(bool open)
        {
            if (isOpen == open) return;

            isOpen = open;
            ApplyVisualState();
            PersistentWorldState.Instance?.SetDoorState(PersistentId, isOpen);
        }

        private void ApplyVisualState()
        {
            if (animator != null && !string.IsNullOrEmpty(animatorOpenParameter))
            {
                animator.SetBool(animatorOpenParameter, isOpen);
            }

            var collider = GetComponent<Collider>();
            if (collider != null)
            {
                collider.enabled = !isOpen;
            }
        }

        // Optional helper to toggle from events
        public void Toggle()
        {
            SetOpen(!isOpen);
        }
    }
}

