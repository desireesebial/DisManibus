using UnityEngine;

public class KeyPickable : MonoBehaviour, IPickable
{
    public KeySO keyData;
    public bool isPickedUp = false;

    public void PickItem()
    {
        if (isPickedUp) return;
        isPickedUp = true;
        Destroy(gameObject);
    }
}



