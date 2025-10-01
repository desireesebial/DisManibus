using UnityEngine;

namespace SaveLoad
{
    public class PersistentBoolState : PersistentEntity
    {
        [SerializeField] private bool value;
        [SerializeField] private UnityEngine.Events.UnityEvent<bool> onValueApplied;

        public override void CaptureState(PersistentWorldState worldState)
        {
            if (worldState == null || string.IsNullOrEmpty(PersistentId))
                return;

            worldState.SetBool(PersistentId, value);
        }

        public override void ApplyState(PersistentWorldState worldState)
        {
            if (worldState == null || string.IsNullOrEmpty(PersistentId))
                return;

            if (worldState.TryGetBool(PersistentId, out bool storedValue))
            {
                value = storedValue;
                onValueApplied?.Invoke(value);
            }
        }

        public void SetValue(bool newValue)
        {
            if (value == newValue) return;

            value = newValue;
            PersistentWorldState.Instance?.SetBool(PersistentId, value);
            onValueApplied?.Invoke(value);
        }
    }
}
