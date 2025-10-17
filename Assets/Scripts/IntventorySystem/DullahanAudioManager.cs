using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class DullahanAudioManager : MonoBehaviour
{
    [Header("Audio Sources")]
    public AudioSource musicSource;
    public AudioSource sfxSource;
    public AudioSource voiceSource;
    
    [Header("Music Tracks")]
    public AudioClip ambientMusic;
    public AudioClip chaseMusic;
    public AudioClip puzzleMusic;
    public AudioClip victoryMusic;
    
    [Header("Sound Effects")]
    public AudioClip footstepSound;
    public AudioClip headPickupSound;
    public AudioClip headDropSound;
    public AudioClip doorOpenSound;
    public AudioClip doorCloseSound;
    public AudioClip puzzleCompleteSound;
    public AudioClip puzzleFailSound;
    
    [Header("Voice Lines")]
    public AudioClip[] dullahanVoiceLines;
    public AudioClip[] playerVoiceLines;
    
    [Header("Audio Settings")]
    public float musicVolume = 0.7f;
    public float sfxVolume = 0.8f;
    public float voiceVolume = 0.9f;
    public float fadeTime = 2f;
    
    [Header("Dynamic Audio")]
    public bool enableDynamicAudio = true;
    public float chaseIntensityThreshold = 0.5f;
    public float musicTransitionSpeed = 1f;
    
    // Private variables
    private AudioClip currentMusic;
    private Coroutine musicFadeCoroutine;
    private Dictionary<string, AudioClip> audioClips = new Dictionary<string, AudioClip>();
    private bool isChasing = false;
    private float currentChaseIntensity = 0f;
    
    void Start()
    {
        InitializeAudioSources();
        LoadAudioClips();
        PlayAmbientMusic();
    }
    
    void InitializeAudioSources()
    {
        // Create audio sources if they don't exist
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
            musicSource.loop = true;
            musicSource.volume = musicVolume;
        }
        
        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.loop = false;
            sfxSource.volume = sfxVolume;
        }
        
        if (voiceSource == null)
        {
            voiceSource = gameObject.AddComponent<AudioSource>();
            voiceSource.loop = false;
            voiceSource.volume = voiceVolume;
        }
    }
    
    void LoadAudioClips()
    {
        // Load music tracks
        if (ambientMusic != null) audioClips["ambient"] = ambientMusic;
        if (chaseMusic != null) audioClips["chase"] = chaseMusic;
        if (puzzleMusic != null) audioClips["puzzle"] = puzzleMusic;
        if (victoryMusic != null) audioClips["victory"] = victoryMusic;
        
        // Load sound effects
        if (footstepSound != null) audioClips["footstep"] = footstepSound;
        if (headPickupSound != null) audioClips["headPickup"] = headPickupSound;
        if (headDropSound != null) audioClips["headDrop"] = headDropSound;
        if (doorOpenSound != null) audioClips["doorOpen"] = doorOpenSound;
        if (doorCloseSound != null) audioClips["doorClose"] = doorCloseSound;
        if (puzzleCompleteSound != null) audioClips["puzzleComplete"] = puzzleCompleteSound;
        if (puzzleFailSound != null) audioClips["puzzleFail"] = puzzleFailSound;
    }
    
    public void PlayAmbientMusic()
    {
        if (ambientMusic != null)
        {
            PlayMusic(ambientMusic);
        }
    }
    
    public void PlayChaseMusic()
    {
        if (chaseMusic != null)
        {
            PlayMusic(chaseMusic);
        }
    }
    
    public void PlayPuzzleMusic()
    {
        if (puzzleMusic != null)
        {
            PlayMusic(puzzleMusic);
        }
    }
    
    public void PlayVictoryMusic()
    {
        if (victoryMusic != null)
        {
            PlayMusic(victoryMusic);
        }
    }
    
    public void PlayMusic(AudioClip clip)
    {
        if (clip == null || clip == currentMusic) return;
        
        if (musicFadeCoroutine != null)
        {
            StopCoroutine(musicFadeCoroutine);
        }
        
        musicFadeCoroutine = StartCoroutine(FadeMusic(clip));
    }
    
    IEnumerator FadeMusic(AudioClip newClip)
    {
        // Fade out current music
        if (currentMusic != null)
        {
            float startVolume = musicSource.volume;
            while (musicSource.volume > 0)
            {
                musicSource.volume -= startVolume * Time.deltaTime / fadeTime;
                yield return null;
            }
        }
        
        // Change clip
        musicSource.clip = newClip;
        currentMusic = newClip;
        
        // Fade in new music
        if (newClip != null)
        {
            musicSource.Play();
            while (musicSource.volume < musicVolume)
            {
                musicSource.volume += musicVolume * Time.deltaTime / fadeTime;
                yield return null;
            }
        }
        
        musicSource.volume = musicVolume;
    }
    
    public void PlaySFX(string clipName)
    {
        if (audioClips.ContainsKey(clipName))
        {
            PlaySFX(audioClips[clipName]);
        }
        else
        {
            Debug.LogWarning($"[DullahanAudioManager] SFX clip '{clipName}' not found");
        }
    }
    
    public void PlaySFX(AudioClip clip)
    {
        if (clip != null && sfxSource != null)
        {
            sfxSource.PlayOneShot(clip);
        }
    }
    
    public void PlayVoiceLine(string clipName)
    {
        if (audioClips.ContainsKey(clipName))
        {
            PlayVoiceLine(audioClips[clipName]);
        }
        else
        {
            Debug.LogWarning($"[DullahanAudioManager] Voice clip '{clipName}' not found");
        }
    }
    
    public void PlayVoiceLine(AudioClip clip)
    {
        if (clip != null && voiceSource != null)
        {
            voiceSource.PlayOneShot(clip);
        }
    }
    
    public void PlayHeadPickupSound(HeadType headType)
    {
        switch (headType)
        {
            case HeadType.Real:
                PlaySFX("headPickup");
                break;
            case HeadType.Fake1:
                PlaySFX("headPickup");
                break;
            case HeadType.Fake2:
                PlaySFX("headPickup");
                break;
        }
    }
    
    public void PlayHeadDropSound(HeadType headType)
    {
        switch (headType)
        {
            case HeadType.Real:
                PlaySFX("headDrop");
                break;
            case HeadType.Fake1:
                PlaySFX("headDrop");
                break;
            case HeadType.Fake2:
                PlaySFX("headDrop");
                break;
        }
    }
    
    public void PlayFootstepSound()
    {
        PlaySFX("footstep");
    }
    
    public void PlayDoorOpenSound()
    {
        PlaySFX("doorOpen");
    }
    
    public void PlayDoorCloseSound()
    {
        PlaySFX("doorClose");
    }
    
    public void PlayPuzzleCompleteSound()
    {
        PlaySFX("puzzleComplete");
    }
    
    public void PlayPuzzleFailSound()
    {
        PlaySFX("puzzleFail");
    }
    
    public void SetChaseIntensity(float intensity)
    {
        currentChaseIntensity = intensity;
        
        if (enableDynamicAudio)
        {
            if (intensity > chaseIntensityThreshold && !isChasing)
            {
                StartChase();
            }
            else if (intensity <= chaseIntensityThreshold && isChasing)
            {
                StopChase();
            }
        }
    }
    
    public void StartChase()
    {
        if (isChasing) return;
        
        Debug.Log("[DullahanAudioManager] Starting chase audio");
        isChasing = true;
        PlayChaseMusic();
    }
    
    public void StopChase()
    {
        if (!isChasing) return;
        
        Debug.Log("[DullahanAudioManager] Stopping chase audio");
        isChasing = false;
        PlayAmbientMusic();
    }
    
    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);
        if (musicSource != null)
            musicSource.volume = musicVolume;
    }
    
    public void SetSFXVolume(float volume)
    {
        sfxVolume = Mathf.Clamp01(volume);
        if (sfxSource != null)
            sfxSource.volume = sfxVolume;
    }
    
    public void SetVoiceVolume(float volume)
    {
        voiceVolume = Mathf.Clamp01(volume);
        if (voiceSource != null)
            voiceSource.volume = voiceVolume;
    }
    
    public void MuteAll()
    {
        if (musicSource != null) musicSource.volume = 0f;
        if (sfxSource != null) sfxSource.volume = 0f;
        if (voiceSource != null) voiceSource.volume = 0f;
    }
    
    public void UnmuteAll()
    {
        if (musicSource != null) musicSource.volume = musicVolume;
        if (sfxSource != null) sfxSource.volume = sfxVolume;
        if (voiceSource != null) voiceSource.volume = voiceVolume;
    }
    
    public void StopAllAudio()
    {
        if (musicSource != null) musicSource.Stop();
        if (sfxSource != null) sfxSource.Stop();
        if (voiceSource != null) voiceSource.Stop();
    }
    
    public void PauseAllAudio()
    {
        if (musicSource != null) musicSource.Pause();
        if (sfxSource != null) sfxSource.Pause();
        if (voiceSource != null) voiceSource.Pause();
    }
    
    public void ResumeAllAudio()
    {
        if (musicSource != null) musicSource.UnPause();
        if (sfxSource != null) sfxSource.UnPause();
        if (voiceSource != null) voiceSource.UnPause();
    }
    
    public bool IsPlayingMusic()
    {
        return musicSource != null && musicSource.isPlaying;
    }
    
    public bool IsPlayingSFX()
    {
        return sfxSource != null && sfxSource.isPlaying;
    }
    
    public bool IsPlayingVoice()
    {
        return voiceSource != null && voiceSource.isPlaying;
    }
    
    public AudioClip GetCurrentMusic()
    {
        return currentMusic;
    }
    
    public float GetMusicVolume()
    {
        return musicVolume;
    }
    
    public float GetSFXVolume()
    {
        return sfxVolume;
    }
    
    public float GetVoiceVolume()
    {
        return voiceVolume;
    }
    
    public bool IsChasing()
    {
        return isChasing;
    }
    
    public float GetChaseIntensity()
    {
        return currentChaseIntensity;
    }
    
    // Debug methods
    public void TestAudio()
    {
        Debug.Log("[DullahanAudioManager] Testing audio system");
        
        if (ambientMusic != null)
        {
            PlayAmbientMusic();
            Debug.Log("Playing ambient music");
        }
        
        if (headPickupSound != null)
        {
            PlaySFX(headPickupSound);
            Debug.Log("Playing head pickup sound");
        }
    }
    
    void OnValidate()
    {
        // Clamp volume values in inspector
        musicVolume = Mathf.Clamp01(musicVolume);
        sfxVolume = Mathf.Clamp01(sfxVolume);
        voiceVolume = Mathf.Clamp01(voiceVolume);
        fadeTime = Mathf.Max(0.1f, fadeTime);
        musicTransitionSpeed = Mathf.Max(0.1f, musicTransitionSpeed);
    }
}