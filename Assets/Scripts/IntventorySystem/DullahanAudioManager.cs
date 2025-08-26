using UnityEngine;
using System.Collections;

public class DullahanAudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource dullahanAudioSource; // For Dullahan-specific sounds
    public AudioSource playerAudioSource;   // For player-related sounds
    public AudioSource ambientAudioSource;  // For ambient sounds
    public AudioSource effectAudioSource;   // For effect sounds
    
    [Header("Chase Audio")]
    public AudioClip[] chaseStartClips;
    public AudioClip[] chaseIntensityClips;
    public AudioClip[] chaseEndClips;
    public AudioClip[] heartbeatClips;
    
    [Header("Head Audio")]
    public AudioClip[] headPickupClips;
    public AudioClip[] headDropClips;
    public AudioClip[] headEffectClips;
    public AudioClip wrongHeadSound;
    public AudioClip puzzleCompleteSound;
    
    [Header("Flashlight Audio")]
    public AudioClip flashlightOnSound;
    public AudioClip flashlightOffSound;
    public AudioClip batteryLowSound;
    public AudioClip batteryDeadSound;
    
    [Header("Timer Audio")]
    public AudioClip timerWarningSound;
    public AudioClip doorOpenSound;
    
    [Header("Audio Settings")]
    public float maxVolume = 1f;
    public float minVolume = 0.1f;
    public float crossfadeTime = 1f;
    public bool useSpatialAudio = true;
    
    [Header("Chase Intensity Audio")]
    public float maxChaseVolume = 1f;
    public float minChaseVolume = 0.3f;
    public float maxChasePitch = 1.2f;
    public float minChasePitch = 0.8f;
    
    private AudioSource currentChaseSource;
    private AudioSource currentHeartbeatSource;
    private bool isInitialized = false;
    
    void Start()
    {
        // Setup audio sources
        SetupAudioSources();
        
        // Find missing references
        FindMissingReferences();
        
        isInitialized = true;
    }
    
    private void SetupAudioSources()
    {
        // Setup Dullahan audio source
        if (dullahanAudioSource == null)
        {
            GameObject dullahanAudioObj = new GameObject("DullahanAudio");
            dullahanAudioObj.transform.SetParent(transform);
            dullahanAudioSource = dullahanAudioObj.AddComponent<AudioSource>();
        }
        
        // Setup player audio source
        if (playerAudioSource == null)
        {
            GameObject playerAudioObj = new GameObject("PlayerAudio");
            playerAudioObj.transform.SetParent(transform);
            playerAudioSource = playerAudioObj.AddComponent<AudioSource>();
        }
        
        // Setup ambient audio source
        if (ambientAudioSource == null)
        {
            GameObject ambientAudioObj = new GameObject("AmbientAudio");
            ambientAudioObj.transform.SetParent(transform);
            ambientAudioSource = ambientAudioObj.AddComponent<AudioSource>();
        }
        
        // Setup effect audio source
        if (effectAudioSource == null)
        {
            GameObject effectAudioObj = new GameObject("EffectAudio");
            effectAudioObj.transform.SetParent(transform);
            effectAudioSource = effectAudioObj.AddComponent<AudioSource>();
        }
        
        // Configure audio sources
        ConfigureAudioSource(dullahanAudioSource, true);
        ConfigureAudioSource(playerAudioSource, false);
        ConfigureAudioSource(ambientAudioSource, true);
        ConfigureAudioSource(effectAudioSource, false);
    }
    
    private void ConfigureAudioSource(AudioSource source, bool spatial)
    {
        if (source == null) return;
        
        source.spatialBlend = spatial ? 1f : 0f; // 1f = 3D, 0f = 2D
        source.volume = maxVolume;
        source.loop = false;
        source.playOnAwake = false;
    }
    
    private void FindMissingReferences()
    {
        // Find existing audio sources in scene if not assigned
        if (dullahanAudioSource == null)
            dullahanAudioSource = FindObjectOfType<AudioSource>();
        
        if (playerAudioSource == null)
        {
            AudioSource[] sources = FindObjectsOfType<AudioSource>();
            foreach (AudioSource source in sources)
            {
                if (source != dullahanAudioSource)
                {
                    playerAudioSource = source;
                    break;
                }
            }
        }
    }
    
    // Chase Audio Methods
    public void PlayChaseStart()
    {
        if (!isInitialized || chaseStartClips.Length == 0) return;
        
        AudioClip clip = GetRandomClip(chaseStartClips);
        if (clip != null)
        {
            PlayAudioClip(dullahanAudioSource, clip, maxChaseVolume);
        }
    }
    
    public void PlayChaseEnd()
    {
        if (!isInitialized || chaseEndClips.Length == 0) return;
        
        AudioClip clip = GetRandomClip(chaseEndClips);
        if (clip != null)
        {
            PlayAudioClip(dullahanAudioSource, clip, maxChaseVolume);
        }
        
        // Stop heartbeat
        StopHeartbeat();
    }
    
    public void UpdateChaseIntensity(float intensity)
    {
        if (!isInitialized) return;
        
        // Update chase audio volume and pitch
        if (dullahanAudioSource != null && dullahanAudioSource.isPlaying)
        {
            float targetVolume = Mathf.Lerp(minChaseVolume, maxChaseVolume, intensity);
            float targetPitch = Mathf.Lerp(minChasePitch, maxChasePitch, intensity);
            
            dullahanAudioSource.volume = Mathf.Lerp(dullahanAudioSource.volume, targetVolume, Time.deltaTime * 2f);
            dullahanAudioSource.pitch = Mathf.Lerp(dullahanAudioSource.pitch, targetPitch, Time.deltaTime * 2f);
        }
        
        // Update heartbeat
        UpdateHeartbeat(intensity);
    }
    
    // Method for DullahanChaseSystem to set chase intensity
    public void SetChaseIntensity(float intensity)
    {
        UpdateChaseIntensity(intensity);
    }
    
    // Method for DullahanChaseSystem to start chase
    public void StartChase()
    {
        PlayChaseStart();
        StartHeartbeat();
    }
    
    // Method for DullahanChaseSystem to end chase
    public void EndChase()
    {
        PlayChaseEnd();
        StopHeartbeat();
    }
    
    public void StartHeartbeat()
    {
        if (!isInitialized || heartbeatClips.Length == 0) return;
        
        if (currentHeartbeatSource == null)
        {
            GameObject heartbeatObj = new GameObject("HeartbeatAudio");
            heartbeatObj.transform.SetParent(transform);
            currentHeartbeatSource = heartbeatObj.AddComponent<AudioSource>();
            ConfigureAudioSource(currentHeartbeatSource, false);
        }
        
        AudioClip clip = GetRandomClip(heartbeatClips);
        if (clip != null)
        {
            currentHeartbeatSource.clip = clip;
            currentHeartbeatSource.loop = true;
            currentHeartbeatSource.volume = 0f;
            currentHeartbeatSource.Play();
        }
    }
    
    public void StopHeartbeat()
    {
        if (currentHeartbeatSource != null)
        {
            StartCoroutine(FadeOutAudio(currentHeartbeatSource, 1f));
        }
    }
    
    private void UpdateHeartbeat(float intensity)
    {
        if (currentHeartbeatSource != null && currentHeartbeatSource.isPlaying)
        {
            float targetVolume = intensity * 0.8f;
            float targetPitch = Mathf.Lerp(0.8f, 1.2f, intensity);
            
            currentHeartbeatSource.volume = Mathf.Lerp(currentHeartbeatSource.volume, targetVolume, Time.deltaTime * 2f);
            currentHeartbeatSource.pitch = Mathf.Lerp(currentHeartbeatSource.pitch, targetPitch, Time.deltaTime * 2f);
        }
    }
    
    // Head Audio Methods
    public void PlayHeadPickupSound(HeadType headType)
    {
        if (!isInitialized || headPickupClips.Length == 0) return;
        
        AudioClip clip = GetRandomClip(headPickupClips);
        if (clip != null)
        {
            PlayAudioClip(effectAudioSource, clip, maxVolume);
        }
    }
    
    public void PlayHeadDropSound(HeadType headType)
    {
        if (!isInitialized || headDropClips.Length == 0) return;
        
        AudioClip clip = GetRandomClip(headDropClips);
        if (clip != null)
        {
            PlayAudioClip(effectAudioSource, clip, maxVolume);
        }
    }
    
    public void PlayHeadEffectSound(HeadType headType)
    {
        if (!isInitialized || headEffectClips.Length == 0) return;
        
        AudioClip clip = GetRandomClip(headEffectClips);
        if (clip != null)
        {
            PlayAudioClip(effectAudioSource, clip, maxVolume);
        }
    }
    
    public void PlayWrongHeadSound()
    {
        if (!isInitialized || wrongHeadSound == null) return;
        
        PlayAudioClip(effectAudioSource, wrongHeadSound, maxVolume);
    }
    
    public void PlayPuzzleCompleteSound()
    {
        if (!isInitialized || puzzleCompleteSound == null) return;
        
        PlayAudioClip(effectAudioSource, puzzleCompleteSound, maxVolume);
    }
    
    // Utility Methods
    private AudioClip GetRandomClip(AudioClip[] clips)
    {
        if (clips == null || clips.Length == 0) return null;
        
        return clips[Random.Range(0, clips.Length)];
    }
    
    private void PlayAudioClip(AudioSource source, AudioClip clip, float volume)
    {
        if (source == null || clip == null) return;
        
        source.clip = clip;
        source.volume = volume;
        source.Play();
    }
    
    private IEnumerator FadeOutAudio(AudioSource source, float duration)
    {
        if (source == null) yield break;
        
        float startVolume = source.volume;
        float elapsed = 0f;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            source.volume = Mathf.Lerp(startVolume, 0f, elapsed / duration);
            yield return null;
        }
        
        source.Stop();
        source.volume = startVolume;
    }
    
    private IEnumerator CrossfadeAudio(AudioSource fromSource, AudioSource toSource, AudioClip newClip, float duration)
    {
        if (fromSource == null || toSource == null || newClip == null) yield break;
        
        // Start new audio
        toSource.clip = newClip;
        toSource.volume = 0f;
        toSource.Play();
        
        float elapsed = 0f;
        float fromStartVolume = fromSource.volume;
        
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            
            fromSource.volume = Mathf.Lerp(fromStartVolume, 0f, t);
            toSource.volume = Mathf.Lerp(0f, maxVolume, t);
            
            yield return null;
        }
        
        fromSource.Stop();
        fromSource.volume = fromStartVolume;
    }
    
    // Public methods for other scripts
    public void SetMasterVolume(float volume)
    {
        maxVolume = Mathf.Clamp01(volume);
        
        if (dullahanAudioSource != null) dullahanAudioSource.volume = maxVolume;
        if (playerAudioSource != null) playerAudioSource.volume = maxVolume;
        if (ambientAudioSource != null) ambientAudioSource.volume = maxVolume;
        if (effectAudioSource != null) effectAudioSource.volume = maxVolume;
    }
    
    public void StopAllAudio()
    {
        if (dullahanAudioSource != null) dullahanAudioSource.Stop();
        if (playerAudioSource != null) playerAudioSource.Stop();
        if (ambientAudioSource != null) ambientAudioSource.Stop();
        if (effectAudioSource != null) effectAudioSource.Stop();
        if (currentHeartbeatSource != null) currentHeartbeatSource.Stop();
    }
    
    public bool IsPlayingChaseAudio()
    {
        return dullahanAudioSource != null && dullahanAudioSource.isPlaying;
    }
    
    public bool IsPlayingHeartbeat()
    {
        return currentHeartbeatSource != null && currentHeartbeatSource.isPlaying;
    }
    
    // Flashlight Audio Methods
    public void PlayFlashlightOnSound()
    {
        if (!isInitialized || flashlightOnSound == null) return;
        
        PlayAudioClip(effectAudioSource, flashlightOnSound, maxVolume);
    }
    
    public void PlayFlashlightOffSound()
    {
        if (!isInitialized || flashlightOffSound == null) return;
        
        PlayAudioClip(effectAudioSource, flashlightOffSound, maxVolume);
    }
    
    public void PlayBatteryLowSound()
    {
        if (!isInitialized || batteryLowSound == null) return;
        
        PlayAudioClip(effectAudioSource, batteryLowSound, maxVolume);
    }
    
    public void PlayBatteryDeadSound()
    {
        if (!isInitialized || batteryDeadSound == null) return;
        
        PlayAudioClip(effectAudioSource, batteryDeadSound, maxVolume);
    }
    
    // Timer Audio Methods
    public void PlayTimerWarningSound()
    {
        if (!isInitialized || timerWarningSound == null) return;
        
        PlayAudioClip(effectAudioSource, timerWarningSound, maxVolume);
    }
    
    public void PlayDoorOpenSound()
    {
        if (!isInitialized || doorOpenSound == null) return;
        
        PlayAudioClip(effectAudioSource, doorOpenSound, maxVolume);
    }
}
