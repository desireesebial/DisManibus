using UnityEngine;

namespace SaveLoad
{
    public abstract class PersistentEntity : MonoBehaviour
    {
        [SerializeField] private string persistentId;

        public string PersistentId => persistentId;

        protected virtual void Awake()
        {
            if (string.IsNullOrEmpty(persistentId))
            {
                Debug.LogWarning($"PersistentEntity '{name}' has no ID set.");
            }
        }

        protected virtual void OnEnable()
        {
            PersistentWorldState.Instance.RegisterEntity(this);

            if (!string.IsNullOrEmpty(persistentId) &&
                PersistentWorldState.Instance.HasDuplicateId(persistentId, this))
            {
                Debug.LogWarning($"Duplicate PersistentEntity ID '{persistentId}' detected on '{name}'.");
            }
        }

        protected virtual void OnDisable()
        {
            if (PersistentWorldState.Instance != null)
            {
                PersistentWorldState.Instance.UnregisterEntity(this);
            }
        }

        public abstract void CaptureState(PersistentWorldState worldState);
        public abstract void ApplyState(PersistentWorldState worldState);
    }
}
