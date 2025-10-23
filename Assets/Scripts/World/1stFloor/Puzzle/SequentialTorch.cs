using UnityEngine;
using System.Collections;

/// <summary>
/// Individual torch/candle for sequential lighting puzzle
/// Each torch can only be lit when the previous one in sequence is already lit
/// </summary>
public class SequentialTorch : MonoBehaviour
{
    [Header("Torch Settings")]
    [Tooltip("Sequence number (1, 2, 3, etc.) - determines lighting order")]
    public int sequenceNumber = 1;
    
    [Tooltip("How close player needs to be to interact")]
    public float interactionDistance = 3f;
    
    [Header("Visual Components")]
    [Tooltip("The flame GameObject (should be disabled initially)")]
    public GameObject flameObject;
    
    [Tooltip("The light component for illumination")]
    public Light torchLight;
    
    [Tooltip("Particle system for fire effects")]
    public ParticleSystem fireParticles;
    
    [Tooltip("Particle system for ready-to-light effect")]
    public ParticleSystem readyParticles;
    
    [Header("Audio")]
    public AudioClip lightSound;
    public AudioClip wrongSequenceSound;
    public AudioClip readySound;
    
    [Header("UI")]
    [Tooltip("UI text for interaction prompt")]
    public TMPro.TextMeshProUGUI interactionText;
    
    // State
    private bool isLit = false;
    private bool isReadyToLight = false;
    private Transform player;
    private AudioSource audioSource;
    private SequentialTorchManager puzzleManager;
    
    // Visual feedback
    private Renderer torchRenderer;
    private Color originalColor;
    private Color readyColor = Color.yellow;
    private Color litColor = Color.orange;
    
    void Start()
    {
        // Find player
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj) player = playerObj.transform;
        
        // Find puzzle manager
        puzzleManager = FindObjectOfType<SequentialTorchManager>();
        
        // Audio
        audioSource = GetComponent<AudioSource>();
        if (!audioSource) audioSource = gameObject.AddComponent<AudioSource>();
        
        // Visual setup
        torchRenderer = GetComponent<Renderer>();
        if (torchRenderer) originalColor = torchRenderer.material.color;
        
        // Initially unlit
        SetTorchState(false, false);
        
        // Hide interaction text
        if (interactionText) interactionText.gameObject.SetActive(false);
        
        Debug.Log($"[SequentialTorch] Torch {sequenceNumber} initialized");
    }
    
    void Update()
    {
        if (!player || isLit) return;
        
        // Check distance to player
        float distance = Vector3.Distance(transform.position, player.position);
        bool inRange = distance <= interactionDistance;
        
        // Show/hide interaction prompt
        if (interactionText)
        {
            bool showPrompt = inRange && isReadyToLight;
            interactionText.gameObject.SetActive(showPrompt);
            
            if (showPrompt)
            {
                interactionText.text = "Press F to light torch";
                interactionText.color = Color.white;
            }
        }
        
        // Handle F key press
        if (inRange && Input.GetKeyDown(KeyCode.F))
        {
            TryLightTorch();
        }
    }
    
    void TryLightTorch()
    {
        if (isLit) return;
        
        // Check if this torch is ready to be lit
        if (puzzleManager && !puzzleManager.CanLightTorch(sequenceNumber))
        {
            // Wrong sequence - show feedback
            ShowWrongSequenceFeedback();
            return;
        }
        
        // Light the torch
        LightTorch();
    }
    
    public void LightTorch()
    {
        Debug.Log($"[SequentialTorch] ✓ Torch {sequenceNumber} LIT!");

        isLit = true;
        SetTorchState(true, false);

        // Play sound
        if (lightSound) audioSource.PlayOneShot(lightSound);

        // Notify puzzle manager
        if (puzzleManager) puzzleManager.OnTorchLit(sequenceNumber);

        // Hide interaction text
        if (interactionText) interactionText.gameObject.SetActive(false);
    }

    public void ExtinguishTorch()
    {
        Debug.Log($"[SequentialTorch] Torch {sequenceNumber} extinguished");

        isLit = false;
        isReadyToLight = false;
        SetTorchState(false, false);

        // Hide interaction text
        if (interactionText) interactionText.gameObject.SetActive(false);
    }

    void ShowWrongSequenceFeedback()
    {
        Debug.Log($"[SequentialTorch] ✗ Torch {sequenceNumber} - Wrong sequence!");
        
        // Play wrong sound
        if (wrongSequenceSound) audioSource.PlayOneShot(wrongSequenceSound);
        
        // Notify puzzle manager
        if (puzzleManager) puzzleManager.OnWrongSequenceAttempted(sequenceNumber);
        
        // Visual feedback - red flash
        StartCoroutine(FlashRed());
    }
    
    IEnumerator FlashRed()
    {
        if (torchRenderer)
        {
            Color original = torchRenderer.material.color;
            torchRenderer.material.color = Color.red;
            yield return new WaitForSeconds(0.2f);
            torchRenderer.material.color = original;
        }
    }
    
    public void SetReadyToLight(bool ready)
    {
        if (isLit) return;
        
        isReadyToLight = ready;
        SetTorchState(false, ready);
        
        // Play ready sound
        if (ready && readySound) audioSource.PlayOneShot(readySound);
        
        Debug.Log($"[SequentialTorch] Torch {sequenceNumber} ready to light: {ready}");
    }
    
    void SetTorchState(bool lit, bool ready)
    {
        // Flame object
        if (flameObject) flameObject.SetActive(lit);
        
        // Light component
        if (torchLight) torchLight.enabled = lit;
        
        // Fire particles
        if (fireParticles)
        {
            if (lit && !fireParticles.isPlaying)
                fireParticles.Play();
            else if (!lit && fireParticles.isPlaying)
                fireParticles.Stop();
        }
        
        // Ready particles
        if (readyParticles)
        {
            if (ready && !lit && !readyParticles.isPlaying)
                readyParticles.Play();
            else if ((!ready || lit) && readyParticles.isPlaying)
                readyParticles.Stop();
        }
        
        // Torch color
        if (torchRenderer)
        {
            if (lit)
                torchRenderer.material.color = litColor;
            else if (ready)
                torchRenderer.material.color = readyColor;
            else
                torchRenderer.material.color = originalColor;
        }
    }
    
    public bool IsLit => isLit;
    public int SequenceNumber => sequenceNumber;
    
    // Debug visualization
    void OnDrawGizmosSelected()
    {
        Gizmos.color = isReadyToLight ? Color.green : (isLit ? Color.orange : Color.gray);
        Gizmos.DrawWireSphere(transform.position, interactionDistance);

        #if UNITY_EDITOR
        // Draw sequence number
        UnityEditor.Handles.Label(transform.position + Vector3.up * 2f, $"Torch {sequenceNumber}");
        #endif
    }
}
