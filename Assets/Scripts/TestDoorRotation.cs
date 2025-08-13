using UnityEngine;

public class TestDoorRotation : MonoBehaviour
{
    void Update()
    {
        // Test rotation with arrow keys
        if (Input.GetKey(KeyCode.UpArrow))
        {
            transform.Rotate(0, 1, 0); // Rotate Y axis
        }
        
        if (Input.GetKey(KeyCode.DownArrow))
        {
            transform.Rotate(0, -1, 0); // Rotate Y axis opposite
        }
        
        // Test with number keys
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            transform.rotation = Quaternion.Euler(0, 0, 0); // Reset rotation
            Debug.Log("Door rotation reset to 0");
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            transform.rotation = Quaternion.Euler(0, 90, 0); // Set to 90 degrees
            Debug.Log("Door rotation set to 90");
        }
        
        if (Input.GetKeyDown(KeyCode.Alpha3))
        {
            transform.rotation = Quaternion.Euler(0, 180, 0); // Set to 180 degrees
            Debug.Log("Door rotation set to 180");
        }
        
        // Show current rotation
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("Current door rotation: " + transform.rotation.eulerAngles);
        }
    }
}
