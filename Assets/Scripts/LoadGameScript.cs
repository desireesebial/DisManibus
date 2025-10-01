using UnityEngine;

public class LoadGameScript : MonoBehaviour
{
    [SerializeField] private SaveLoad.SaveManager saveManager;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Call this method to load a saved game
    public void LoadGame()
    {
        if (saveManager == null)
        {
            saveManager = FindAnyObjectByType<SaveLoad.SaveManager>();
        }

        if (saveManager == null)
        {
            Debug.LogError("LoadGameScript could not find a SaveManager in the scene.");
            return;
        }
        saveManager.LoadGame();
    }
}
