using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class KuchisakeQuestionUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private GameObject questionPanel;
    [SerializeField] private TextMeshProUGUI questionText;
    [SerializeField] private TextMeshProUGUI timerText;
    [SerializeField] private Button yesButton;
    [SerializeField] private Button noButton;
    [SerializeField] private Button maybeButton;
    [SerializeField] private Image timerBarFill;

    [Header("Player Health System")]
    [SerializeField] private PlayerHealthSystem playerHealthSystem;

    [Header("Audio")]
    [SerializeField] private AudioSource uiAudioSource;
    [SerializeField] private AudioClip heartbeatSound;
    [SerializeField] private AudioClip scissorCloseSound;
    [SerializeField] private AudioClip buttonClickSound;

    [Header("Settings")]
    [SerializeField] private string questionString = "Am I beautiful?";
    [SerializeField] private Color timerNormalColor = Color.white;
    [SerializeField] private Color timerWarningColor = Color.yellow;
    [SerializeField] private Color timerDangerColor = Color.red;
    [SerializeField] private float heartbeatStartTime = 3f;
    
    private float currentTimer;
    private float maxTimer;
    private bool isQuestionActive = false;
    private KuchisakeOnnaController enemyController;
    private Coroutine heartbeatCoroutine;

    public enum Answer
    {
        Yes,
        No,
        Maybe
    }

    void Start()
    {
        // Setup button listeners
        if (yesButton != null)
            yesButton.onClick.AddListener(() => OnAnswerSelected(Answer.Yes));

        if (noButton != null)
            noButton.onClick.AddListener(() => OnAnswerSelected(Answer.No));

        if (maybeButton != null)
            maybeButton.onClick.AddListener(() => OnAnswerSelected(Answer.Maybe));

        // Find PlayerHealthSystem if not assigned
        if (playerHealthSystem == null)
        {
            playerHealthSystem = FindObjectOfType<PlayerHealthSystem>();
            if (playerHealthSystem == null)
            {
                Debug.LogWarning("[KuchisakeQuestionUI] PlayerHealthSystem not found! Death screen will not work.");
            }
        }

        // Hide UI initially
        if (questionPanel != null)
            questionPanel.SetActive(false);
    }

    void Update()
    {
        if (isQuestionActive)
        {
            currentTimer -= Time.deltaTime;

            // Update timer display
            UpdateTimerDisplay();

            // Check if time ran out
            if (currentTimer <= 0)
            {
                OnTimerExpired();
            }
        }
    }

    public void ShowQuestion(float timerDuration, KuchisakeOnnaController controller)
    {
        enemyController = controller;
        maxTimer = timerDuration;
        currentTimer = timerDuration;
        isQuestionActive = true;

        // Show UI
        if (questionPanel != null)
        {
            questionPanel.SetActive(true);
        }

        if (questionText != null)
        {
            questionText.text = questionString;
        }

        // Freeze player
        FreezePlayer(true);

        // Start heartbeat sound
        if (currentTimer <= heartbeatStartTime)
        {
            StartHeartbeat();
        }
    }

    void UpdateTimerDisplay()
    {
        // Update text
        if (timerText != null)
        {
            timerText.text = Mathf.CeilToInt(currentTimer).ToString();

            // Change color based on time remaining
            if (currentTimer <= 1f)
                timerText.color = timerDangerColor;
            else if (currentTimer <= 2f)
                timerText.color = timerWarningColor;
            else
                timerText.color = timerNormalColor;
        }

        // Update progress bar
        if (timerBarFill != null)
        {
            timerBarFill.fillAmount = currentTimer / maxTimer;

            // Change bar color
            if (currentTimer <= 1f)
                timerBarFill.color = timerDangerColor;
            else if (currentTimer <= 2f)
                timerBarFill.color = timerWarningColor;
            else
                timerBarFill.color = timerNormalColor;
        }

        // Start heartbeat when time is running low
        if (currentTimer <= heartbeatStartTime && heartbeatCoroutine == null)
        {
            StartHeartbeat();
        }
    }

    void OnAnswerSelected(Answer answer)
    {
        if (!isQuestionActive) return;

        // Play click sound
        if (buttonClickSound != null && uiAudioSource != null)
        {
            uiAudioSource.PlayOneShot(buttonClickSound);
        }

        // Stop question sequence
        isQuestionActive = false;
        StopHeartbeat();

        // Hide UI
        HideQuestion();

        // Unfreeze player
        FreezePlayer(false);

        // Notify enemy controller
        if (enemyController != null)
        {
            enemyController.OnPlayerAnswered(answer);
        }
    }

    void OnTimerExpired()
    {
        isQuestionActive = false;
        StopHeartbeat();

        // Play death sound
        if (scissorCloseSound != null && uiAudioSource != null)
        {
            uiAudioSource.PlayOneShot(scissorCloseSound);
        }

        // Hide question UI
        HideQuestion();

        // Unfreeze player (so PlayerHealthSystem can manage controls)
        FreezePlayer(false);

        // Show death screen with custom Kuchisake-specific message
        if (playerHealthSystem != null)
        {
            playerHealthSystem.ShowDeathScreen("You hesitated...\n\nShe didn't.");
        }
        else
        {
            Debug.LogWarning("[KuchisakeQuestionUI] PlayerHealthSystem not found! Cannot show death screen.");
        }

        // Notify enemy controller
        if (enemyController != null)
        {
            enemyController.OnQuestionTimeout();
        }
    }


    void HideQuestion()
    {
        if (questionPanel != null)
        {
            questionPanel.SetActive(false);
        }
    }

    void FreezePlayer(bool freeze)
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            // Disable player movement
            var playerController = player.GetComponent<MonoBehaviour>();
            if (playerController != null)
            {
                playerController.enabled = !freeze;
            }

            // Alternative: Freeze rigidbody if using physics-based movement
            var rb = player.GetComponent<Rigidbody>();
            if (rb != null)
            {
                rb.isKinematic = freeze;
            }

            // Lock cursor during question
            if (freeze)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
            }
        }
    }

    void StartHeartbeat()
    {
        if (heartbeatSound != null && uiAudioSource != null && heartbeatCoroutine == null)
        {
            heartbeatCoroutine = StartCoroutine(PlayHeartbeat());
        }
    }

    void StopHeartbeat()
    {
        if (heartbeatCoroutine != null)
        {
            StopCoroutine(heartbeatCoroutine);
            heartbeatCoroutine = null;
        }
    }

    IEnumerator PlayHeartbeat()
    {
        while (isQuestionActive)
        {
            if (uiAudioSource != null && heartbeatSound != null)
            {
                uiAudioSource.PlayOneShot(heartbeatSound);
            }

            // Speed up heartbeat as time runs out
            float interval = Mathf.Lerp(0.3f, 0.8f, currentTimer / maxTimer);
            yield return new WaitForSeconds(interval);
        }
    }

    public void ResetUI()
    {
        isQuestionActive = false;
        StopHeartbeat();
        HideQuestion();

        // Unfreeze player
        FreezePlayer(false);

        // Note: Death screen is managed by PlayerHealthSystem
        // It will handle its own reset when player respawns
    }
}

