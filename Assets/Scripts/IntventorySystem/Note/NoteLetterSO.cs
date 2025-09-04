using UnityEngine;
using TMPro;

[CreateAssetMenu(fileName = "New Note", menuName = "Game/Note Letter")]
public class NoteLetterSO : ScriptableObject
{
    [Header("Note Information")]
    public string noteTitle = "Untitled Note";
    [TextArea(5, 15)]
    public string noteContent = "This is the content of the note...";
    
    [Header("Visual Settings")]
    public Sprite paperTexture;
    public Color textColor = Color.black;
    public TMP_FontAsset textFont;
    
    [Header("Audio")]
    public AudioClip openSound;
    public AudioClip closeSound;
    
    [Header("Metadata")]
    public string author = "Unknown";
    public string date = "Unknown Date";
    public bool isImportant = false;
}
