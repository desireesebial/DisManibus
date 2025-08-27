using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance; // Optional singleton

    [Header("Options")]
    public bool useDistanceBasedAudio = false;

    [Header("Audio Sources")]
    public AudioSource mapAmbience;
    public AudioSource enemyAmbience;
    public AudioSource enemyFootsteps;
    public AudioSource chaseMusic;

    [Header("Fade Durations")]
    public float fadeOutDuration = 0.5f;
    public float fadeInDuration = 1.5f;

    [Header("Volumes")]
    [Range(0f, 1f)] public float ambienceVolume = 1f;
    [Range(0f, 1f)] public float footstepsVolume = 1f;
    [Range(0f, 1f)] public float chaseMusicVolume = 1f;

    [Header("Random Ambience")]
    public AudioSource randomAmbienceSource;
    public AudioClip[] randomAmbienceClips;
    public float minAmbienceDelay = 10f;
    public float maxAmbienceDelay = 25f;

    private bool isPlayerNearby = false;
    private bool isChasing = false;

    private Dictionary<AudioSource, Coroutine> fadeCoroutines = new Dictionary<AudioSource, Coroutine>();

    void Awake()
    {
        // Singleton setup (optional, comment out if not needed)
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // Start random ambience system
        if (randomAmbienceSource != null && randomAmbienceClips.Length > 0)
            StartCoroutine(RandomAmbienceCoroutine());
    }

    // ===== Public Calls =====

    // Called by PlayerProximity
    public void SetPlayerNearby(bool nearby)
    {
        if (!useDistanceBasedAudio && !isChasing)
        {
            if (isPlayerNearby != nearby)
            {
                isPlayerNearby = nearby;
                if (isPlayerNearby)
                {
                    FadeIn(enemyAmbience, fadeInDuration, ambienceVolume);
                    FadeIn(enemyFootsteps, fadeInDuration, footstepsVolume);
                }
                else
                {
                    FadeOut(enemyAmbience, fadeOutDuration);
                    FadeOut(enemyFootsteps, fadeOutDuration);
                }
            }
        }
    }

    // Called by ChaseTrigger
    public void StartChaseMode()
    {
        if (isChasing) return;
        isChasing = true;

        if (chaseMusic.isPlaying) chaseMusic.Stop();
        chaseMusic.volume = chaseMusicVolume;
        chaseMusic.Play();

        FadeOut(mapAmbience, fadeOutDuration);
        FadeOut(enemyAmbience, fadeOutDuration);
        FadeOut(enemyFootsteps, fadeOutDuration);
    }

    public void EndChaseMode()
    {
        if (!isChasing) return;
        isChasing = false;

        FadeOut(chaseMusic, fadeOutDuration);

        // Restore ambience
        FadeIn(mapAmbience, fadeInDuration, ambienceVolume);
        FadeIn(enemyAmbience, fadeInDuration, ambienceVolume);
        FadeIn(enemyFootsteps, fadeInDuration, footstepsVolume);
    }

    // ===== Utility =====

    public void PlayRandomized(AudioSource source, AudioClip clip, float baseVolume = 1f)
    {
        if (source == null || clip == null) return;
        source.pitch = Random.Range(0.95f, 1.05f);
        source.PlayOneShot(clip, baseVolume);
    }

    private IEnumerator FadeTo(AudioSource source, float targetVolume, float duration)
    {
        if (source == null) yield break;

        float startVolume = source.volume;
        float startTime = Time.time;

        while (Time.time < startTime + duration)
        {
            float t = (Time.time - startTime) / duration;
            source.volume = Mathf.Lerp(startVolume, targetVolume, t);
            yield return null;
        }

        source.volume = targetVolume;

        if (Mathf.Approximately(targetVolume, 0f))
            source.Stop();
    }

    private void FadeOut(AudioSource source, float duration)
    {
        if (source == null) return;

        if (fadeCoroutines.ContainsKey(source) && fadeCoroutines[source] != null)
            StopCoroutine(fadeCoroutines[source]);

        fadeCoroutines[source] = StartCoroutine(FadeTo(source, 0f, duration));
    }

    private void FadeIn(AudioSource source, float duration, float targetVolume = 1f)
    {
        if (source == null) return;

        if (!source.isPlaying)
            source.Play();

        if (fadeCoroutines.ContainsKey(source) && fadeCoroutines[source] != null)
            StopCoroutine(fadeCoroutines[source]);

        fadeCoroutines[source] = StartCoroutine(FadeTo(source, targetVolume, duration));
    }

    // ===== Random Ambience =====

    private IEnumerator RandomAmbienceCoroutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(minAmbienceDelay, maxAmbienceDelay));
            if (randomAmbienceClips.Length > 0)
            {
                AudioClip clip = randomAmbienceClips[Random.Range(0, randomAmbienceClips.Length)];
                PlayRandomized(randomAmbienceSource, clip, 0.5f);
            }
        }
    }
}
