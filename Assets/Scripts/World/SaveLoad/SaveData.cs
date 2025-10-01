using System;
using UnityEngine;

namespace SaveLoad
{
    [Serializable]
    public class SaveData
    {
        public string sceneName;
        public Vector3Serializable playerPosition;
        public Vector3Serializable playerRotation;
        public int playerHealth;
        public int maxHealth;
        public bool hasCameraRotation;
        public Vector3Serializable cameraLocalRotation;
        public bool isCrouched;
        public bool isSprinting;
        public WorldState worldState;
    }

    [Serializable]
    public class WorldState
    {
        public SerializableDoorState[] doors = Array.Empty<SerializableDoorState>();
        public SerializableBoolState[] boolStates = Array.Empty<SerializableBoolState>();
    }

    [Serializable]
    public struct SerializableDoorState
    {
        public string id;
        public bool isOpen;
    }

    [Serializable]
    public struct SerializableBoolState
    {
        public string id;
        public bool value;
    }

    [Serializable]
    public struct Vector3Serializable
    {
        public float x;
        public float y;
        public float z;

        public Vector3Serializable(Vector3 vector)
        {
            x = vector.x;
            y = vector.y;
            z = vector.z;
        }

        public Vector3 ToVector3()
        {
            return new Vector3(x, y, z);
        }
    }
}
