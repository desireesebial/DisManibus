using UnityEngine;

[CreateAssetMenu(fileName = "Lantern", menuName = "Scriptable Objects/Lantern")]
public class LanternSO : ScriptableObject
{
    [Header("Lantern Properties")]
    public string lanternName = "Mysterious Lantern";
    public int lanternID; // Unique identifier
    public Sprite lanternSprite;
    public Sprite lanternIcon; // Icon for inventory display
    public string description = "An old lantern that provides light in the darkness.";
    
    [Header("Light Settings")]
    public Color lightColor = Color.yellow;
    public float lightIntensity = 1f;
    public float lightRange = 5f;
    public bool flickerEffect = false;
    public float flickerSpeed = 2f;
    public float flickerAmount = 0.1f;
    
    [Header("Audio")]
    public AudioClip pickupSound;
    public AudioClip toggleOnSound;
    public AudioClip toggleOffSound;
    
    [Header("Visual Effects")]
    public GameObject lanternPrefab;
    public Material lanternMaterial;
    public Color lanternGlowColor = Color.yellow;
    public bool hasGlowEffect = false;
    public ParticleSystem lanternParticles;
    
    [Header("Controls")]
    public KeyCode toggleKey = KeyCode.L;
    public string toggleMessage = "Press L to toggle lantern";
}
