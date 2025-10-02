using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SaveLoad
{
    public static class SaveSystem
    {
        private static readonly string FileName = "player.sav";
        private static readonly string FilePath = Path.Combine(Application.persistentDataPath, FileName);

        private static readonly byte[] EncryptionKey;
        private static readonly byte[] InitializationVector;

        // Configuration for key derivation
        private const string Passphrase = "DisManibus_SaveKey_2025";
        private static readonly byte[] Salt = Encoding.UTF8.GetBytes("DisManibusSaltValue");
        private const int Iterations = 100_000;

        static SaveSystem()
        {
            using (var keyDeriver = new Rfc2898DeriveBytes(Passphrase, Salt, Iterations, HashAlgorithmName.SHA256))
            {
                EncryptionKey = keyDeriver.GetBytes(32); // AES-256 key size
                InitializationVector = keyDeriver.GetBytes(16); // AES block size
            }
        }

        public static void SavePlayer(GameObject player, PlayerHealthSystem healthSystem)
        {
            if (player == null)
            {
                Debug.LogError("SaveSystem.SavePlayer called with null player.");
                return;
            }

            var data = BuildSaveData(player, healthSystem);
            string json = SerializeSaveData(data, true);

            try
            {
                WriteEncrypted(json);
#if UNITY_EDITOR
                Debug.Log($"Game saved to {FilePath}\n{json}");
#else
                Debug.Log("Game saved successfully (encrypted).");
#endif
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save game: {ex.Message}\n{ex.StackTrace}");
            }
        }

        public static SaveData BuildSaveData(GameObject player, PlayerHealthSystem healthSystem)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            var controller = player.GetComponent<FirstPersonController>();
            Camera playerCamera = controller != null ? controller.PlayerCamera : FirstPersonController.GetActivePlayerCamera();

            WorldState worldState = PersistentWorldState.Instance != null
                ? PersistentWorldState.Instance.CreateSnapshot()
                : new WorldState();

            return new SaveData
            {
                sceneName = SceneManager.GetActiveScene().name,
                playerPosition = new Vector3Serializable(player.transform.position),
                playerRotation = new Vector3Serializable(player.transform.eulerAngles),
                playerHealth = healthSystem != null ? healthSystem.GetCurrentHealth() : -1,
                maxHealth = healthSystem != null ? healthSystem.GetMaxHealth() : -1,
                hasCameraRotation = playerCamera != null,
                cameraLocalRotation = playerCamera != null ? new Vector3Serializable(playerCamera.transform.localEulerAngles) : default,
                isCrouched = controller != null && controller.IsCrouched,
                isSprinting = controller != null && controller.IsSprinting,
                worldState = worldState
            };
        }

        public static string SerializeSaveData(SaveData data, bool prettyPrint)
        {
            return JsonUtility.ToJson(data, prettyPrint);
        }

        public static IEnumerator WriteSaveAsync(string json, Action onSuccess = null, Action<Exception> onError = null)
        {
            if (string.IsNullOrEmpty(json))
            {
                onError?.Invoke(new ArgumentException("Cannot write empty save data."));
                yield break;
            }

            Exception capturedException = null;

            Task task = Task.Run(() =>
            {
                try
                {
                    WriteEncrypted(json);
                }
                catch (Exception ex)
                {
                    capturedException = ex;
                }
            });

            while (!task.IsCompleted)
            {
                yield return null;
            }

            if (capturedException != null)
            {
                onError?.Invoke(capturedException);
            }
            else
            {
                onSuccess?.Invoke();
            }
        }

        public static bool TryLoadPlayer(out SaveData saveData)
        {
            saveData = null;

            if (!File.Exists(FilePath))
            {
                Debug.LogWarning("No save file found.");
                return false;
            }

            try
            {
                string encrypted = File.ReadAllText(FilePath);
                string decrypted = Decrypt(encrypted);
                saveData = JsonUtility.FromJson<SaveData>(decrypted);

                if (saveData == null)
                {
                    Debug.LogError("Failed to parse save data.");
                    return false;
                }

                EnsureControllerStateCompatibility(saveData);

                if (PersistentWorldState.Instance != null)
                {
                    PersistentWorldState.Instance.RestoreFromSnapshot(saveData.worldState);
                }

                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load game: {ex.Message}\n{ex.StackTrace}");
                saveData = null;
                return false;
            }
        }

        public static bool HasSaveData()
        {
            return File.Exists(FilePath);
        }

        public static bool DeleteSave()
        {
            if (!File.Exists(FilePath))
            {
                return false;
            }

            try
            {
                File.Delete(FilePath);
                PersistentWorldState.Instance?.ResetState();
                return true;
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to delete save: {ex.Message}");
                return false;
            }
        }

        private static void WriteEncrypted(string json)
        {
            string encrypted = Encrypt(json);
            File.WriteAllText(FilePath, encrypted);
#if UNITY_EDITOR
            Debug.Log($"Game saved to {FilePath}\n{json}");
#endif
        }

        private static string Encrypt(string plainText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = EncryptionKey;
                aes.IV = InitializationVector;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
                    using (StreamWriter writer = new StreamWriter(cs))
                    {
                        writer.Write(plainText);
                    }

                    return Convert.ToBase64String(ms.ToArray());
                }
            }
        }

        private static string Decrypt(string cipherText)
        {
            using (Aes aes = Aes.Create())
            {
                aes.Key = EncryptionKey;
                aes.IV = InitializationVector;
                aes.Mode = CipherMode.CBC;
                aes.Padding = PaddingMode.PKCS7;

                ICryptoTransform decryptor = aes.CreateDecryptor(aes.Key, aes.IV);
                byte[] buffer = Convert.FromBase64String(cipherText);

                using (MemoryStream ms = new MemoryStream(buffer))
                using (CryptoStream cs = new CryptoStream(ms, decryptor, CryptoStreamMode.Read))
                using (StreamReader reader = new StreamReader(cs))
                {
                    return reader.ReadToEnd();
                }
            }
        }

        private static void EnsureControllerStateCompatibility(SaveData saveData)
        {
            if (saveData != null)
            {
                // Ensure default values for newer fields
                if (saveData.maxHealth == 0 && saveData.playerHealth > 0 && saveData.maxHealth < saveData.playerHealth)
                {
                    saveData.maxHealth = saveData.playerHealth;
                }
            }
        }
    }
}
