using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadGameScript : MonoBehaviour
{
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
        // SceneManager.LoadScene("SavedGameScene"); // Commented out for debugging
        Debug.Log("Load Game function called!");
    }
}
