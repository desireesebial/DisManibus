using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

namespace SaveLoad
{
    public class SaveManager : MonoBehaviour
    {
        private static SaveData pendingLoadData;
        private static bool hasPendingLoadData;

        [Header("References")]
        [Tooltip("Reference to the player GameObject that will be saved.")]
        [SerializeField] private GameObject player;

        [Tooltip("Optional reference to the player's health system.")]
        [SerializeField] private PlayerHealthSystem playerHealthSystem;

        private CharacterController characterController;
        private Camera playerCamera;

        [Header("Events")]
        [SerializeField] private UnityEvent onSaveSuccess;
        [SerializeField] private UnityEvent onLoadSuccess;
        [SerializeField] private UnityEvent onLoadFailure;

        public static void SetPendingLoadData(SaveData data)
        {
            pendingLoadData = data;
            hasPendingLoadData = data != null;
        }

        private void Awake()
        {
            if (player == null)
            {
                player = GameObject.FindGameObjectWithTag("Player");
            }

            CachePlayerComponents();
        }

        private void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        public void SaveGame()
        {
            if (player == null)
            {
                Debug.LogError("SaveManager.SaveGame called but no player reference is set.");
                return;
            }

            SaveSystem.SavePlayer(player, playerHealthSystem);
            onSaveSuccess?.Invoke();
        }

        public void LoadGame()
        {
            if (!SaveSystem.TryLoadPlayer(out SaveData data))
            {
                onLoadFailure?.Invoke();
                return;
            }

            if (SceneManager.GetActiveScene().name != data.sceneName)
            {
                SetPendingLoadData(data);
                LoadSceneByName(data.sceneName);
            }
            else
            {
                StartCoroutine(ApplyLoadedDataNextFrame(data));
            }
        }

        private void LoadSceneByName(string sceneName)
        {
            if (SceneTransitionManager.Instance != null)
            {
                SceneTransitionManager.Instance.LoadScene(sceneName);
            }
            else
            {
                SceneManager.LoadScene(sceneName);
            }
        }

        private IEnumerator ApplyLoadedDataNextFrame(SaveData data)
        {
            yield return null; // wait a frame to allow other components to initialize
            ApplyLoadedData(data);
            onLoadSuccess?.Invoke();
        }

        private void ApplyLoadedData(SaveData data)
        {
            if (data == null)
            {
                Debug.LogError("SaveManager received null save data.");
                return;
            }

            if (player == null)
            {
                player = GameObject.FindGameObjectWithTag("Player");
                if (player == null)
                {
                    Debug.LogError("SaveManager could not find player after loading scene.");
                    return;
                }
                CachePlayerComponents();
            }

            player.transform.position = data.playerPosition.ToVector3();
            player.transform.eulerAngles = data.playerRotation.ToVector3();

            if (PersistentWorldState.Instance != null)
            {
                PersistentWorldState.Instance.RestoreFromSnapshot(data.worldState);
            }

            if (data.hasCameraRotation && playerCamera != null)
            {
                playerCamera.transform.localEulerAngles = data.cameraLocalRotation.ToVector3();
            }

            if (playerHealthSystem != null && data.playerHealth >= 0)
            {
                playerHealthSystem.ApplySavedHealth(data.playerHealth, data.maxHealth);
            }
        }

        private void CachePlayerComponents()
        {
            if (player != null)
            {
                characterController = player.GetComponent<CharacterController>();
                var fpc = player.GetComponent<FirstPersonController>();
                playerCamera = fpc != null ? fpc.PlayerCamera : FirstPersonController.GetActivePlayerCamera();

                if (playerHealthSystem == null)
                {
                    playerHealthSystem = player.GetComponent<PlayerHealthSystem>();
                }
            }
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            player = GameObject.FindGameObjectWithTag("Player");
            CachePlayerComponents();

            if (hasPendingLoadData && pendingLoadData != null && scene.name == pendingLoadData.sceneName)
            {
                StartCoroutine(ApplyPendingLoadRoutine());
            }
        }

        private IEnumerator ApplyPendingLoadRoutine()
        {
            yield return null;
            ApplyLoadedData(pendingLoadData);
            onLoadSuccess?.Invoke();
            pendingLoadData = null;
            hasPendingLoadData = false;
        }

        public bool HasSaveData()
        {
            return SaveSystem.HasSaveData();
        }

        public void AutoSaveIfAvailable()
        {
            if (!gameObject.activeInHierarchy)
            {
                return;
            }

            if (HasSaveData())
            {
                SaveGame();
            }
        }
    }
}
