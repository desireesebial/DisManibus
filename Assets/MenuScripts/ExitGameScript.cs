using UnityEngine;

public class ExitGameScript : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    // Call this method to exit the game
    public void ExitGame()
    {
        // Application.Quit(); // Commented out for debugging
        Debug.Log("Exit Game function called!");
    }
}
