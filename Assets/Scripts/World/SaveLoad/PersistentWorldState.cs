using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace SaveLoad
{
    public class PersistentWorldState : MonoBehaviour
    {
        private static PersistentWorldState instance;

        public static PersistentWorldState Instance
        {
            get
            {
                if (instance == null)
                {
                    instance = FindFirstObjectByType<PersistentWorldState>();
                    if (instance == null)
                    {
                        GameObject go = new GameObject("PersistentWorldState");
                        instance = go.AddComponent<PersistentWorldState>();
                        DontDestroyOnLoad(go);
                    }
                }

                return instance;
            }
        }

        private readonly Dictionary<string, bool> boolStates = new Dictionary<string, bool>();
        private readonly Dictionary<string, bool> doorStates = new Dictionary<string, bool>();

        private readonly List<PersistentEntity> registeredEntities = new List<PersistentEntity>();

        private readonly Dictionary<string, PersistentEntity> idToEntity = new Dictionary<string, PersistentEntity>();

        private void Awake()
        {
            if (instance != null && instance != this)
            {
                Destroy(gameObject);
                return;
            }

            instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public void RegisterEntity(PersistentEntity entity)
        {
            if (entity == null || registeredEntities.Contains(entity))
                return;

            registeredEntities.Add(entity);

            if (!string.IsNullOrEmpty(entity.PersistentId))
            {
                idToEntity[entity.PersistentId] = entity;
            }
        }

        public void UnregisterEntity(PersistentEntity entity)
        {
            if (entity == null)
                return;

            registeredEntities.Remove(entity);

            if (!string.IsNullOrEmpty(entity.PersistentId) && idToEntity.TryGetValue(entity.PersistentId, out var existing) && existing == entity)
            {
                idToEntity.Remove(entity.PersistentId);
            }
        }

        public bool HasDuplicateId(string id, PersistentEntity entity)
        {
            if (string.IsNullOrEmpty(id))
                return false;

            return idToEntity.TryGetValue(id, out var existing) && existing != entity;
        }

        public void SetBool(string id, bool value)
        {
            if (string.IsNullOrEmpty(id))
                return;

            boolStates[id] = value;
        }

        public bool TryGetBool(string id, out bool value)
        {
            if (string.IsNullOrEmpty(id))
            {
                value = default;
                return false;
            }

            return boolStates.TryGetValue(id, out value);
        }

        public void SetDoorState(string id, bool isOpen)
        {
            if (string.IsNullOrEmpty(id))
                return;

            doorStates[id] = isOpen;
        }

        public bool TryGetDoorState(string id, out bool isOpen)
        {
            if (string.IsNullOrEmpty(id))
            {
                isOpen = false;
                return false;
            }

            return doorStates.TryGetValue(id, out isOpen);
        }

        public WorldState CreateSnapshot()
        {
            foreach (var entity in registeredEntities)
            {
                entity.CaptureState(this);
            }

            return new WorldState
            {
                boolStates = boolStates.Select(kvp => new SerializableBoolState { id = kvp.Key, value = kvp.Value }).ToArray(),
                doors = doorStates.Select(kvp => new SerializableDoorState { id = kvp.Key, isOpen = kvp.Value }).ToArray()
            };
        }

        public void RestoreFromSnapshot(WorldState state)
        {
            boolStates.Clear();
            doorStates.Clear();

            if (state?.boolStates != null)
            {
                foreach (var entry in state.boolStates)
                {
                    if (!string.IsNullOrEmpty(entry.id))
                    {
                        boolStates[entry.id] = entry.value;
                    }
                }
            }

            if (state?.doors != null)
            {
                foreach (var entry in state.doors)
                {
                    if (!string.IsNullOrEmpty(entry.id))
                    {
                        doorStates[entry.id] = entry.isOpen;
                    }
                }
            }

            foreach (var entity in registeredEntities)
            {
                entity.ApplyState(this);
            }
        }

        public void ResetState()
        {
            boolStates.Clear();
            doorStates.Clear();
            idToEntity.Clear();
        }
    }
}
