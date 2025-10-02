using System.Collections;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;
using World.UI;

namespace SaveLoad
{
    public class SaveManager : MonoBehaviour
    {
        private static SaveData pendingLoadData;
        private static bool hasPendingLoadData;
        private Coroutine pendingSaveCoroutine;
        private string lastSerializedSave;

        [Header("Checkpoint Saving")]
        [SerializeField] private bool saveAsynchronously = true;
        [SerializeField] private float checkpointCooldown = 0.25f;
        private float lastSaveTime;

        [Header("References")]
        [Tooltip("Reference to the player GameObject that will be saved.")]
        [SerializeField] private GameObject player;

        [Tooltip("Optional reference to the player's health system.")]
        [SerializeField] private PlayerHealthSystem playerHealthSystem;

        private CharacterController characterController;
        private Rigidbody playerRigidbody;
        private FirstPersonController firstPersonController;
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

            if (pendingSaveCoroutine != null)
            {
                StopCoroutine(pendingSaveCoroutine);
                pendingSaveCoroutine = null;
            }
        }

        public void SaveGame()
        {
            if (player == null)
            {
                Debug.LogError("SaveManager.SaveGame called but no player reference is set.");
                return;
            }

            if (saveAsynchronously)
            {
                StartAsyncSave();
            }
            else
            {
                if (PersistentWorldState.Instance != null)
                {
                    PersistentWorldState.Instance.CreateSnapshot();
                }

                SaveSystem.SavePlayer(player, playerHealthSystem);
                onSaveSuccess?.Invoke();
            }
        }

        public void SaveCheckpoint()
        {
            if (!gameObject.activeInHierarchy)
                return;

            if (Time.unscaledTime - lastSaveTime < checkpointCooldown)
                return;

            lastSaveTime = Time.unscaledTime;
            SaveGame();
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
                SceneTransitionManager.Instance.LoadScene(sceneName, "Loading Save");
            }
            else
            {
                StartCoroutine(LoadSceneAsyncFallback(sceneName, "Loading Save"));
            }
        }

        private IEnumerator LoadSceneAsyncFallback(string sceneName, string loadingMessage)
        {
            var operation = SceneManager.LoadSceneAsync(sceneName);

            if (LoadingScreenController.Instance != null)
            {
                yield return LoadingScreenController.Instance.TrackAsyncOperation(operation, loadingMessage);
            }
            else
            {
                yield return operation;
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

            var controllerEnabledState = DisableMovementComponents();

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

            RestoreMovementComponents(controllerEnabledState);
        }

        private (bool characterControllerEnabled, bool rigidbodyKinematic, bool firstPersonControllerEnabled) DisableMovementComponents()
        {
            bool ccEnabled = characterController != null && characterController.enabled;
            bool fpcEnabled = firstPersonController != null && firstPersonController.enabled;
            bool rbKinematic = playerRigidbody != null && playerRigidbody.isKinematic;

            if (characterController != null && characterController.enabled)
            {
                characterController.enabled = false;
            }

            if (firstPersonController != null && firstPersonController.enabled)
            {
                firstPersonController.enabled = false;
            }

            if (playerRigidbody != null)
            {
                playerRigidbody.isKinematic = true;
                playerRigidbody.linearVelocity = Vector3.zero;
                playerRigidbody.angularVelocity = Vector3.zero;
            }

            return (ccEnabled, rbKinematic, fpcEnabled);
        }

        private void RestoreMovementComponents((bool characterControllerEnabled, bool rigidbodyKinematic, bool firstPersonControllerEnabled) previousState)
        {
            if (characterController != null)
            {
                characterController.enabled = previousState.characterControllerEnabled;
            }

            if (firstPersonController != null)
            {
                firstPersonController.enabled = previousState.firstPersonControllerEnabled;
            }

            if (playerRigidbody != null)
            {
                playerRigidbody.isKinematic = previousState.rigidbodyKinematic;
                playerRigidbody.linearVelocity = Vector3.zero;
                playerRigidbody.angularVelocity = Vector3.zero;
            }
        }

        private void CachePlayerComponents()
        {
            if (player != null)
            {
                characterController = player.GetComponent<CharacterController>();
                firstPersonController = player.GetComponent<FirstPersonController>();
                playerRigidbody = player.GetComponent<Rigidbody>();
                playerCamera = firstPersonController != null ? firstPersonController.PlayerCamera : FirstPersonController.GetActivePlayerCamera();

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
            if (LoadingScreenController.Instance != null)
            {
                LoadingScreenController.Instance.SetStatus("Applying Save...");
            }

            yield return null;

            ApplyLoadedData(pendingLoadData);
            onLoadSuccess?.Invoke();

            if (LoadingScreenController.Instance != null)
            {
                LoadingScreenController.Instance.Hide();
            }

            pendingLoadData = null;
            hasPendingLoadData = false;
        }

        private void StartAsyncSave()
        {
            if (pendingSaveCoroutine != null)
            {
                StopCoroutine(pendingSaveCoroutine);
            }

            var data = SaveSystem.BuildSaveData(player, playerHealthSystem);
            lastSerializedSave = SaveSystem.SerializeSaveData(data, true);

            pendingSaveCoroutine = StartCoroutine(SaveSystem.WriteSaveAsync(lastSerializedSave,
                () =>
                {
                    onSaveSuccess?.Invoke();
                    pendingSaveCoroutine = null;
                },
                ex =>
                {
                    Debug.LogError($"Async save failed: {ex.Message}");
                    pendingSaveCoroutine = null;
                }));
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
