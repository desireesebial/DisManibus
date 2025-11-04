using UnityEngine;
using UnityEngine.Audio;

/// <summary>
/// Automatically routes AudioManager's AudioSources through the MasterMixer.
/// Attach this component to the same GameObject as AudioManager.
/// This ensures professional volume control through the mixer instead of direct volume manipulation.
/// </summary>
[RequireComponent(typeof(AudioManager))]
public class AudioManagerMixerRouter : MonoBehaviour
{
    [Header("Mixer Configuration")]
    [Tooltip("The Master Mixer to route audio through")]
    [SerializeField] private AudioMixer masterMixer;

    [Tooltip("Name of the mixer group to route to (default: Master)")]
    [SerializeField] private string mixerGroupName = "Master";

    private AudioManager audioManager;
    private AudioMixerGroup masterGroup;

    private void Awake()
    {
        audioManager = GetComponent<AudioManager>();

        if (masterMixer == null)
        {
            Debug.LogWarning("[AudioManagerMixerRouter] Master Mixer not assigned! Audio won't be routed through mixer.", this);
            return;
        }

        // Find the mixer group
        AudioMixerGroup[] groups = masterMixer.FindMatchingGroups(mixerGroupName);
        if (groups.Length > 0)
        {
            masterGroup = groups[0];
            RouteAudioSourcesToMixer();
        }
        else
        {
            Debug.LogError($"[AudioManagerMixerRouter] Mixer group '{mixerGroupName}' not found in {masterMixer.name}!", this);
        }
    }

    /// <summary>
    /// Routes all AudioManager's AudioSources through the mixer group
    /// </summary>
    private void RouteAudioSourcesToMixer()
    {
        if (masterGroup == null) return;

        int routedCount = 0;

        if (audioManager.mapAmbience != null)
        {
            audioManager.mapAmbience.outputAudioMixerGroup = masterGroup;
            routedCount++;
        }

        if (audioManager.enemyAmbience != null)
        {
            audioManager.enemyAmbience.outputAudioMixerGroup = masterGroup;
            routedCount++;
        }

        if (audioManager.enemyFootsteps != null)
        {
            audioManager.enemyFootsteps.outputAudioMixerGroup = masterGroup;
            routedCount++;
        }

        if (audioManager.chaseMusic != null)
        {
            audioManager.chaseMusic.outputAudioMixerGroup = masterGroup;
            routedCount++;
        }

        if (audioManager.randomAmbienceSource != null)
        {
            audioManager.randomAmbienceSource.outputAudioMixerGroup = masterGroup;
            routedCount++;
        }

        Debug.Log($"[AudioManagerMixerRouter] Successfully routed {routedCount} AudioSources to '{mixerGroupName}' mixer group.");
    }

    #if UNITY_EDITOR
    /// <summary>
    /// Validates configuration in the Unity Editor
    /// </summary>
    private void OnValidate()
    {
        if (masterMixer == null)
        {
            // Try to find MasterMixer automatically
            string[] guids = UnityEditor.AssetDatabase.FindAssets("t:AudioMixer MasterMixer");
            if (guids.Length > 0)
            {
                string path = UnityEditor.AssetDatabase.GUIDToAssetPath(guids[0]);
                masterMixer = UnityEditor.AssetDatabase.LoadAssetAtPath<AudioMixer>(path);
                Debug.Log($"[AudioManagerMixerRouter] Auto-assigned MasterMixer from {path}");
            }
        }
    }
    #endif
}
