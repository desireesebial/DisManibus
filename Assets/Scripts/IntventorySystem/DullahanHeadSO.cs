using UnityEngine;

[CreateAssetMenu(fileName = "DullahanHead", menuName = "Scriptable Objects/DullahanHead")]
public class DullahanHeadSO : ScriptableObject
{
    [Header("Head Properties")]
    public string headName = "Dullahan Head";
    public int headID; // Unique identifier
    public Sprite headSprite;
    public string description;
    
    [Header("Head Type")]
    public HeadType headType = HeadType.Real;
    
    [Header("Effects")]
    public bool hasEffect = false;
    public EffectType effectType = EffectType.None;
    public float effectStrength = 1f;
    public float effectDuration = 10f;
    
    [Header("Audio")]
    public AudioClip pickupSound;
    public AudioClip dropSound;
    public AudioClip effectSound;
    
    [Header("Visual Effects")]
    public GameObject headPrefab;
    public Material headMaterial;
    public Color headGlowColor = Color.white;
    public bool hasGlowEffect = false;
}

public enum HeadType
{
    Real,       // The real head that opens the door
    Fake1,      // First fake head with debuff
    Fake2       // Second fake head with buff
}

public enum EffectType
{
    None,
    SpeedBoost,     // Increases player speed
    SpeedDebuff,    // Decreases player speed
    HealthBoost,    // Increases player health
    HealthDebuff,   // Decreases player health
    VisionBoost,    // Increases FOV
    VisionDebuff,   // Decreases FOV
    StaminaBoost,   // Increases sprint duration
    StaminaDebuff,  // Decreases sprint duration
    FearEffect,     // Increases Dullahan chase intensity
    CalmEffect      // Decreases Dullahan chase intensity
}
