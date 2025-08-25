using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DullahanAudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource dullahanAudioSource;
    public AudioSource playerAudioSource;
    public AudioSource ambientAudioSource;
    public AudioSource effectAudioSource;
    
    [Header("Chase Intensity Audio")]
    [Header("Chase Start/End")]
    public AudioClip chaseStartSound;
    public AudioClip chaseEndSound;
    public AudioClip chaseLoopSound;
    
    [Header("Chase Intensity Levels")]
    public AudioClip[] chaseIntensityClips; // Different intensity levels (0-3)
    public AudioClip[] heartbeatClips; // Player heartbeat at different intensities
    public AudioClip[] dullahanFootsteps; // Dullahan footsteps at different speeds
    
    [Header("Head Puzzle Audio")]
    [Header("Head Pickup")]
    public AudioClip headPickupSound;
    public AudioClip headDropSound;
    public AudioClip wrongHeadSound;
    public AudioClip headEffectSound;
    
    [Header("Head Types")]
    public AudioClip realHeadPickupSound;
    public AudioClip fakeHeadPickupSound;
    public AudioClip headGlowSound;
    
    [Header("Body Interaction")]
    public AudioClip headAttachSound;
    public AudioClip bodyActivateSound;
    public AudioClip doorUnlockSound;
    public AudioClip puzzleCompleteSound;
    
    [Header("Effect Audio")]
    [Header("Speed Effects")]
    public AudioClip speedBoostSound;
    public AudioClip speedDebuffSound;
    
    [Header("Vision Effects")]
    public AudioClip visionBoostSound;
    public AudioClip visionDebuffSound;
    
    [Header("Fear/Calm Effects")]
    public AudioClip fearEffectSound;
    public AudioClip calmEffectSound;
    
    [Header("Ambient Audio")]
    public AudioClip dullahanAmbientSound;
    public AudioClip puzzleAmbientSound;
    public AudioClip[] ambientTensionClips; // Background tension sounds
    
    [Header("Audio Settings")]
    public float masterVolume = 1f;
    public float chaseVolume = 0.8f;
    public float effectVolume = 0.6f;
    public float ambientVolume = 0.4f;
    
    [Header("Crossfade Settings")]
    public float crossfadeTime = 2f;
    public float heartbeatFadeTime = 1f;
    
    [Header("Spatial Audio")]
    public float dullahanMaxDistance = 20f;
    public float dullahanMinDistance = 1f;
    public float playerMaxDistance = 10f;
    
    // Private variables
    private AudioSource currentChaseSource;
    private AudioSource currentHeartbeatSource;
    private AudioSource currentAmbientSource;
    
    private float currentChaseIntensity = 0f;
    private bool isChasing = false;
    private bool puzzleActive = false;
    
    // Audio state tracking
    private int currentIntensityLevel = 0;
    private bool heartbeatActive = false;
    private bool ambientActive = false;
    
    void Start()
    {
        SetupAudioSources();
        InitializeAudio();
    }
    
    void SetupAudioSources()
    {
        // Setup Dullahan audio source
        if (dullahanAudioSource == null)
        {
            dullahanAudioSource = gameObject.AddComponent<AudioSource>();
        }
        SetupDullahanAudioSource();
        
        // Setup player audio source
        if (playerAudioSource == null)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            if (player != null)
            {
                playerAudioSource = player.GetComponent<AudioSource>();
                if (playerAudioSource == null)
                {
                    playerAudioSource = player.AddComponent<AudioSource>();
                }
            }
        }
        SetupPlayerAudioSource();
        
        // Setup ambient audio source
        if (ambientAudioSource == null)
        {
            ambientAudioSource = gameObject.AddComponent<AudioSource>();
        }
        SetupAmbientAudioSource();
        
        // Setup effect audio source
        if (effectAudioSource == null)
        {
            effectAudioSource = gameObject.AddComponent<AudioSource>();
        }
        SetupEffectAudioSource();
    }
    
    void SetupDullahanAudioSource()
    {
        if (dullahanAudioSource == null) return;
        
        dullahanAudioSource.spatialBlend = 1f; // 3D sound
        dullahanAudioSource.rolloffMode = AudioRolloffMode.Linear;
        dullahanAudioSource.maxDistance = dullahanMaxDistance;
        dullahanAudioSource.minDistance = dullahanMinDistance;
        dullahanAudioSource.volume = chaseVolume * masterVolume;
        dullahanAudioSource.loop = false;
    }
    
    void SetupPlayerAudioSource()
    {
        if (playerAudioSource == null) return;
        
        playerAudioSource.spatialBlend = 0f; // 2D sound
        playerAudioSource.volume = effectVolume * masterVolume;
        playerAudioSource.loop = false;
    }
    
    void SetupAmbientAudioSource()
    {
        if (ambientAudioSource == null) return;
        
        ambientAudioSource.spatialBlend = 0f; // 2D sound
        ambientAudioSource.volume = ambientVolume * masterVolume;
        ambientAudioSource.loop = true;
    }
    
    void SetupEffectAudioSource()
    {
        if (effectAudioSource == null) return;
        
        effectAudioSource.spatialBlend = 0f; // 2D sound
        effectAudioSource.volume = effectVolume * masterVolume;
        effectAudioSource.loop = false;
    }
    
    void InitializeAudio()
    {
        // Start ambient audio
        if (dullahanAmbientSound != null)
        {
            PlayAmbientAudio(dullahanAmbientSound);
        }
    }
    
    #region Chase Audio Methods
    
    public void StartChase()
    {
        if (isChasing) return;
        
        isChasing = true;
        
        // Play chase start sound
        if (chaseStartSound != null)
        {
            PlayDullahanAudio(chaseStartSound);
        }
        
        // Start chase loop
        if (chaseLoopSound != null)
        {
            StartCoroutine(StartChaseLoop());
        }
        
        // Start heartbeat
        StartHeartbeat();
        
        Debug.Log("Dullahan chase audio started");
    }
    
    public void EndChase()
    {
        if (!isChasing) return;
        
        isChasing = false;
        
        // Play chase end sound
        if (chaseEndSound != null)
        {
            PlayDullahanAudio(chaseEndSound);
        }
        
        // Stop chase loop
        StopChaseLoop();
        
        // Stop heartbeat
        StopHeartbeat();
        
        // Reset intensity
        SetChaseIntensity(0f);
        
        Debug.Log("Dullahan chase audio ended");
    }
    
    public void SetChaseIntensity(float intensity)
    {
        currentChaseIntensity = Mathf.Clamp01(intensity);
        
        // Update audio based on intensity
        UpdateChaseAudio();
        UpdateHeartbeatAudio();
        UpdateFootstepAudio();
    }
    
    void UpdateChaseAudio()
    {
        if (!isChasing) return;
        
        // Determine intensity level (0-3)
        int newIntensityLevel = Mathf.FloorToInt(currentChaseIntensity * 3f);
        
        if (newIntensityLevel != currentIntensityLevel && chaseIntensityClips.Length > newIntensityLevel)
        {
            currentIntensityLevel = newIntensityLevel;
            
            // Crossfade to new intensity clip
            if (chaseIntensityClips[newIntensityLevel] != null)
            {
                StartCoroutine(CrossfadeChaseAudio(chaseIntensityClips[newIntensityLevel]));
            }
        }
        
        // Update volume and pitch based on intensity
        if (dullahanAudioSource != null)
        {
            float targetVolume = chaseVolume * masterVolume * (0.5f + currentChaseIntensity * 0.5f);
            float targetPitch = 1f + (currentChaseIntensity * 0.3f);
            
            dullahanAudioSource.volume = Mathf.Lerp(dullahanAudioSource.volume, targetVolume, Time.deltaTime * 2f);
            dullahanAudioSource.pitch = Mathf.Lerp(dullahanAudioSource.pitch, targetPitch, Time.deltaTime * 2f);
        }
    }
    
    void UpdateHeartbeatAudio()
    {
        if (!heartbeatActive || heartbeatClips.Length == 0) return;
        
        // Select heartbeat clip based on intensity
        int heartbeatIndex = Mathf.FloorToInt(currentChaseIntensity * (heartbeatClips.Length - 1));
        heartbeatIndex = Mathf.Clamp(heartbeatIndex, 0, heartbeatClips.Length - 1);
        
        if (heartbeatClips[heartbeatIndex] != null && playerAudioSource != null)
        {
            // Update heartbeat volume and pitch
            float targetVolume = effectVolume * masterVolume * currentChaseIntensity;
            float targetPitch = 0.8f + (currentChaseIntensity * 0.4f);
            
            playerAudioSource.volume = Mathf.Lerp(playerAudioSource.volume, targetVolume, Time.deltaTime * 3f);
            playerAudioSource.pitch = Mathf.Lerp(playerAudioSource.pitch, targetPitch, Time.deltaTime * 3f);
        }
    }
    
    void UpdateFootstepAudio()
    {
        if (!isChasing || dullahanFootsteps.Length == 0) return;
        
        // Play footsteps based on intensity
        int footstepIndex = Mathf.FloorToInt(currentChaseIntensity * (dullahanFootsteps.Length - 1));
        footstepIndex = Mathf.Clamp(footstepIndex, 0, dullahanFootsteps.Length - 1);
        
        if (dullahanFootsteps[footstepIndex] != null)
        {
            // Play footsteps at intervals based on intensity
            float footstepInterval = 1f - (currentChaseIntensity * 0.7f); // Faster footsteps at higher intensity
            StartCoroutine(PlayFootstepsAtInterval(footstepInterval, dullahanFootsteps[footstepIndex]));
        }
    }
    
    void StartHeartbeat()
    {
        if (heartbeatActive || heartbeatClips.Length == 0) return;
        
        heartbeatActive = true;
        
        if (playerAudioSource != null && heartbeatClips[0] != null)
        {
            playerAudioSource.clip = heartbeatClips[0];
            playerAudioSource.loop = true;
            playerAudioSource.volume = 0f;
            playerAudioSource.Play();
        }
    }
    
    void StopHeartbeat()
    {
        if (!heartbeatActive) return;
        
        heartbeatActive = false;
        
        if (playerAudioSource != null)
        {
            StartCoroutine(FadeOutAudio(playerAudioSource, heartbeatFadeTime));
        }
    }
    
    IEnumerator StartChaseLoop()
    {
        if (dullahanAudioSource == null || chaseLoopSound == null) yield break;
        
        yield return new WaitForSeconds(chaseStartSound != null ? chaseStartSound.length : 1f);
        
        dullahanAudioSource.clip = chaseLoopSound;
        dullahanAudioSource.loop = true;
        dullahanAudioSource.Play();
    }
    
    void StopChaseLoop()
    {
        if (dullahanAudioSource != null)
        {
            StartCoroutine(FadeOutAudio(dullahanAudioSource, crossfadeTime));
        }
    }
    
    IEnumerator CrossfadeChaseAudio(AudioClip newClip)
    {
        if (dullahanAudioSource == null || newClip == null) yield break;
        
        AudioSource newSource = gameObject.AddComponent<AudioSource>();
        newSource.clip = newClip;
        newSource.volume = 0f;
        newSource.loop = true;
        newSource.spatialBlend = 1f;
        newSource.rolloffMode = AudioRolloffMode.Linear;
        newSource.maxDistance = dullahanMaxDistance;
        newSource.minDistance = dullahanMinDistance;
        newSource.Play();
        
        float elapsed = 0f;
        float originalVolume = dullahanAudioSource.volume;
        
        while (elapsed < crossfadeTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / crossfadeTime;
            
            newSource.volume = Mathf.Lerp(0f, chaseVolume * masterVolume, t);
            dullahanAudioSource.volume = Mathf.Lerp(originalVolume, 0f, t);
            
            yield return null;
        }
        
        // Replace old source with new one
        Destroy(dullahanAudioSource);
        dullahanAudioSource = newSource;
    }
    
    IEnumerator PlayFootstepsAtInterval(float interval, AudioClip footstepClip)
    {
        if (dullahanAudioSource == null || footstepClip == null) yield break;
        
        while (isChasing)
        {
            dullahanAudioSource.PlayOneShot(footstepClip, 0.3f);
            yield return new WaitForSeconds(interval);
        }
    }
    
    #endregion
    
    #region Head Puzzle Audio Methods
    
    public void PlayHeadPickupSound(HeadType headType)
    {
        AudioClip clipToPlay = headPickupSound;
        
        switch (headType)
        {
            case HeadType.Real:
                clipToPlay = realHeadPickupSound != null ? realHeadPickupSound : headPickupSound;
                break;
            case HeadType.Fake1:
            case HeadType.Fake2:
                clipToPlay = fakeHeadPickupSound != null ? fakeHeadPickupSound : headPickupSound;
                break;
        }
        
        PlayEffectAudio(clipToPlay);
    }
    
    public void PlayHeadDropSound()
    {
        PlayEffectAudio(headDropSound);
    }
    
    public void PlayWrongHeadSound()
    {
        PlayEffectAudio(wrongHeadSound);
    }
    
    public void PlayHeadEffectSound()
    {
        PlayEffectAudio(headEffectSound);
    }
    
    public void PlayHeadGlowSound()
    {
        PlayEffectAudio(headGlowSound);
    }
    
    public void PlayHeadAttachSound()
    {
        PlayEffectAudio(headAttachSound);
    }
    
    public void PlayBodyActivateSound()
    {
        PlayEffectAudio(bodyActivateSound);
    }
    
    public void PlayDoorUnlockSound()
    {
        PlayEffectAudio(doorUnlockSound);
    }
    
    public void PlayPuzzleCompleteSound()
    {
        PlayEffectAudio(puzzleCompleteSound);
    }
    
    #endregion
    
    #region Effect Audio Methods
    
    public void PlaySpeedBoostSound()
    {
        PlayEffectAudio(speedBoostSound);
    }
    
    public void PlaySpeedDebuffSound()
    {
        PlayEffectAudio(speedDebuffSound);
    }
    
    public void PlayVisionBoostSound()
    {
        PlayEffectAudio(visionBoostSound);
    }
    
    public void PlayVisionDebuffSound()
    {
        PlayEffectAudio(visionDebuffSound);
    }
    
    public void PlayFearEffectSound()
    {
        PlayEffectAudio(fearEffectSound);
    }
    
    public void PlayCalmEffectSound()
    {
        PlayEffectAudio(calmEffectSound);
    }
    
    #endregion
    
    #region Ambient Audio Methods
    
    public void StartPuzzleAmbient()
    {
        if (puzzleActive) return;
        
        puzzleActive = true;
        
        if (puzzleAmbientSound != null)
        {
            PlayAmbientAudio(puzzleAmbientSound);
        }
    }
    
    public void StopPuzzleAmbient()
    {
        if (!puzzleActive) return;
        
        puzzleActive = false;
        
        if (ambientAudioSource != null)
        {
            StartCoroutine(FadeOutAudio(ambientAudioSource, crossfadeTime));
        }
    }
    
    public void PlayAmbientTension(float intensity)
    {
        if (ambientTensionClips.Length == 0) return;
        
        int clipIndex = Mathf.FloorToInt(intensity * (ambientTensionClips.Length - 1));
        clipIndex = Mathf.Clamp(clipIndex, 0, ambientTensionClips.Length - 1);
        
        if (ambientTensionClips[clipIndex] != null)
        {
            PlayAmbientAudio(ambientTensionClips[clipIndex]);
        }
    }
    
    void PlayAmbientAudio(AudioClip clip)
    {
        if (ambientAudioSource == null || clip == null) return;
        
        if (ambientAudioSource.isPlaying)
        {
            StartCoroutine(CrossfadeAmbientAudio(clip));
        }
        else
        {
            ambientAudioSource.clip = clip;
            ambientAudioSource.volume = ambientVolume * masterVolume;
            ambientAudioSource.Play();
        }
    }
    
    IEnumerator CrossfadeAmbientAudio(AudioClip newClip)
    {
        if (ambientAudioSource == null || newClip == null) yield break;
        
        AudioSource newSource = gameObject.AddComponent<AudioSource>();
        newSource.clip = newClip;
        newSource.volume = 0f;
        newSource.loop = true;
        newSource.spatialBlend = 0f;
        newSource.Play();
        
        float elapsed = 0f;
        float originalVolume = ambientAudioSource.volume;
        
        while (elapsed < crossfadeTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / crossfadeTime;
            
            newSource.volume = Mathf.Lerp(0f, ambientVolume * masterVolume, t);
            ambientAudioSource.volume = Mathf.Lerp(originalVolume, 0f, t);
            
            yield return null;
        }
        
        // Replace old source with new one
        Destroy(ambientAudioSource);
        ambientAudioSource = newSource;
    }
    
    #endregion
    
    #region Utility Audio Methods
    
    void PlayDullahanAudio(AudioClip clip)
    {
        if (dullahanAudioSource != null && clip != null)
        {
            dullahanAudioSource.PlayOneShot(clip);
        }
    }
    
    void PlayEffectAudio(AudioClip clip)
    {
        if (effectAudioSource != null && clip != null)
        {
            effectAudioSource.PlayOneShot(clip);
        }
    }
    
    IEnumerator FadeOutAudio(AudioSource source, float fadeTime)
    {
        if (source == null) yield break;
        
        float originalVolume = source.volume;
        float elapsed = 0f;
        
        while (elapsed < fadeTime)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / fadeTime;
            
            source.volume = Mathf.Lerp(originalVolume, 0f, t);
            yield return null;
        }
        
        source.Stop();
        source.volume = originalVolume;
    }
    
    #endregion
    
    #region Public Control Methods
    
    public void SetMasterVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        UpdateAllVolumes();
    }
    
    public void SetChaseVolume(float volume)
    {
        chaseVolume = Mathf.Clamp01(volume);
        if (dullahanAudioSource != null)
        {
            dullahanAudioSource.volume = chaseVolume * masterVolume;
        }
    }
    
    public void SetEffectVolume(float volume)
    {
        effectVolume = Mathf.Clamp01(volume);
        if (playerAudioSource != null)
        {
            playerAudioSource.volume = effectVolume * masterVolume;
        }
        if (effectAudioSource != null)
        {
            effectAudioSource.volume = effectVolume * masterVolume;
        }
    }
    
    public void SetAmbientVolume(float volume)
    {
        ambientVolume = Mathf.Clamp01(volume);
        if (ambientAudioSource != null)
        {
            ambientAudioSource.volume = ambientVolume * masterVolume;
        }
    }
    
    void UpdateAllVolumes()
    {
        SetChaseVolume(chaseVolume);
        SetEffectVolume(effectVolume);
        SetAmbientVolume(ambientVolume);
    }
    
    public void StopAllAudio()
    {
        if (dullahanAudioSource != null) dullahanAudioSource.Stop();
        if (playerAudioSource != null) playerAudioSource.Stop();
        if (ambientAudioSource != null) ambientAudioSource.Stop();
        if (effectAudioSource != null) effectAudioSource.Stop();
    }
    
    #endregion
}
