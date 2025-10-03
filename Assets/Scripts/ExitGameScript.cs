using UnityEngine;

public class ExitGameScript : MonoBehaviour
{
    public void ExitGame()
    {
        Debug.Log("Exit Game triggered.");

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
